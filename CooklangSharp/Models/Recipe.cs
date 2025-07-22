using System.Collections.Immutable;

namespace CooklangSharp.Models;

/// <summary>
/// Base type for quantity values in ingredients.
/// </summary>
public abstract record QuantityValue
{
    /// <summary>
    /// Gets the numeric value as a double for calculations.
    /// Returns null if the quantity is not numeric (e.g., "some", "a pinch").
    /// </summary>
    public abstract double? GetNumericValue();
    
    /// <summary>
    /// Gets the display value as it should appear in recipes.
    /// </summary>
    public abstract override string ToString();
}

/// <summary>
/// Represents a regular numeric quantity (integer or decimal).
/// </summary>
public record RegularQuantity : QuantityValue
{
    /// <summary>
    /// Gets the numeric value.
    /// </summary>
    public double Value { get; init; }
    
    public RegularQuantity(double value)
    {
        Value = value;
    }
    
    public override double? GetNumericValue() => Value;
    
    public override string ToString()
    {
        // If it's a whole number, display without decimal
        if (Value == Math.Floor(Value))
            return ((int)Value).ToString();
        return Value.ToString();
    }
}

/// <summary>
/// Represents a fractional quantity (e.g., 1/2, 2 1/3).
/// </summary>
public record FractionalQuantity : QuantityValue
{
    /// <summary>
    /// Gets the whole part of the number (e.g., 2 in "2 1/3").
    /// </summary>
    public int Whole { get; init; }
    
    /// <summary>
    /// Gets the numerator of the fraction (e.g., 1 in "2 1/3").
    /// </summary>
    public int Numerator { get; init; }
    
    /// <summary>
    /// Gets the denominator of the fraction (e.g., 3 in "2 1/3").
    /// </summary>
    public int Denominator { get; init; }
    
    public FractionalQuantity(int whole = 0, int numerator = 0, int denominator = 1)
    {
        if (denominator == 0)
            throw new ArgumentException("Denominator cannot be zero", nameof(denominator));
            
        Whole = whole;
        Numerator = numerator;
        Denominator = denominator;
    }
    
    /// <summary>
    /// Creates a FractionalQuantity from a decimal value.
    /// </summary>
    public static FractionalQuantity FromDecimal(decimal value)
    {
        var whole = (int)Math.Floor(value);
        var fractionalPart = value - whole;
        
        if (fractionalPart == 0)
            return new FractionalQuantity(whole, 0, 1);
            
        // Find a reasonable denominator (up to 16)
        for (int denom = 2; denom <= 16; denom++)
        {
            var testNumerator = fractionalPart * denom;
            if (Math.Abs(testNumerator - Math.Round(testNumerator)) < 0.001m)
            {
                return new FractionalQuantity(whole, (int)Math.Round(testNumerator), denom);
            }
        }
        
        // If no simple fraction found, use a larger denominator
        return new FractionalQuantity(whole, (int)Math.Round(fractionalPart * 100), 100);
    }
    
    public override double? GetNumericValue()
    {
        return Whole + (double)Numerator / Denominator;
    }
    
    public override string ToString()
    {
        if (Numerator == 0)
            return Whole.ToString();
        
        if (Whole == 0)
            return $"{Numerator}/{Denominator}";
            
        return $"{Whole} {Numerator}/{Denominator}";
    }
}

/// <summary>
/// Represents a text-based quantity (e.g., "some", "a pinch").
/// </summary>
public record TextQuantity : QuantityValue
{
    /// <summary>
    /// Gets the text value.
    /// </summary>
    public string Value { get; init; }
    
    public TextQuantity(string value)
    {
        Value = value;
    }
    
    public override double? GetNumericValue() => null;
    
    public override string ToString() => Value;
}

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
    /// Gets the quantity value (can be RegularQuantity, FractionalQuantity, or TextQuantity).
    /// Null when no quantity is specified (e.g., @salt{}).
    /// </summary>
    public QuantityValue? Quantity { get; init; }
    
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
    /// Gets the quantity (typically a RegularQuantity, defaults to 1).
    /// </summary>
    public required QuantityValue Quantity { get; init; }
    
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
    /// Gets the duration (can be RegularQuantity, FractionalQuantity, or TextQuantity).
    /// </summary>
    public required QuantityValue Quantity { get; init; }
    
    /// <summary>
    /// Gets the time units (e.g., "minutes", "hours").
    /// </summary>
    public required string Units { get; init; }
}