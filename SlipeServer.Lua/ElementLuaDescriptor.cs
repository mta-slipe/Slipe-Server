// ElementLuaDescriptor

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Lua;

internal class ElementLuaDescriptor : StandardUserDataDescriptor
{
    // Side-table for arbitrary Lua fields set on element userdata (e.g. SAES OOP pattern)
    private static readonly ConditionalWeakTable<Element, Dictionary<string, DynValue>> extraFields = new();

    // Per-runtime-type descriptors for subclass method lookup
    private static readonly ConcurrentDictionary<Type, StandardUserDataDescriptor> runtimeDescriptors = new();

    private static StandardUserDataDescriptor GetRuntimeDescriptor(Type type)
        => runtimeDescriptors.GetOrAdd(type, t => new StandardUserDataDescriptor(t, InteropAccessMode.Default, null));

    public ElementLuaDescriptor() : base(typeof(Element), InteropAccessMode.Default, null) { }

    public override DynValue Index(Script script, object obj, DynValue index, bool isNameIndex)
    {
        // Check side-table first — Lua-level assignments shadow C# properties (SAES OOP pattern)
        if (isNameIndex && index.Type == DataType.String && obj is Element elemSideTable)
        {
            if (extraFields.TryGetValue(elemSideTable, out var sideDict) && sideDict.TryGetValue(index.String, out var sideVal))
                return sideVal;
        }

        // Intercept ElementId fields before base.Index to avoid MoonSharp throwing on the unregistered struct
        if (isNameIndex && index.Type == DataType.String && obj is Element elemForId)
        {
            var fieldName = index.String;
            var candidate = fieldName.Length > 0 && char.IsLower(fieldName[0])
                ? char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1)
                : fieldName;

            if (candidate == "Id")
                return DynValue.NewNumber(elemForId.Id.Value);
        }

        // MTA OOP verb-prefixed method globals MUST be checked BEFORE base.Index, because
        // base.Index resolves e.g. "getData" to the CLR Element.GetData member which returns
        // a raw LuaValue that Lua cannot consume.  By intercepting here, we return a thin
        // callback that routes through the scripting global (and its proper ToDynValues path).
        if (isNameIndex && index.Type == DataType.String && obj is Element oopEarlyElem)
        {
            var n = index.String;
            if (n.Length > 0 && char.IsLower(n[0]) && HasOopVerbPrefix(n))
            {
                var oopGlobal = ResolveMtaOopGlobal(script, oopEarlyElem, n);
                if (oopGlobal.Type != DataType.Nil)
                {
                    var capturedFn = oopGlobal;
                    var capturedScript = script;
                    return DynValue.NewCallback((ctx, args) =>
                        capturedScript.Call(capturedFn, args.GetArray()));
                }
            }
        }

        DynValue result;
        try
        {
            result = base.Index(script, obj, index, isNameIndex);
        }
        catch
        {
            result = DynValue.Nil;
        }

        if (result != null && result.Type != DataType.Nil)
        {
            // Convert ElementId struct → Lua number so Lua scripts can use it directly
            if (result.Type == DataType.UserData && result.UserData?.Object is ElementId eid)
                return DynValue.NewNumber(eid.Value);
            return result;
        }

        if (!isNameIndex || index.Type != DataType.String)
            return DynValue.Nil;

        var name = index.String;

        if (name == "vehicleType" && obj is Vehicle vehicle)
            return DynValue.NewString(VehicleTypeToString(vehicle.VehicleType));

        if (name.Length > 0 && char.IsLower(name[0]))
        {
            var pascal = char.ToUpperInvariant(name[0]) + name.Substring(1);
            var r = base.Index(script, obj, DynValue.NewString(pascal), true);
            if (r != null && r.Type != DataType.Nil)
                return r;

            // Also try on the actual runtime type (for subclass members like Ped.SetStat)
            var runtimeType = obj.GetType();
            if (runtimeType != typeof(Element))
            {
                var rd = GetRuntimeDescriptor(runtimeType);
                try
                {
                    var rr = rd.Index(script, obj, DynValue.NewString(pascal), true);
                    if (rr != null && rr.Type != DataType.Nil)
                        return rr;
                }
                catch { }
            }
        }

