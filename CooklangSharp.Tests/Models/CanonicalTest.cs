namespace CooklangSharp.Tests.Models;

public class CanonicalTestData
{
    public int Version { get; set; }
    public Dictionary<string, CanonicalTest> Tests { get; set; } = new();
}

public class CanonicalTest
{
    public string Source { get; set; } = "";
    public CanonicalResult Result { get; set; } = new();
}

public class CanonicalResult
{
    public List<List<CanonicalItem>> Steps { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class CanonicalItem
{
    public string Type { get; set; } = "";
    public string? Value { get; set; }
    public string? Name { get; set; }
    public object? Quantity { get; set; }
    public string? Units { get; set; }
}