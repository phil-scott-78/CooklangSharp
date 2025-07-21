# CooklangSharp

A .NET parser for the [Cooklang](https://cooklang.org/) recipe markup language. Parse and analyze recipes written in Cooklang format with full support for ingredients, cookware, timers, and metadata.

[![NuGet](https://img.shields.io/nuget/v/CooklangSharp.svg)](https://www.nuget.org/packages/CooklangSharp/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Installation

```bash
dotnet add package CooklangSharp
```

## Quick Start

```csharp
using CooklangSharp;

var recipeText = @"
---
servings: 4
prep time: 15 minutes
---

Preheat #oven{} to 350°F.

Mix @flour{2%cups}, @sugar{1%cup}, and @eggs{2} in a #mixing bowl{}.
Bake for ~{25%minutes} until golden brown.
";

var result = CooklangParser.Parse(recipeText);

if (result.Success)
{
    var recipe = result.Recipe;
    Console.WriteLine($"Servings: {recipe.Metadata["servings"]}");
    
    // Access ingredients, cookware, and timers
    foreach (var section in recipe.Sections)
    {
        foreach (var content in section.Content)
        {
            if (content is StepContent step)
            {
                Console.WriteLine($"Step {step.Step.Number}:");
                foreach (var item in step.Step.Items)
                {
                    switch (item)
                    {
                        case IngredientItem ingredient:
                            Console.WriteLine($"  - Ingredient: {ingredient.Name} ({ingredient.Quantity} {ingredient.Units})");
                            break;
                        case CookwareItem cookware:
                            Console.WriteLine($"  - Cookware: {cookware.Name}");
                            break;
                        case TimerItem timer:
                            Console.WriteLine($"  - Timer: {timer.Quantity} {timer.Units}");
                            break;
                    }
                }
            }
        }
    }
}
else
{
    foreach (var diagnostic in result.Diagnostics)
    {
        Console.WriteLine($"{diagnostic.Severity} at {diagnostic.Line}:{diagnostic.Column}: {diagnostic.Message}");
    }
}
```

## API Reference

### CooklangParser.Parse

The main entry point for parsing Cooklang recipes.

```csharp
public static ParseResult Parse(string text)
```

**Parameters:**
- `text`: The Cooklang recipe text to parse

**Returns:**
- `ParseResult`: Contains the parsed recipe or error diagnostics

### ParseResult

```csharp
public record ParseResult
{
    public bool Success { get; }
    public Recipe? Recipe { get; }
    public List<Diagnostic> Diagnostics { get; }
}
```

### Recipe Structure

```csharp
public record Recipe
{
    public List<Section> Sections { get; }
    public Dictionary<string, object> Metadata { get; }
    public string FrontMatter { get; }
}
```

### Working with Recipe Components

#### Ingredients
```csharp
public record IngredientItem
{
    public string Name { get; }
    public object Quantity { get; }  // Can be int, double, or string
    public string Units { get; }
    public string? Note { get; }     // Optional preparation note
}
```

Examples:
- `@salt` → Name: "salt", Quantity: "some", Units: ""
- `@flour{2%cups}` → Name: "flour", Quantity: 2, Units: "cups"
- `@onion{1}(diced)` → Name: "onion", Quantity: 1, Units: "", Note: "diced"

#### Cookware
```csharp
public record CookwareItem
{
    public string Name { get; }
    public object Quantity { get; }  // Defaults to 1 if not specified
    public string? Note { get; }
}
```

Examples:
- `#pot` → Name: "pot", Quantity: 1
- `#mixing bowl{2}` → Name: "mixing bowl", Quantity: 2
- `#pan{}(non-stick)` → Name: "pan", Quantity: 1, Note: "non-stick"

#### Timers
```csharp
public record TimerItem
{
    public string Name { get; }      // Empty string for anonymous timers
    public object Quantity { get; }
    public string Units { get; }
}
```

Examples:
- `~{10%minutes}` → Name: "", Quantity: 10, Units: "minutes"
- `~bake{25%minutes}` → Name: "bake", Quantity: 25, Units: "minutes"

## Complete Example

```csharp
using CooklangSharp;
using CooklangSharp.Models;

var recipeText = File.ReadAllText("chocolate-chip-cookies.cook");
var result = CooklangParser.Parse(recipeText);

if (result.Success)
{
    var recipe = result.Recipe;
    
    // Extract all ingredients
    var ingredients = recipe.Sections
        .SelectMany(s => s.Content)
        .OfType<StepContent>()
        .SelectMany(sc => sc.Step.Items)
        .OfType<IngredientItem>()
        .ToList();
    
    Console.WriteLine("Shopping List:");
    foreach (var ing in ingredients.GroupBy(i => i.Name))
    {
        var total = ing.Sum(i => i.Quantity is double d ? d : 
                               i.Quantity is int n ? n : 0);
        var unit = ing.First().Units;
        Console.WriteLine($"- {ing.Key}: {total} {unit}".Trim());
    }
    
    // Extract all cookware
    var cookware = recipe.Sections
        .SelectMany(s => s.Content)
        .OfType<StepContent>()
        .SelectMany(sc => sc.Step.Items)
        .OfType<CookwareItem>()
        .Select(c => c.Name)
        .Distinct();
    
    Console.WriteLine("\nRequired Cookware:");
    foreach (var item in cookware)
    {
        Console.WriteLine($"- {item}");
    }
    
    // Calculate total time
    var timers = recipe.Sections
        .SelectMany(s => s.Content)
        .OfType<StepContent>()
        .SelectMany(sc => sc.Step.Items)
        .OfType<TimerItem>()
        .Where(t => t.Units.Contains("minute"))
        .Sum(t => t.Quantity is double d ? d : t.Quantity is int n ? n : 0);
    
    Console.WriteLine($"\nTotal active time: {timers} minutes");
}
```

## Error Handling

The parser provides detailed diagnostics for syntax errors:

```csharp
var result = CooklangParser.Parse("Invalid @recipe{text");

if (!result.Success)
{
    foreach (var diagnostic in result.Diagnostics)
    {
        Console.WriteLine($"{diagnostic.Severity} at line {diagnostic.Line}, column {diagnostic.Column}:");
        Console.WriteLine($"  {diagnostic.Message}");
        
        if (!string.IsNullOrEmpty(diagnostic.Code))
        {
            Console.WriteLine($"  Code: {diagnostic.Code}");
        }
    }
}
```

## Cooklang Syntax

For complete Cooklang syntax documentation, visit [cooklang.org](https://cooklang.org/).

### Quick Reference:
- **Ingredients**: `@ingredient` or `@ingredient{quantity%unit}`
- **Cookware**: `#cookware` or `#cookware{quantity}`
- **Timers**: `~{duration%unit}` or `~timer{duration%unit}`
- **Metadata**: YAML front matter between `---` markers
- **Comments**: `-- single line` or `[- block comment -]`
- **Sections**: `== Section Name ==`

### Important Notes:
- Multi-word ingredients/cookware require `{}`: `@olive oil{}`, `#mixing bowl{}`
- Empty braces `{}` means "some" for ingredients, or default quantity for cookware
- Preparation notes use parentheses: `@carrots{2}(diced)`

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Alternatives

If you're looking for other Cooklang parsers for .NET, check out:
- [CookLangNet](https://github.com/heytherewill/CookLangNet) - An F# implementation with C# wrapper

## Acknowledgments

- [Cooklang](https://cooklang.org/) - The cooking markup language
