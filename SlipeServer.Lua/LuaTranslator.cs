using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Scripting;
using SlipeServer.Scripting.Definitions;
using SlipeServer.Server.Concepts;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using MarkerArrowProperties = SlipeServer.Scripting.Definitions.MarkerArrowProperties;

namespace SlipeServer.Lua;

public class LuaTranslator
{
    private readonly ILogger logger;
    private readonly IElementCollection elementCollection;
    private readonly ConditionalWeakTable<Closure, EventDelegate> eventDelegateCache = [];
    private readonly ConditionalWeakTable<Script, LuaEnvironment> scriptEnvironments = [];

    internal void RegisterEnvironment(Script script, LuaEnvironment environment) => this.scriptEnvironments.Add(script, environment);
    private LuaEnvironment? GetEnvironment(Script script) => this.scriptEnvironments.TryGetValue(script, out var env) ? env : null;

    private static void RestoreGlobal(Script script, string key, DynValue? previousValue)
    {
        if (previousValue == null || previousValue.IsNil())
            script.Globals.Remove(key);
        else
            script.Globals[key] = previousValue;
    }

    public LuaTranslator(ILogger logger, IElementCollection elementCollection)
    {
        this.elementCollection = elementCollection;
        this.logger = logger;

        UserData.RegisterType<Element>(new ElementLuaDescriptor());
        UserData.RegisterType<Resource>(InteropAccessMode.HideMembers);
        UserData.RegisterType<ScriptFile>(InteropAccessMode.Hardwired);
        UserData.RegisterType<ScriptXmlNode>(InteropAccessMode.Hardwired);
        UserData.RegisterType<AclEntry>(InteropAccessMode.Hardwired);
        UserData.RegisterType<AclGroup>(InteropAccessMode.Hardwired);
        UserData.RegisterType<DbConnectionHandle>(InteropAccessMode.Hardwired);
        UserData.RegisterType<DbQueryHandle>(InteropAccessMode.Hardwired);
        UserData.RegisterType<AccountHandle>(InteropAccessMode.Hardwired);
        UserData.RegisterType<TextItem>(InteropAccessMode.Hardwired);
        UserData.RegisterType<TextDisplay>(InteropAccessMode.Hardwired);
        UserData.RegisterType<ScriptTimer>(InteropAccessMode.Hardwired);
    }

