using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Adv76.JsonMergePatch;

public static partial class JsonMergePatcher
{
    private static readonly FrozenSet<Type> s_cacheableUnderlyingTypes = FrozenSet.ToFrozenSet<Type>([
        typeof(string),
        typeof(bool),
        typeof(char),
        typeof(Guid),
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(Int128),
        typeof(UInt128),
        typeof(Half),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(DateOnly),
        typeof(TimeOnly)
    ]);

    private static readonly ConcurrentDictionary<Type, ReadDelegate> s_readCache = new();
    private static readonly ConcurrentDictionary<Type, ReadAsPropertyNameDelegate> s_readAsPropertyNameCache = new();

    private static readonly MethodInfo s_readMethod =
        typeof(JsonMergePatcher).GetMethod(nameof(Read), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo s_readAsPropertyNameMethod =
        typeof(JsonMergePatcher).GetMethod(nameof(ReadAsPropertyName), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static PropertyInfo? _jsonTypeInfoPropertyInfo;

    private static bool IsCacheablePrimitive(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return s_cacheableUnderlyingTypes.Contains(underlying);
    }

    private static JsonTypeInfo GetTypeInfo(JsonPropertyInfo property)
    {
        _jsonTypeInfoPropertyInfo ??= typeof(JsonPropertyInfo).GetProperty("JsonTypeInfo", BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Suppress null
        return (JsonTypeInfo)_jsonTypeInfoPropertyInfo.GetValue(property, null)!;
    }

    private static object? ReadValueWithConverter(ref Utf8JsonReader reader, JsonConverter converter, Type propertyType,
        JsonSerializerOptions jsonOptions)
    {
        var readDelegate = CreateGenericReadDelegate(propertyType);

        return readDelegate(converter, ref reader, propertyType, jsonOptions);
    }

    private delegate object? ReadDelegate(JsonConverter c, ref Utf8JsonReader r, Type t, JsonSerializerOptions o);

    private static ReadDelegate CreateGenericReadDelegate(Type valueType)
    {
        if (IsCacheablePrimitive(valueType))
        {
            return s_readCache.GetOrAdd(valueType, static t => s_readMethod.MakeGenericMethod(t).CreateDelegate<ReadDelegate>());
        }

        return s_readMethod.MakeGenericMethod(valueType).CreateDelegate<ReadDelegate>();
    }

    private static object? Read<TValue>(JsonConverter c, ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
        => ((JsonConverter<TValue>)c).Read(ref r, t, o);

    private static object? ReadValueAsPropertyNameWithConverter(ref Utf8JsonReader reader, JsonConverter converter, Type propertyType,
        JsonSerializerOptions jsonOptions)
    {
        var readDelegate = CreateGenericReadAsPropertyNameDelegate(propertyType);

        return readDelegate(converter, ref reader, propertyType, jsonOptions);
    }

    private delegate object? ReadAsPropertyNameDelegate(JsonConverter c, ref Utf8JsonReader r, Type t, JsonSerializerOptions o);

    private static ReadAsPropertyNameDelegate CreateGenericReadAsPropertyNameDelegate(Type valueType)
    {
        if (IsCacheablePrimitive(valueType))
        {
            return s_readAsPropertyNameCache.GetOrAdd(valueType, static t => s_readAsPropertyNameMethod.MakeGenericMethod(t).CreateDelegate<ReadAsPropertyNameDelegate>());
        }

        return s_readAsPropertyNameMethod.MakeGenericMethod(valueType).CreateDelegate<ReadAsPropertyNameDelegate>();
    }

    private static object? ReadAsPropertyName<TValue>(JsonConverter c, ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
        => ((JsonConverter<TValue>)c).ReadAsPropertyName(ref r, t, o);
}
