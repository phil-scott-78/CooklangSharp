using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CooklangSharp.Tests.Models;

public class CanonicalTestData
{
    public int Version { get; set; }
    public Dictionary<string, CanonicalTest> Tests { get; set; } = new();
}

public class CanonicalTest : IFormattable, IParsable<CanonicalTest>
{
    private static readonly IDeserializer Deserializer;
    private static readonly ISerializer Serializer;
    public required string Source { get; init; } 
    public required CanonicalResult Result { get; init; } 

    static CanonicalTest()
    {
        Deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        
        Serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return Serializer.Serialize((Source, Result));
    }

    public static CanonicalTest Parse(string s, IFormatProvider? provider)
    {
        var v = Deserializer.Deserialize<(string, CanonicalResult)>(s);
        return new CanonicalTest()
        {
            Source = v.Item1, Result = v.Item2
        };
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out CanonicalTest result)
    {
        if (s == null)
        {
            result = null;
            return false;
        }
        
        var v = Deserializer.Deserialize<(string, CanonicalResult)>(s);
        result = new CanonicalTest
        {
            Source = v.Item1, Result = v.Item2
        };

        return true;
    }
}

public class CanonicalResult
{
    public required List<List<CanonicalItem>> Steps { get; init; }
    public required Dictionary<string, object> Metadata { get; init; }
}

public class CanonicalItem
{
    public required string Type { get; init; }
    public string? Value { get; init; }
    public string? Name { get; init; }
    public object? Quantity { get; init; }
    public string? Units { get; init; }
}