    public IEnumerable<DynValue> ToDynValues(object? obj)
    {
        if (obj == null)
            return [DynValue.Nil];
        if (obj is ScriptTimer scriptTimer)
            return [UserData.Create(scriptTimer)];
        if (obj is ScriptTimerDetails td)
            return [DynValue.NewNumber(td.Remaining), DynValue.NewNumber(td.ExecutesRemaining), DynValue.NewNumber(td.Interval)];
        if (obj is KeyPairResult kp)
            return [DynValue.NewString(kp.PublicKey), DynValue.NewString(kp.PrivateKey)];
        if (obj is PerformanceStatsResult ps)
            return [.. ToDynValues(ps.Columns), .. ToDynValues(ps.Rows)];
        if (obj is DbConnectionHandle dbConnection)
            return [UserData.Create(dbConnection)];
        if (obj is DbQueryHandle dbQueryHandle)
            return [UserData.Create(dbQueryHandle)];
        if (obj is AccountHandle accountHandle)
            return [UserData.Create(accountHandle)];
        if (obj is TextItem textItem)
            return [UserData.Create(textItem)];
        if (obj is TextDisplay textDisplay)
            return [UserData.Create(textDisplay)];
        if (obj is DbQueryResult queryResult)
        {
            var rowsTable = new Table(null);
            for (int idx = 0; idx < queryResult.Rows.Count; idx++)
            {
                var rowTable = new Table(null);
                foreach (var pair in queryResult.Rows[idx])
                    rowTable.Set(pair.Key, pair.Value == null ? DynValue.Nil : ToDynValues(pair.Value).First());
                rowsTable.Set(idx + 1, DynValue.NewTable(rowTable));
            }
            return [DynValue.NewTable(rowsTable), DynValue.NewNumber(queryResult.AffectedRows), DynValue.NewNumber(queryResult.LastInsertId)];
        }
        if (obj is ScriptXmlNode xmlNode)
            return [UserData.Create(xmlNode)];
        if (obj is ScriptFile scriptFile)
            return [UserData.Create(scriptFile)];
        if (obj is AclEntry aclEntry)
            return [UserData.Create(aclEntry)];
        if (obj is AclGroup aclGroup)
            return [UserData.Create(aclGroup)];
        if (obj is Resource resource)
            return [UserData.Create(resource)];
        if (obj is Element element)
            return [UserData.Create(element)];
        if (obj is byte int8)
            return [DynValue.NewNumber(int8)];
        if (obj is short int16)
            return [DynValue.NewNumber(int16)];
        if (obj is int int32)
            return [DynValue.NewNumber(int32)];
        if (obj is long int64)
            return [DynValue.NewNumber(int64)];
        if (obj is ushort uint16)
            return [DynValue.NewNumber(uint16)];
        if (obj is uint uint32)
            return [DynValue.NewNumber(uint32)];
        if (obj is ulong uint64)
            return [DynValue.NewNumber(uint64)];
        if (obj is float single)
            return [DynValue.NewNumber(single)];
        if (obj is double dub)
            return [DynValue.NewNumber(dub)];
        if (obj is bool boolean)
            return [DynValue.NewBoolean(boolean)];
        if (obj is string str)
            return [DynValue.NewString(str)];
        if (obj is Color color)
            return
            [
                    DynValue.NewNumber(color.R),
                    DynValue.NewNumber(color.G),
                    DynValue.NewNumber(color.B),
                    DynValue.NewNumber(color.A),
            ];
        if (obj is Vector2 vector2)
            return
            [
                    DynValue.NewNumber(vector2.X),
                    DynValue.NewNumber(vector2.Y)
            ];
        if (obj is Point point)
            return
            [
                    DynValue.NewNumber(point.X),
                    DynValue.NewNumber(point.Y)
            ];
        if (obj is Vector3 vector3)
            return
            [
                    DynValue.NewNumber(vector3.X),
                    DynValue.NewNumber(vector3.Y),
                    DynValue.NewNumber(vector3.Z)
            ];
        if (obj is CameraMatrix cameraMatrix)
            return
            [
                DynValue.NewNumber(cameraMatrix.Position.X),
                DynValue.NewNumber(cameraMatrix.Position.Y),
                DynValue.NewNumber(cameraMatrix.Position.Z),
                DynValue.NewNumber(cameraMatrix.LookAt.X),
                DynValue.NewNumber(cameraMatrix.LookAt.Y),
                DynValue.NewNumber(cameraMatrix.LookAt.Z),
                DynValue.NewNumber(cameraMatrix.Roll),
                DynValue.NewNumber(cameraMatrix.Fov),
            ];
        if (obj is MarkerArrowProperties arrowProps)
            return
            [
                DynValue.NewNumber(arrowProps.Color.R),
                DynValue.NewNumber(arrowProps.Color.G),
                DynValue.NewNumber(arrowProps.Color.B),
                DynValue.NewNumber(arrowProps.Color.A),
                DynValue.NewNumber(arrowProps.Size),
            ];
        if (obj is PedClothingInfo pedClothingInfo)
            return
            [
                DynValue.NewString(pedClothingInfo.Texture),
                DynValue.NewString(pedClothingInfo.Model),
            ];
        if (obj is Delegate del)
            return [DynValue.NewCallback((context, arguments) => ToDynValues(del.DynamicInvoke(arguments.GetArray())!).First())];
        if (obj is Table table)
            return [DynValue.NewTable(table)];
        if (obj is DynValue dynValue)
            return [dynValue];
        if (obj is DynValue[] dynValues)
            return dynValues;
        if (obj is LuaValue luaValue)
            return [LuaValueToDynValue(luaValue)];

        if (obj is System.Runtime.CompilerServices.ITuple tuple)
        {
            var results = new List<DynValue>();
            for (int i = 0; i < tuple.Length; i++)
                results.AddRange(ToDynValues(tuple[i]));
            return results;
        }

        if (obj is System.Collections.Generic.IDictionary<string, string?> stringStringDict)
        {
            var dictTable = new Table(null);
            foreach (var (key, val) in stringStringDict)
                dictTable.Set(key, val == null ? DynValue.Nil : DynValue.NewString(val));
            return [DynValue.NewTable(dictTable)];
        }
        if (obj is IEnumerable<string> stringEnumerable)
        {
            var enumerableTable = new Table(null);
            foreach (var value in stringEnumerable.Select(ToDynValues).SelectMany(x => x))
                enumerableTable.Append(value);

            return [DynValue.NewTable(enumerableTable)];
        }
        if (obj is IEnumerable<object> enumerable)
        {
            var enumerableTable = new Table(null);
            foreach (var value in enumerable.Select(ToDynValues).SelectMany(x => x))
                enumerableTable.Append(value);

            return [DynValue.NewTable(enumerableTable)];
        }
        if (obj is IEnumerable nonGenericEnumerable)
        {
            var enumerableTable = new Table(null);
            foreach (var value in nonGenericEnumerable.Cast<object>().Select(ToDynValues).SelectMany(x => x))
                enumerableTable.Append(value);

            return [DynValue.NewTable(enumerableTable)];
        }

        throw new NotImplementedException($"Conversion to Lua for {obj.GetType()} not implemented");
    }

