using CooklangSharp.Models;
using Shouldly;
using System.Diagnostics;

namespace CooklangSharp.Tests.Core;

public class Comprehensive
{
    [Fact]
    public void Can_do_comprehensive_test()
    {
        var recipe = """
                     ---
                     title: Comprehensive Test Recipe
                     servings: 4
                     tags:
                       - test
                       - comprehensive
                     ---

                     > This recipe tests all major Cooklang features

                     = Ingredients Section =

                     We need @flour{2%cups} and @baking soda{1%tsp} for the base.
                     Also @eggs{3}(room temperature) and @butter{1/2%cup}(melted).

                     == Sub-section for special ingredients

                     Add @vanilla extract{2%tsp} and @./sauces/chocolate sauce{100%ml}.

                     = Equipment Section =

                     Prepare your #mixing bowl{} and #whisk{}(clean and dry).
                     Set #oven{} to 350°F.

                     = Method =

                     -- This is a comment about the method
                     Mix all ingredients in the #bowl for ~{5%minutes}.
                     Rest the dough for ~rest{30%minutes}(or until doubled).

                     [-
                     This is a block comment
                     spanning multiple lines
                     -]

                     Bake in the #oven{} for ~baking time{25%minutes}.

                     = Notes =

                     >> source: https://example.com/recipe
                     >> difficulty: easy

                     The recipe references work: @./bases/pizza dough{500%g}
                     """;
        
        var result = CooklangParser.Parse(recipe);
        
        // Output the current structure for comparison
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        Console.WriteLine($"Sections: {result.Recipe.Sections.Count}");
        foreach (var section in result.Recipe.Sections)
        {
            Console.WriteLine($"  Section: {section.Name ?? "(default)"}");
            Console.WriteLine($"  Content items: {section.Content.Count}");
            foreach (var content in section.Content)
            {
                if (content is StepContent step)
                {
                    Console.WriteLine($"    Step {step.Step.Number}: {step.Step.Items.Count} items");
                    foreach (var item in step.Step.Items)
                    {
                        Console.WriteLine($"      {item.Type}: {GetItemDescription(item)}");
                    }
                }
                else if (content is NoteContent note)
                {
                    Console.WriteLine($"    Note: {note.Value}");
                }
            }
        }
        
        Debug.WriteLine($"\nMetadata: {result.Recipe.Metadata.Count} items");
        foreach (var (key, value) in result.Recipe.Metadata)
        {
            Debug.WriteLine($"  {key}: {value}");
        }
    }
    
    private static string GetItemDescription(Item item)
    {
        return item switch
        {
            TextItem text => $"\"{text.Value}\"",
            IngredientItem ing => $"{ing.Name} ({ing.Quantity} {ing.Units}){(ing.Note != null ? $" - {ing.Note}" : "")}",
            CookwareItem cook => $"{cook.Name} ({cook.Quantity} {cook.Units}){(cook.Note != null ? $" - {cook.Note}" : "")}",
            TimerItem timer => $"{timer.Name} ({timer.Quantity} {timer.Units})",
            _ => "unknown"
        };
    }
}