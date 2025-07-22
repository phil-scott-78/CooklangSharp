using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests.Core;

public class BasicParsingTests
{
    [Fact]
    public void ParsesPlainTextCorrectly()
    {
        var result = CooklangParser.Parse("Add a bit of chilli");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Sections.First().Content.Count.ShouldBe(1);

        var stepContent = result.Recipe.Sections.First().Content[0] as StepContent;
        stepContent.ShouldNotBeNull();
        stepContent.Step.Items.Count.ShouldBe(1);
        stepContent.Step.Items[0].Type.ShouldBe("text");
    }

    [Fact]
    public void ParsesIngredientWithQuantityAndUnits()
    {
        var result = CooklangParser.Parse("@chilli{3%items}");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Sections.First().Content.Count.ShouldBe(1);

        var stepContent = result.Recipe.Sections.First().Content[0] as StepContent;
        stepContent.ShouldNotBeNull();

        var ingredient = stepContent.Step.Items[0] as IngredientItem;
        ingredient.ShouldNotBeNull();
        ingredient.Name.ShouldBe("chilli");
        ingredient.Quantity.ShouldBeOfType<RegularQuantity>().Value.ShouldBe(3.0);
        ingredient.Units.ShouldBe("items");
    }

    [Fact]
    public void ParsesIngredientSingleWord()
    {
        var result = CooklangParser.Parse("@chilli");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Sections.First().Content.Count.ShouldBe(1);

        var stepContent = result.Recipe.Sections.First().Content[0] as StepContent;
        stepContent.ShouldNotBeNull();
        stepContent.Step.Items.Count.ShouldBe(1);

        var ingredient = stepContent.Step.Items[0] as IngredientItem;
        ingredient.ShouldNotBeNull();
        ingredient.Name.ShouldBe("chilli");
        ingredient.Quantity.ShouldBeNull(); // No quantity specified for @chilli
        ingredient.Units.ShouldBe("");
    }

    [Fact]
    public void ParsesIngredientWithSpaceInName()
    {
        var result = CooklangParser.Parse("@ingredient name{1}");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        var ingredient = result.Recipe.Sections[0].Content[0].ShouldBeOfType<StepContent>()
            .Step.Items[0].ShouldBeOfType<IngredientItem>();
        ingredient.Name.ShouldBe("ingredient name");
    }

    [Fact]
    public void ParsesCookwareWithSpaceInName()
    {
        var result = CooklangParser.Parse("#large pan{}");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        var cookware = result.Recipe.Sections[0].Content[0].ShouldBeOfType<StepContent>()
            .Step.Items[0].ShouldBeOfType<CookwareItem>();
        cookware.Name.ShouldBe("large pan");
    }

    [Fact]
    public void ParsesMixedTextAndIngredients()
    {
        var result = CooklangParser.Parse("Add @chilli{3%items}, @ginger{10%g} and @milk{1%l}.");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Sections.First().Content.Count.ShouldBe(1);

        var stepContent = result.Recipe.Sections.First().Content[0] as StepContent;
        stepContent.ShouldNotBeNull();
        stepContent.Step.Items.Count.ShouldBe(7);

        stepContent.Step.Items[0].Type.ShouldBe("text");
        stepContent.Step.Items[1].Type.ShouldBe("ingredient");
        stepContent.Step.Items[2].Type.ShouldBe("text");
        stepContent.Step.Items[3].Type.ShouldBe("ingredient");
        stepContent.Step.Items[4].Type.ShouldBe("text");
        stepContent.Step.Items[5].Type.ShouldBe("ingredient");
        stepContent.Step.Items[6].Type.ShouldBe("text");
    }

    [Fact]
    public void ParsesCommentOnlyLine()
    {
        var result = CooklangParser.Parse("-- testing comments");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Sections.First().Content.Count.ShouldBe(0);
    }
}