        // SAES OOP fallback: check Element_index[element][key] in Lua globals
        if (obj is Element elemOop && isNameIndex && index.Type == DataType.String)
        {
            var elementIndex = script.Globals.Get("Element_index");
            if (elementIndex.Type == DataType.Table)
            {
                var self = UserData.Create(elemOop);
                var classTable = elementIndex.Table.Get(self);
                if (classTable.Type == DataType.Table)
                {
                    var method = classTable.Table.Get(index.String);
                    if (method.Type != DataType.Nil)
                        return method;
                }
            }
        }

        // MTA OOP property fallback: map element.type → getElementType(element), etc.
        if (obj is Element propElem && isNameIndex && index.Type == DataType.String)
        {
            var propName = index.String;
            var pascal = propName.Length > 0 && char.IsLower(propName[0])
                ? char.ToUpperInvariant(propName[0]) + propName.Substring(1)
                : propName;
            var getter = script.Globals.Get("getElement" + pascal);
            if (getter.Type == DataType.Function || getter.Type == DataType.ClrFunction)
            {
                try
                {
                    var r = script.Call(getter, UserData.Create(propElem));
                    if (r != null && r.Type != DataType.Nil)
                        return r;
                }
                catch { }
            }
        }

        // MTA OOP method fallback: map element:setXxx(...) → setElementXxx(element, ...)
        // and element:getXxx() → getElementXxx(element), etc.
        if (obj is Element mtaElem && isNameIndex && index.Type == DataType.String)
        {
            var methodName = index.String;
            var globalFunc = ResolveMtaOopGlobal(script, mtaElem, methodName);
            if (globalFunc.Type != DataType.Nil)
            {
                var capturedFn = globalFunc;
                var capturedScript = script;
                return DynValue.NewCallback((ctx, args) =>
                {
                    // Colon syntax already passes self as args[0]; forward args as-is.
                    return capturedScript.Call(capturedFn, args.GetArray());
                });
            }
        }

