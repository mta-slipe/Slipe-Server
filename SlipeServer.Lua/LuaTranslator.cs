using MoonSharp.Interpreter;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Lua
{
    public class LuaTranslator
    {
        private readonly MtaServer server;

        public LuaTranslator(MtaServer server)
        {
            this.server = server;

            UserData.RegisterType<Element>(InteropAccessMode.Default);
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

            return null;
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

            return null;
        }
    }
}
