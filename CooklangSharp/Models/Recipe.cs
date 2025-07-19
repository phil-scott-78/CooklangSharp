namespace CooklangSharp.Models;

public record Recipe
{
    public List<Step> Steps { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

public record Step
{
    public List<Item> Items { get; init; } = new();
}

public abstract record Item
{
    public abstract string Type { get; }
}

public record TextItem : Item
{
    public override string Type => "text";
    public required string Value { get; init; }
}

public record IngredientItem : Item
{
    public override string Type => "ingredient";
    public required string Name { get; init; }
    public required object Quantity { get; init; }
    public required string Units { get; init; }
}

public record CookwareItem : Item
{
    public override string Type => "cookware";
    public required string Name { get; init; }
    public required object Quantity { get; init; }
    public string Units { get; init; } = "";
}

public record TimerItem : Item
{
    public override string Type => "timer";
    public required string Name { get; init; }
    public required object Quantity { get; init; }
    public required string Units { get; init; }
}