    private ScriptCallbackDelegateWrapper CreateCallbackWrapper(DynValue callbackValue)
    {
        var callback = callbackValue.Function;
        var context = Scripting.ScriptExecutionContext.Current;
        var environment = GetEnvironment(callback.OwnerScript);

        return new ScriptCallbackDelegateWrapper(parameters =>
        {
            var values = parameters
                .Select(ToDynValues)
                .SelectMany(x => x)
                .ToArray();

            var previous = Scripting.ScriptExecutionContext.Current;
            Scripting.ScriptExecutionContext.Current = context;

            environment?.EnterScriptLock();
            try
            {
                callback.Call(values);
            }
            catch (ScriptRuntimeException e)
            {
                this.logger.LogError(e, "Error while executing Lua callback: {decoratedMessage}", e.DecoratedMessage);
                ScriptErrored?.Invoke(e.DecoratedMessage);
            }
            finally
            {
                environment?.ExitScriptLock();
                Scripting.ScriptExecutionContext.Current = previous;
            }
        }, callback);
    }

    private DynValue LuaValueToDynValue(LuaValue luaValue)
    {
        if (luaValue.IsNil)
            return DynValue.Nil;
        if (luaValue.ElementId.HasValue)
        {
            var element = this.elementCollection.Get(luaValue.ElementId.Value);
            return element is not null ? UserData.Create(element) : DynValue.Nil;
        }
        if (luaValue.BoolValue.HasValue)
            return DynValue.NewBoolean(luaValue.BoolValue.Value);
        if (luaValue.StringValue != null)
            return DynValue.NewString(luaValue.StringValue);
        if (luaValue.IntegerValue.HasValue)
            return DynValue.NewNumber(luaValue.IntegerValue.Value);
        if (luaValue.FloatValue.HasValue)
            return DynValue.NewNumber(luaValue.FloatValue.Value);
        if (luaValue.DoubleValue.HasValue)
            return DynValue.NewNumber(luaValue.DoubleValue.Value);
        if (luaValue.TableValue != null)
        {
            var table = new Table(null);
            foreach (var kvp in luaValue.TableValue)
                table.Set(LuaValueToDynValue(kvp.Key), LuaValueToDynValue(kvp.Value));
            return DynValue.NewTable(table);
        }
        return DynValue.Nil;
    }

    private LuaValue DynValueToLuaValue(DynValue dynValue)
    {
        if (dynValue.Type == DataType.UserData && dynValue.UserData?.Object is Element element)
            return LuaValue.CreateElement(element.Id.Value);

        return dynValue.Type switch
        {
            DataType.Boolean => new LuaValue(dynValue.Boolean),
            DataType.String => new LuaValue(dynValue.String),
            DataType.Number => new LuaValue(dynValue.Number),
            DataType.Table => new LuaValue(dynValue.Table.Pairs
                .ToDictionary(
                    p => DynValueToLuaValue(p.Key),
                    p => DynValueToLuaValue(p.Value))),
            _ => LuaValue.Nil
        };
    }

