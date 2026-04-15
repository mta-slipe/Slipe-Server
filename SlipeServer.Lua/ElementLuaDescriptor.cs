using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using SlipeServer.Server.Elements;
using System.Numerics;

namespace SlipeServer.Lua;

internal class ElementLuaDescriptor : StandardUserDataDescriptor
{
    public ElementLuaDescriptor() : base(typeof(Element), InteropAccessMode.Default, null) { }

    public override DynValue Index(Script script, object obj, DynValue index, bool isNameIndex)
    {
        var result = base.Index(script, obj, index, isNameIndex);
        if (result != null && result.Type != DataType.Nil)
            return result;

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
        }

        return DynValue.Nil;
    }


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

        return base.SetIndex(script, obj, DynValue.NewString(memberName), value, true);
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
