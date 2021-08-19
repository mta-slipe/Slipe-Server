using MoonSharp.Interpreter;
using SlipeServer.Scripting;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Lua
{
    public class LuaTranslator
    {
        public LuaTranslator()
        {
            UserData.RegisterType<Element>(InteropAccessMode.Hardwired);
        }

        public IEnumerable<DynValue> ToDynValues(object obj)
        {
            if (obj == null)
                return new DynValue[] { DynValue.Nil };
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
            if (obj is Vector3 vector)
                return new DynValue[]
                {
                    DynValue.NewNumber(vector.X),
                    DynValue.NewNumber(vector.Y),
                    DynValue.NewNumber(vector.Z)
                };
            if (obj is Delegate del)
                return new DynValue[] { DynValue.NewCallback((context, arguments) => ToDynValues(del.DynamicInvoke(arguments.GetArray())).First()) };
            if (obj is Table table)
                return new DynValue[] { DynValue.NewTable(table) };
            if (obj is DynValue dynValue)
                return new DynValue[] { dynValue };

            if (obj is IEnumerable<string> stringEnumerable)
                return stringEnumerable.Select(x => DynValue.NewString(x)).ToArray();
            if (obj is IEnumerable<object> enumerable)
                return enumerable.Select(x => ToDynValues(obj)).SelectMany(x => x).ToArray();

            throw new NotImplementedException($"Conversion to Lua for {obj.GetType()} not implemented");
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

        public object FromDynValue(Type targetType, Queue<DynValue> dynValues)
        {
            if (targetType == typeof(Vector3))
                return new Vector3(GetSingleFromDynValue(dynValues.Dequeue()), GetSingleFromDynValue(dynValues.Dequeue()), GetSingleFromDynValue(dynValues.Dequeue()));
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
            if (typeof(Element).IsAssignableFrom(targetType))
                return dynValues.Dequeue().UserData.Object;
            if (targetType == typeof(ScriptCallbackDelegate))
            {
                var callback = dynValues.Dequeue().Function;
                return (ScriptCallbackDelegate)((parameters) => callback.Call(ToDynValues(parameters)));
            }
            if (targetType == typeof(EventDelegate))
            {
                var callback = dynValues.Dequeue().Function;
                return (EventDelegate)((element, parameters) => callback.Call(new DynValue[] { UserData.Create(element) }.Concat(ToDynValues(parameters))));
            }
            if (targetType == typeof(CommandDelegate))
            {
                var callback = dynValues.Dequeue().Function;
                return (CommandDelegate)((element, commandName, parameters) => callback.Call(new DynValue[] { UserData.Create(element), DynValue.NewString(commandName) }.Concat(ToDynValues(parameters)).ToArray()));
            }

            throw new NotImplementedException($"Conversion from Lua for {targetType} not implemented");
        }
    }
}