    public float GetSingleFromDynValue(DynValue dynValue) => (float)dynValue.Number;
    public double GetDoubleFromDynValue(DynValue dynValue) => dynValue.Number;
    public byte GetByteFromDynValue(DynValue dynValue) => (byte)dynValue.Number;
    public short GetInt16FromDynValue(DynValue dynValue) => (short)dynValue.Number;
    public int GetInt32FromDynValue(DynValue dynValue) => (int)dynValue.Number;
    public long GetInt64FromDynValue(DynValue dynValue) => (long)dynValue.Number;
    public ushort GetUInt16FromDynValue(DynValue dynValue) => (ushort)dynValue.Number;
    public uint GetUInt32FromDynValue(DynValue dynValue) => (uint)dynValue.Number;
    public ulong GetUInt64FromDynValue(DynValue dynValue) => (ulong)dynValue.Number;
    public string GetStringFromDynValue(DynValue dynValue) => dynValue.String;
    public bool GetBooleanFromDynValue(DynValue dynValue) => dynValue.Boolean;
    public Table GetTableFromDynValue(DynValue dynValue) => dynValue.Table;

    public object? FromDynValue(Type targetType, Queue<DynValue> dynValues, bool isNullable = false)
    {
        if (isNullable && dynValues.Count > 0 && !IsCompatibleWith(targetType, dynValues.Peek()))
            return null;

