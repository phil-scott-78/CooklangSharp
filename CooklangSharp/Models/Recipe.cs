using System.Collections.Immutable;

namespace CooklangSharp.Models;

/// <summary>
/// Represents a parsed Cooklang recipe containing sections, metadata, and front matter.
/// </summary>
public record Recipe
{
    /// <summary>
    /// Gets the list of sections in the recipe, each containing steps or notes.
    /// </summary>
    public ImmutableList<Section> Sections { get; init; } = ImmutableList<Section>.Empty;
    
    /// <summary>
    /// Gets the recipe metadata extracted from YAML front matter.
    /// </summary>
    public ImmutableDictionary<string, object> Metadata { get; init; } = ImmutableDictionary<string, object>.Empty;

    /// <summary>
    /// Gets the raw YAML front matter text.
    /// </summary>
    public string FrontMatter { get; init; } = string.Empty;
}

/// <summary>
/// Represents a section within a recipe, optionally named with == Section Name == syntax.
/// </summary>
public record Section
{
    /// <summary>
    /// Gets the optional name of the section.
    /// </summary>
    public string? Name { get; init; }
    
    /// <summary>
    /// Gets the content items within this section (steps or notes).
    /// </summary>
    public required ImmutableList<SectionContent> Content { get; init; }
}

/// <summary>
/// Base type for content that can appear in a section.
/// </summary>
public abstract record SectionContent
{
    /// <summary>
    /// Gets the type identifier for this content.
    /// </summary>
    public abstract string Type { get; }
}

/// <summary>
/// Represents a cooking step containing ingredients, cookware, timers, and instructions.
/// </summary>
public record StepContent : SectionContent
{
    /// <summary>
    /// Gets the type identifier "step".
    /// </summary>
    public override string Type => "step";
    
    /// <summary>
    /// Gets the step details including all items.
    /// </summary>
    public required Step Step { get; init; }
}

/// <summary>
/// Represents a standalone note or comment in the recipe.
/// </summary>
public record NoteContent : SectionContent  
{
    /// <summary>
    /// Gets the type identifier "text".
    /// </summary>
    public override string Type => "text";
    
    /// <summary>
    /// Gets the note text value.
    /// </summary>
    public required string Value { get; init; }
}

/// <summary>
/// Represents a single step in a recipe containing various items.
/// </summary>
public record Step
{
    /// <summary>
    /// Gets the list of items in this step (text, ingredients, cookware, timers).
    /// </summary>
    public required ImmutableList<Item> Items { get; init; }
    
    /// <summary>
    /// Gets the optional step number.
    /// </summary>
    public int? Number { get; init; }
}

/// <summary>
/// Base type for items that can appear in a step.
/// </summary>
public abstract record Item
{
    /// <summary>
    /// Gets the type identifier for this item.
    /// </summary>
    public abstract string Type { get; }
}

/// <summary>
/// Represents plain text within a step.
/// </summary>
public record TextItem : Item
{
    /// <summary>
    /// Gets the type identifier "text".
    /// </summary>
    public override string Type => "text";
    
    /// <summary>
    /// Gets the text value.
    /// </summary>
    public required string Value { get; init; }
}

/// <summary>
/// Represents an ingredient with quantity, units, and optional preparation note.
/// </summary>
public record IngredientItem : Item
{
    /// <summary>
    /// Gets the type identifier "ingredient".
    /// </summary>
    public override string Type => "ingredient";
    
    /// <summary>
    /// Gets the ingredient name.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Gets the quantity (can be int, double, or string like "some").
    /// </summary>
    public required object Quantity { get; init; }
    
    /// <summary>
    /// Gets the unit of measurement (empty string if not specified).
    /// </summary>
    public required string Units { get; init; }
    
    /// <summary>
    /// Gets the optional preparation note (e.g., "diced", "minced").
    /// </summary>
    public string? Note { get; init; }
}

/// <summary>
/// Represents cookware with quantity and optional note.
/// </summary>
public record CookwareItem : Item
{
    /// <summary>
    /// Gets the type identifier "cookware".
    /// </summary>
    public override string Type => "cookware";
    
    /// <summary>
    /// Gets the cookware name.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Gets the quantity (typically int, defaults to 1).
    /// </summary>
    public required object Quantity { get; init; }
    
    /// <summary>
    /// Gets the units (typically empty for cookware).
    /// </summary>
    public string Units { get; init; } = "";
    
    /// <summary>
    /// Gets the optional note (e.g., "non-stick", "12-inch").
    /// </summary>
    public string? Note { get; init; }
}

/// <summary>
/// Represents a timer with duration and units.
/// </summary>
public record TimerItem : Item
{
    /// <summary>
    /// Gets the type identifier "timer".
    /// </summary>
    public override string Type => "timer";
    
    /// <summary>
    /// Gets the timer name (empty string for anonymous timers).
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Gets the duration (can be int, double, or string).
    /// </summary>
    public required object Quantity { get; init; }
    
    /// <summary>
    /// Gets the time units (e.g., "minutes", "hours").
    /// </summary>
    public required string Units { get; init; }
}