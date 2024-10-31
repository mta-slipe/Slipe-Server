using SlipeServer.Server.Elements;
using System.Reflection;

namespace SlipeServer.LuaControllers.Commands;

public class LuaControllerArgumentException : Exception
{
    public int Index { get; }
    public MethodInfo MethodInfo { get; }

    public LuaControllerArgumentException(int index, MethodInfo methodInfo, Exception innerException) : base(null, innerException)
    {
        this.Index = index;
        this.MethodInfo = methodInfo;
    }
}

public sealed class LuaControllerArgumentsMapper
{
    private readonly Dictionary<Type, Func<string, object?>> mappings = [];
    public event Action<Player, LuaControllerArgumentException>? ArgumentErrorOccurred;
    public LuaControllerArgumentsMapper() { }

    public void DefineMap<T>(Func<string, object?> map)
    {
        if (!this.mappings.ContainsKey(typeof(T)))
        {
            this.mappings[typeof(T)] = map;
        }
    }

    private object? MapCustomeParameter(Type targetType, string value)
    {
        if (this.mappings.TryGetValue(targetType, out var mapFunction))
        {
            return mapFunction(value);
        }

        return null;
    }

    private object? MapParameter(Type targetType, string value)
    {
        if (targetType == typeof(string))
            return value;

        if (targetType.IsAssignableFrom(typeof(string)))
            return targetType;

        if (targetType == typeof(byte))
            return byte.Parse(value);

        if (targetType == typeof(ushort))
            return ushort.Parse(value);
        if (targetType == typeof(short))
            return short.Parse(value);

        if (targetType == typeof(uint))
            return uint.Parse(value);
        if (targetType == typeof(int))
            return int.Parse(value);

        if (targetType == typeof(ulong))
            return ulong.Parse(value);
        if (targetType == typeof(long))
            return long.Parse(value);

        if (targetType == typeof(float))
            return float.Parse(value);
        if (targetType == typeof(double))
            return double.Parse(value);

        if (targetType.IsEnum)
            return Enum.Parse(targetType, value, true);

        return MapCustomeParameter(targetType, value);
    }

    internal object?[]? MapParameters(Player player, string[] values, MethodInfo method)
    {
        var parameters = method.GetParameters();

        if (parameters.Length == 0)
            return [];

        if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableTo(typeof(IEnumerable<string>)))
            return [new CommandArgumentList(values)];

        int i = 0;
        try
        {
            if (parameters.Length != values.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            var objects = new List<object?>();
            for (; i < parameters.Length; i++)
                if (!parameters[i].IsOptional || values.Length > i)
                    objects.Add(MapParameter(parameters[i].ParameterType, values[i]));
                else if (values.Length <= i)
                    objects.Add(parameters[i].DefaultValue);

            return [.. objects];
        }
        catch (Exception ex)
        {
            ArgumentErrorOccurred?.Invoke(player, new LuaControllerArgumentException(i, method, ex));
        }
        return null;
    }
}
