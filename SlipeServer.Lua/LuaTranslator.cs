using MoonSharp.Interpreter;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Scripting;
using SlipeServer.Scripting.Definitions;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using MarkerArrowProperties = SlipeServer.Scripting.Definitions.MarkerArrowProperties;

namespace SlipeServer.Lua;

public class LuaTranslator
{
    public LuaTranslator()
    {
        UserData.RegisterType<Element>(InteropAccessMode.Hardwired);
        UserData.RegisterType<ScriptFile>(InteropAccessMode.Hardwired);
    }

    public IEnumerable<DynValue> ToDynValues(object? obj)
    {
        if (obj == null)
            return new DynValue[] { DynValue.Nil };
        if (obj is ScriptFile scriptFile)
            return new DynValue[] { UserData.Create(scriptFile) };
        if (obj is Element element)
            return new DynValue[] { UserData.Create(element) };
        if (obj is byte int8)
            return new DynValue[] { DynValue.NewNumber(int8) };
        if (obj is short int16)
            return new DynValue[] { DynValue.NewNumber(int16) };
        if (obj is int int32)
            return new DynValue[] { DynValue.NewNumber(int32) };
        if (obj is long int64)
            return new DynValue[] { DynValue.NewNumber(int64) };
        if (obj is ushort uint16)
            return new DynValue[] { DynValue.NewNumber(uint16) };
        if (obj is uint uint32)
            return new DynValue[] { DynValue.NewNumber(uint32) };
        if (obj is ulong uint64)
            return new DynValue[] { DynValue.NewNumber(uint64) };
        if (obj is float single)
            return new DynValue[] { DynValue.NewNumber(single) };
        if (obj is double dub)
            return new DynValue[] { DynValue.NewNumber(dub) };
        if (obj is bool boolean)
            return new DynValue[] { DynValue.NewBoolean(boolean) };
        if (obj is string str)
            return new DynValue[] { DynValue.NewString(str) };
        if (obj is Color color)
            return new DynValue[]
            {
                    DynValue.NewNumber(color.R),
                    DynValue.NewNumber(color.G),
                    DynValue.NewNumber(color.B),
                    DynValue.NewNumber(color.A),
            };
        if (obj is Vector2 vector2)
            return new DynValue[]
            {
                    DynValue.NewNumber(vector2.X),
                    DynValue.NewNumber(vector2.Y)
            };
        if (obj is Point point)
            return new DynValue[]
            {
                    DynValue.NewNumber(point.X),
                    DynValue.NewNumber(point.Y)
            };
        if (obj is Vector3 vector3)
            return new DynValue[]
            {
                    DynValue.NewNumber(vector3.X),
                    DynValue.NewNumber(vector3.Y),
                    DynValue.NewNumber(vector3.Z)
            };
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
        if (obj is Delegate del)
            return new DynValue[] { DynValue.NewCallback((context, arguments) => ToDynValues(del.DynamicInvoke(arguments.GetArray())!).First()) };
        if (obj is Table table)
            return new DynValue[] { DynValue.NewTable(table) };
        if (obj is DynValue dynValue)
            return new DynValue[] { dynValue };
        if (obj is LuaValue luaValue)
            return new DynValue[] { LuaValueToDynValue(luaValue) };

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

        throw new NotImplementedException($"Conversion to Lua for {obj.GetType()} not implemented");
    }

    private DynValue LuaValueToDynValue(LuaValue luaValue)
    {
        if (luaValue.IsNil)
            return DynValue.Nil;
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

    public object? FromDynValue(Type targetType, Queue<DynValue> dynValues)
    {
        if (targetType == typeof(Color) || targetType == typeof(Color?))
        {
            byte red = GetByteFromDynValue(dynValues.Dequeue());
            byte green = GetByteFromDynValue(dynValues.Dequeue());
            byte blue = GetByteFromDynValue(dynValues.Dequeue());
            byte alpha = GetByteFromDynValue(dynValues.Dequeue());
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
        if (targetType == typeof(ScriptFile))
            return dynValues.Dequeue()?.UserData?.Object as ScriptFile;
        if (typeof(Player).IsAssignableFrom(targetType))
            return dynValues.Dequeue()?.UserData?.Object;
        if (typeof(Element).IsAssignableFrom(targetType))
            return dynValues.Dequeue()?.UserData?.Object;
        if (targetType == typeof(ScriptCallbackDelegateWrapper))
        {
            var callback = dynValues.Dequeue().Function;
            var context = Scripting.ScriptExecutionContext.Current;

            return new ScriptCallbackDelegateWrapper(parameters => {
                var values = parameters
                    .Select(ToDynValues)
                    .SelectMany(x => x)
                    .ToArray();

                var previous = Scripting.ScriptExecutionContext.Current;
                Scripting.ScriptExecutionContext.Current = context;

                callback.Call(values);

                Scripting.ScriptExecutionContext.Current = previous;
            }, callback);
        }
        if (targetType == typeof(EventDelegate))
        {
            var callback = dynValues.Dequeue().Function;
            var context = Scripting.ScriptExecutionContext.Current;

            return (EventDelegate)((element, parameters) => {
                var source = UserData.Create(element);

                callback.OwnerScript.Globals["source"] = source;

                var values = parameters
                    .Select(ToDynValues)
                    .SelectMany(x => x)
                    .ToArray();

                var previous = Scripting.ScriptExecutionContext.Current;
                Scripting.ScriptExecutionContext.Current = context;

                callback.Call(values);

                Scripting.ScriptExecutionContext.Current = previous;

                callback.OwnerScript.Globals.Remove("source");
            });
        }

        if (targetType == typeof(LuaValue))
            return DynValueToLuaValue(dynValues.Dequeue());

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var innerType = Nullable.GetUnderlyingType(targetType)!;
            return FromDynValue(innerType, dynValues);
        }

        throw new NotImplementedException($"Conversion from Lua for {targetType} not implemented");
    }
}