        if (targetType == typeof(Color) || targetType == typeof(Color?))
        {
            byte red = GetByteFromDynValue(dynValues.Dequeue());
            byte green = GetByteFromDynValue(dynValues.Dequeue());
            byte blue = GetByteFromDynValue(dynValues.Dequeue());
            byte alpha = dynValues.Count > 0 && dynValues.Peek().Type == DataType.Number
                ? GetByteFromDynValue(dynValues.Dequeue())
                : (byte)255;
            return Color.FromArgb(alpha, red, green, blue);
        }
        if (targetType == typeof(Vector3))
            return new Vector3(GetSingleFromDynValue(dynValues.Dequeue()), GetSingleFromDynValue(dynValues.Dequeue()), GetSingleFromDynValue(dynValues.Dequeue()));
        if (targetType == typeof(Vector2))
            return new Vector2(GetSingleFromDynValue(dynValues.Dequeue()), GetSingleFromDynValue(dynValues.Dequeue()));
        if (targetType == typeof(Point))
            return new Point(GetInt32FromDynValue(dynValues.Dequeue()), GetInt32FromDynValue(dynValues.Dequeue()));
        if (targetType == typeof(Color))
            return Color.FromArgb(255, GetInt32FromDynValue(dynValues.Dequeue()), GetInt32FromDynValue(dynValues.Dequeue()), GetInt32FromDynValue(dynValues.Dequeue()));
        if (targetType == typeof(float))
            return GetSingleFromDynValue(dynValues.Dequeue());
        if (targetType == typeof(double))
            return GetDoubleFromDynValue(dynValues.Dequeue());
        if (targetType == typeof(byte))
            return GetByteFromDynValue(dynValues.Dequeue());
        if (targetType == typeof(short))
            return GetInt16FromDynValue(dynValues.Dequeue());
        if (targetType == typeof(ushort))
            return GetUInt16FromDynValue(dynValues.Dequeue());
        if (targetType == typeof(int))
            return GetInt32FromDynValue(dynValues.Dequeue());
        if (targetType == typeof(uint))
            return GetUInt32FromDynValue(dynValues.Dequeue());
        if (targetType == typeof(long))
            return GetInt64FromDynValue(dynValues.Dequeue());
        if (targetType == typeof(ulong))
            return GetUInt64FromDynValue(dynValues.Dequeue());
        if (targetType == typeof(string))
            return GetStringFromDynValue(dynValues.Dequeue());
        if (targetType == typeof(bool))
            return GetBooleanFromDynValue(dynValues.Dequeue());
        if (targetType == typeof(Table))
            return GetTableFromDynValue(dynValues.Dequeue());
        if (targetType == typeof(ScriptXmlNode))
            return dynValues.Dequeue()?.UserData?.Object as ScriptXmlNode;
        if (targetType == typeof(ScriptFile))
            return dynValues.Dequeue()?.UserData?.Object as ScriptFile;
        if (targetType == typeof(AclEntry))
            return dynValues.Dequeue()?.UserData?.Object as AclEntry;
        if (targetType == typeof(AclGroup))
            return dynValues.Dequeue()?.UserData?.Object as AclGroup;
        if (typeof(Player).IsAssignableFrom(targetType))
        {
            var val = dynValues.Dequeue();
            if (val.Type != DataType.UserData)
            {
                if (isNullable) return null;
                throw new LuaArgumentException(targetType.Name, targetType, 0, val.Type);
            }
            return val.UserData?.Object;
        }
        if (typeof(Element).IsAssignableFrom(targetType))
        {
            var val = dynValues.Dequeue();
            if (val.Type != DataType.UserData)
            {
                if (isNullable) return null;
                throw new LuaArgumentException(targetType.Name, targetType, 0, val.Type);
            }
            return val.UserData?.Object;
        }
        if (targetType == typeof(ScriptCallbackDelegateWrapper))
        {
            var callbackValue = dynValues.Dequeue();
            if (callbackValue.Type != DataType.Function)
                throw new ScriptRuntimeException($"Expected a function for callback argument, got {callbackValue.Type}");

            return CreateCallbackWrapper(callbackValue);
        }
        if (targetType == typeof(EventDelegate))
        {
            var callbackValue = dynValues.Dequeue();
            if (callbackValue.Type != DataType.Function)
                throw new ScriptRuntimeException($"Expected a function for event handler argument, got {callbackValue.Type}");

            var closure = callbackValue.Function;

            if (this.eventDelegateCache.TryGetValue(closure, out var cached))
                return cached;

            var context = Scripting.ScriptExecutionContext.Current;
            var environment = GetEnvironment(closure.OwnerScript);

            EventDelegate eventDelegate = (element, parameters) => {
                var source = UserData.Create(element);

                var values = parameters
                    .Select(ToDynValues)
                    .SelectMany(x => x)
                    .ToArray();

                var previous = Scripting.ScriptExecutionContext.Current;
                Scripting.ScriptExecutionContext.Current = context;

                var additionalGlobals = Scripting.ScriptExecutionContext.PendingGlobals;
                Scripting.ScriptExecutionContext.PendingGlobals = null;

                environment?.EnterScriptLock();
                try
                {
                    // Save previous values of source and any additional globals
                    var previousSource = closure.OwnerScript.Globals.RawGet("source");
                    Dictionary<string, DynValue?>? previousAdditionalValues = null;

                    if (additionalGlobals != null)
                    {
                        previousAdditionalValues = new(additionalGlobals.Count);
                        foreach (var kvp in additionalGlobals)
                        {
                            previousAdditionalValues[kvp.Key] = closure.OwnerScript.Globals.RawGet(kvp.Key);
                            closure.OwnerScript.Globals[kvp.Key] = ToDynValues(kvp.Value).First();
                        }
                    }

                    closure.OwnerScript.Globals["source"] = source;
                    try { closure.Call(values); }
                    finally
                    {
                        // Restore source
                        RestoreGlobal(closure.OwnerScript, "source", previousSource);

                        // Restore additional globals
                        if (previousAdditionalValues != null)
                        {
                            foreach (var kvp in previousAdditionalValues)
                                RestoreGlobal(closure.OwnerScript, kvp.Key, kvp.Value);
                        }
                    }
                }
                catch (ScriptRuntimeException e)
                {
                    this.logger.LogError(e, "Error while executing Lua event callback: {decoratedMessage}", e.DecoratedMessage);
                    ScriptErrored?.Invoke(e.DecoratedMessage);
                }
                finally
                {
                    environment?.ExitScriptLock();
                    Scripting.ScriptExecutionContext.Current = previous;
                }
            };

            this.eventDelegateCache.Add(closure, eventDelegate);
            return eventDelegate;
        }

