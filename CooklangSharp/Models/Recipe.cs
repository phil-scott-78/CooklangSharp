namespace CooklangSharp.Models;

public record Recipe
{
    public List<Section> Sections { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();

    public string FrontMatter { get; init; } = string.Empty;
}

public record Section
{
    public string? Name { get; init; }
    public List<SectionContent> Content { get; init; } = new();
}

public abstract record SectionContent
{
    public abstract string Type { get; }
}

public record StepContent : SectionContent
{
    public override string Type => "step";
    public required Step Step { get; init; }
}

public record NoteContent : SectionContent  
{
    public override string Type => "text";
    public required string Value { get; init; }
}

public record Step
{
    public List<Item> Items { get; init; } = new();
    public int? Number { get; init; }
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
    public string? Note { get; init; }
}

public record CookwareItem : Item
{
    public override string Type => "cookware";
    public required string Name { get; init; }
    public required object Quantity { get; init; }
    public string Units { get; init; } = "";
    public string? Note { get; init; }
}

public record TimerItem : Item
{
    public override string Type => "timer";
    public required string Name { get; init; }
    public required object Quantity { get; init; }
    public required string Units { get; init; }
}