        return DynValue.Nil;
    }

    private static DynValue ResolveMtaOopGlobal(Script script, Element element, string methodName)
    {
        // Build a list of candidate global names to try, in priority order:
        // 1. setElementDimension, getElementHealth, etc.
        // 2. setPlayerNametagColor, getVehicleType, etc. (runtime-type-specific)
        // 3. methodName itself (e.g. removePedFromVehicle)

        if (methodName.Length < 2)
            return DynValue.Nil;

        var candidates = BuildOopCandidates(element, methodName);
        foreach (var candidate in candidates)
        {
            var g = script.Globals.Get(candidate);
            if (g.Type == DataType.Function || g.Type == DataType.ClrFunction)
                return g;
        }
        return DynValue.Nil;
    }

    private static readonly string[] s_oopVerbs = ["set", "get", "is", "has", "remove", "add",
        "toggle", "create", "destroy", "fire", "play", "stop", "reset", "attach", "detach",
        "bind", "unbind", "force", "give", "take", "warp", "kill", "spawn", "fade"];

    /// <summary>Returns true when <paramref name="name"/> starts with a recognised MTA OOP verb
    /// followed immediately by an uppercase letter (e.g. "getData", "setHealth").</summary>
    private static bool HasOopVerbPrefix(string name)
    {
        foreach (var v in s_oopVerbs)
            if (name.StartsWith(v, StringComparison.Ordinal) && name.Length > v.Length && char.IsUpper(name[v.Length]))
                return true;
        return false;
    }

    private static IEnumerable<string> BuildOopCandidates(Element element, string methodName)
    {
        // Determine where to insert the type word:
        // "setDimension"  → verb="set", rest="Dimension"
        // "getData"       → verb="get", rest="Data"
        // "isElement"     → verb="is",  rest="Element"
        string[] verbs = ["set", "get", "is", "has", "remove", "add", "toggle", "create", "destroy", "fire", "play", "stop", "reset", "attach", "detach", "bind", "unbind", "force", "give", "take", "warp", "kill", "spawn", "fade"];
        string? verb = null;
        string rest = methodName;

        foreach (var v in verbs)
        {
            if (methodName.StartsWith(v, StringComparison.Ordinal) && methodName.Length > v.Length
                && char.IsUpper(methodName[v.Length]))
            {
                verb = v;
                rest = methodName.Substring(v.Length);
                break;
            }
        }

        if (verb != null)
        {
            // Priority: Element, then runtime element type name (Vehicle, Player, Ped, Marker, etc.)
            yield return verb + "Element" + rest;

            var typeName = GetElementTypeName(element);
            if (typeName != null && typeName != "Element")
                yield return verb + typeName + rest;
        }

        // Final fallback: the method name as-is (e.g. removePedFromVehicle)
        yield return methodName;
    }

    private static string? GetElementTypeName(Element element) => element switch
    {
        Player => "Player",
        Ped => "Ped",
        Vehicle => "Vehicle",
        WorldObject => "Object",
        SlipeServer.Server.Elements.Marker => "Marker",
        SlipeServer.Server.Elements.Team => "Team",
        SlipeServer.Server.Elements.Blip => "Blip",
        _ => null
    };


    public override bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isNameIndex)
    {
        if (!isNameIndex || index.Type != DataType.String)
            return base.SetIndex(script, obj, index, value, isNameIndex);

        var name = index.String;

        // Resolve to PascalCase upfront so every path below uses the correct member name.
        var memberName = name.Length > 0 && char.IsLower(name[0])
            ? char.ToUpperInvariant(name[0]) + name.Substring(1)
            : name;

        // Intercept Lua Vector3 table → System.Numerics.Vector3 before base can attempt
        // an automatic conversion (which throws rather than returning false).
        if (value.Type == DataType.Table && IsVector3Table(value.Table))
        {
            var prop = obj.GetType().GetProperty(memberName);
            if (prop?.PropertyType == typeof(Vector3) && prop.CanWrite)
            {
                prop.SetValue(obj, TableToVector3(value.Table));
                return true;
            }
        }

        // Intercept Lua number → ElementId before base can throw on incompatible conversion.
        if (value.Type == DataType.Number && obj is Element elemIdCheck)
        {
            var prop = elemIdCheck.GetType().GetProperty(memberName);
            if (prop?.PropertyType == typeof(ElementId) && prop.CanWrite)
            {
                prop.SetValue(elemIdCheck, new ElementId { Value = (uint)value.Number });
                return true;
            }
        }

        bool set;
        try
        {
            set = base.SetIndex(script, obj, DynValue.NewString(memberName), value, true);
        }
        catch
        {
            set = false;
        }
        if (set)
            return true;

        // No C# property found — store in side-table so arbitrary Lua fields work (SAES OOP pattern)
        if (obj is Element element)
        {
            var dict = extraFields.GetOrCreateValue(element);
            dict[name] = value;
            return true;
        }

        return false;
    }

    private static bool IsVector3Table(Table t)
        => t.Get("x").Type == DataType.Number || t.Get("y").Type == DataType.Number || t.Get("z").Type == DataType.Number;

    private static Vector3 TableToVector3(Table t)
        => new((float)t.Get("x").Number, (float)t.Get("y").Number, (float)t.Get("z").Number);

    private static string VehicleTypeToString(VehicleType type) => type switch
    {
        VehicleType.Automobile => "Automobile",
        VehicleType.Plane => "Plane",
        VehicleType.Motorcycle => "Bike",
        VehicleType.Helicopter => "Helicopter",
        VehicleType.Boat => "Boat",
        VehicleType.Train => "Train",
        VehicleType.Trailer => "Trailer",
        VehicleType.Bmx => "BMX",
        VehicleType.MonsterTruck => "Monster Truck",
        VehicleType.QuadBike => "Quad Bike",
        _ => "Unknown"
    };
}