        if (targetType == typeof(LuaValue))
            return DynValueToLuaValue(dynValues.Dequeue());
        if (targetType == typeof(ScriptTimer))
            return dynValues.Dequeue()?.UserData?.Object as ScriptTimer;
        if (targetType == typeof(object))
        {
            var val = dynValues.Dequeue();
            return val.Type switch
            {
                DataType.UserData => val.UserData?.Object,
                DataType.String => val.String,
                DataType.Number => val.Number,
                DataType.Boolean => val.Boolean,
                DataType.Table => val.Table,
                DataType.Function => CreateCallbackWrapper(val),
                _ => null
            };
        }
        if (targetType == typeof(DbConnectionHandle))
            return dynValues.Dequeue()?.UserData?.Object as DbConnectionHandle;
        if (targetType == typeof(DbQueryHandle))
            return dynValues.Dequeue()?.UserData?.Object as DbQueryHandle;
        if (targetType == typeof(AccountHandle))
            return dynValues.Dequeue()?.UserData?.Object as AccountHandle;
        if (targetType == typeof(TextItem))
            return dynValues.Dequeue()?.UserData?.Object as TextItem;
        if (targetType == typeof(TextDisplay))
            return dynValues.Dequeue()?.UserData?.Object as TextDisplay;
        if (targetType == typeof(Resource))
            return dynValues.Dequeue()?.UserData?.Object as Resource;
        if (targetType == typeof(ElementTarget))
        {
            var dynValue = dynValues.Dequeue();
            if (dynValue.Type == DataType.Nil)
                return null;
            if (dynValue.Type == DataType.Table)
            {
                var players = dynValue.Table.Values
                    .Where(v => v.Type == DataType.UserData)
                    .Select(v => v.UserData?.Object as Player)
                    .Where(p => p != null)
                    .Cast<Player>()
                    .ToList();
                return new ElementTarget(players);
            }
            return new ElementTarget(dynValue.UserData?.Object as Element);
        }
        if (targetType == typeof(DynValue))
            return dynValues.Dequeue(); 

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var innerType = Nullable.GetUnderlyingType(targetType)!;
            return FromDynValue(innerType, dynValues);
        }

        throw new NotImplementedException($"Conversion from Lua for {targetType} not implemented");
    }

    private static bool IsCompatibleWith(Type targetType, DynValue value)
    {
        if (targetType == typeof(ScriptCallbackDelegateWrapper)) 
            return value.Type == DataType.Function;

        if (targetType == typeof(Table)) 
            return value.Type == DataType.Table;

        if (targetType == typeof(string)) 
            return value.Type == DataType.String;

        if (targetType == typeof(bool)) 
            return value.Type == DataType.Boolean;

        if (targetType == typeof(float) || targetType == typeof(double) ||
            targetType == typeof(int) || targetType == typeof(long) ||
            targetType == typeof(uint) || targetType == typeof(ulong) ||
            targetType == typeof(short) || targetType == typeof(ushort) ||
            targetType == typeof(byte)) 
            return value.Type == DataType.Number;

        if (targetType == typeof(Resource))
            return value.Type == DataType.UserData;
        if (targetType == typeof(ElementTarget))
            return value.Type == DataType.UserData || value.Type == DataType.Table || value.Type == DataType.Nil;
        if (targetType == typeof(DynValue))
            return true;
        if (targetType == typeof(ScriptFile) || targetType == typeof(ScriptXmlNode) ||
            targetType == typeof(DbConnectionHandle) || targetType == typeof(DbQueryHandle) ||
            targetType == typeof(AclEntry) || targetType == typeof(AclGroup) ||
            targetType == typeof(AccountHandle) ||
            typeof(Element).IsAssignableFrom(targetType))
            return value.Type == DataType.UserData;

        if (targetType == typeof(ScriptTimer))
            return value.Type == DataType.UserData;

        if (targetType == typeof(object))
            return true;

        return true;
    }

    public event Action<string>? ScriptErrored;
}
