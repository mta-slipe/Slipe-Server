namespace SlipeServer.Scripting.Definitions;

public class BitScriptDefinitions
{
    [ScriptFunctionDefinition("bitAnd")]
    public uint BitAnd(uint a, uint b, params uint[] rest)
    {
        uint result = a & b;
        foreach (var v in rest) 
            result &= v;
        return result;
    }

    [ScriptFunctionDefinition("bitNot")]
    public uint BitNot(uint value)
    {
        return ~value;
    }

    [ScriptFunctionDefinition("bitOr")]
    public uint BitOr(uint a, uint b, params uint[] rest)
    {
        uint result = a | b;
        foreach (var v in rest) 
            result |= v;
        return result;
    }

    [ScriptFunctionDefinition("bitXor")]
    public uint BitXor(uint a, uint b, params uint[] rest)
    {
        uint result = a ^ b;
        foreach (var v in rest)
            result ^= v;

        return result;
    }

    [ScriptFunctionDefinition("bitTest")]
    public bool BitTest(uint a, uint b, params uint[] rest)
    {
        uint mask = b;
        foreach (var v in rest) 
            mask &= v;

        return (a & mask) == mask;
    }

    [ScriptFunctionDefinition("bitLRotate")]
    public uint BitLRotate(uint value, int n)
    {
        n = ((n % 32) + 32) % 32;
        return (value << n) | (value >> (32 - n));
    }

    [ScriptFunctionDefinition("bitRRotate")]
    public uint BitRRotate(uint value, int n)
    {
        n = ((n % 32) + 32) % 32;
        return (value >> n) | (value << (32 - n));
    }

    [ScriptFunctionDefinition("bitLShift")]
    public uint BitLShift(uint value, int n)
    {
        if (n < 0) 
            return BitRShift(value, -n);

        if (n >= 32) 
            return 0;

        return value << n;
    }

    [ScriptFunctionDefinition("bitRShift")]
    public uint BitRShift(uint value, int n)
    {
        if (n < 0)
            return BitLShift(value, -n);

        if (n >= 32) 
            return 0;
        return value >> n;
    }

    [ScriptFunctionDefinition("bitArShift")]
    public int BitArShift(int value, int n)
    {
        if (n >= 32)
            return value < 0 ? -1 : 0;

        if (n < 0) 
            return (int)BitLShift((uint)value, -n);

        return value >> n;
    }

    [ScriptFunctionDefinition("bitExtract")]
    public uint BitExtract(uint value, int field, int width = 1)
    {
        if (width <= 0 || field < 0 || field + width > 32)
            return 0;

        return (value >> field) & ((1u << width) - 1u);
    }

    [ScriptFunctionDefinition("bitReplace")]
    public uint BitReplace(uint value, uint replaceValue, int field, int width = 1)
    {
        if (width <= 0 || field < 0 || field + width > 32) 
            return value;

        uint mask = ((1u << width) - 1u) << field;
        return (value & ~mask) | ((replaceValue << field) & mask);
    }
}
