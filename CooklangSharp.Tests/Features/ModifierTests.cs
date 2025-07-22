using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests.Features;

public class ModifierTests
{
    [Fact]
    public void ParsesIngredientModifiersCorrectly()
    {
        var source = """
            Mix @onion{1}(peeled and finely chopped) and @garlic{2%cloves}(peeled and minced) into paste.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        // Get the first section's first step content
        var section = result.Recipe.Sections[0];
        var stepContent = section.Content[0] as StepContent;
        stepContent.ShouldNotBeNull();
        var step = stepContent.Step;

        var ingredients = step.Items.OfType<IngredientItem>().ToList();
        ingredients.Count.ShouldBe(2);
        
        // First ingredient with modifier
        var onion = ingredients[0];
        onion.Name.ShouldBe("onion");
        onion.Quantity?.GetNumericValue().ShouldBe(1.0);
        onion.Units.ShouldBe("");
        onion.Note.ShouldBe("peeled and finely chopped");
        
        // Second ingredient with modifier
        var garlic = ingredients[1];
        garlic.Name.ShouldBe("garlic");
        garlic.Quantity?.GetNumericValue().ShouldBe(2.0);
        garlic.Units.ShouldBe("cloves");
        garlic.Note.ShouldBe("peeled and minced");
    }

    [Fact]
    public void ParsesIngredientWithoutModifier()
    {
        var source = "Add @salt{1%tsp} to taste.";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var ingredient = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step.Items
            .OfType<IngredientItem>().First();
        
        ingredient.Name.ShouldBe("salt");
        ingredient.Quantity?.GetNumericValue().ShouldBe(1.0);
        ingredient.Units.ShouldBe("tsp");
        ingredient.Note.ShouldBeNull();
    }

    [Fact]
    public void ParsesModifierWithSpecialCharacters()
    {
        var source = """
            Add @butter{50%g}(cold, cut into 1/2-inch cubes) to mixture.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var ingredient = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step.Items
            .OfType<IngredientItem>().First();
        
        ingredient.Name.ShouldBe("butter");
        ingredient.Note.ShouldBe("cold, cut into 1/2-inch cubes");
    }

    [Fact]
    public void HandlesNestedParenthesesInModifiers()
    {
        var source = """
            Add @sauce{100%ml}(homemade (see recipe on page 5)) to the dish.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        var section = result.Recipe.Sections[0];
        var stepContent = section.Content[0] as StepContent;
        stepContent.ShouldNotBeNull();
        var step = stepContent.Step;

        var ingredient = step.Items.OfType<IngredientItem>().First();
        ingredient.ShouldNotBeNull();
        ingredient.Name.ShouldBe("sauce");
        ingredient.Quantity?.GetNumericValue().ShouldBe(100.0);
        ingredient.Units.ShouldBe("ml");
        ingredient.Note.ShouldBe("homemade (see recipe on page 5)");
    }

    [Fact]
    public void ParsesEmptyModifier()
    {
        var source = "Add @ingredient{1}() to mixture.";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var ingredient = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step.Items
            .OfType<IngredientItem>().First();
        
        ingredient.Name.ShouldBe("ingredient");
        ingredient.Note.ShouldBe("");
    }

    [Fact]
    public void ParsesModifierWithQuotes()
    {
        var source = """
            Add @herbs{1%tbsp}(fresh "Italian" herbs) to sauce.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var ingredient = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step.Items
            .OfType<IngredientItem>().First();
        
        ingredient.Note.ShouldBe("fresh \"Italian\" herbs");
    }

    [Fact]
    public void ParsesCookwareWithoutModifier()
    {
        var source = "Bake in #oven{} for ~{30%minutes}.";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var step = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step;
        
        var cookware = step.Items.OfType<CookwareItem>().First();
        cookware.Name.ShouldBe("oven");
    }

    [Fact]
    public void ParsesMultipleModifiersInOneStep()
    {
        var source = """
            Mix @flour{200%g}(all-purpose) with @milk{1%cup}(warm) using #whisk{}.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var step = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step;
        
        var flour = step.Items.OfType<IngredientItem>().First(i => i.Name == "flour");
        flour.Note.ShouldBe("all-purpose");
        
        var milk = step.Items.OfType<IngredientItem>().First(i => i.Name == "milk");
        milk.Note.ShouldBe("warm");
        
        var whisk = step.Items.OfType<CookwareItem>().First();
        whisk.Name.ShouldBe("whisk");
    }

    [Fact]
    public void ParsesModifierWithLineBreaks()
    {
        // this should be treated the same as Add @vegetables{500%g}(chopped into small pieces) to pot. 
        var source = """
            Add @vegetables{500%g}(chopped
            into small pieces) to pot.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var ingredient = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step.Items
            .OfType<IngredientItem>().First();

        ingredient.Note.ShouldNotBeNull();
        ingredient.Note.ShouldContain("chopped");
        ingredient.Note.ShouldContain("small pieces");
    }
}