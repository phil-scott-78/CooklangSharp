using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests.Core;

public class QuantityParsingTests
{
    [Fact]
    public void ParsesFractionQuantity()
    {
        var result = CooklangParser.Parse("@milk{1/2%cup}");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Sections.First().Content.Count.ShouldBe(1);

        var stepContent = result.Recipe.Sections.First().Content[0] as StepContent;
        stepContent.ShouldNotBeNull();
        stepContent.Step.Items.Count.ShouldBe(1);

        var ingredient = stepContent.Step.Items[0] as IngredientItem;
        ingredient.ShouldNotBeNull();
        ingredient.Name.ShouldBe("milk");
        ingredient.Quantity?.GetNumericValue().ShouldBe(0.5);
        ingredient.Units.ShouldBe("cup");
    }

    [Theory]
    [InlineData("1/2", 0.5)]
    [InlineData("1/4", 0.25)]
    [InlineData("3/4", 0.75)]
    [InlineData("1.5", 1.5)]
    [InlineData("2", 2.0)]
    public void ParsesFractionalIngredientQuantities(string quantityText, double expectedValue)
    {
        var source = $"Add @butter{{{quantityText}%cup}} to the mixture.";

        var result = CooklangParser.Parse(source);

        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();

        var section = result.Recipe.Sections[0];
        var stepContent = section.Content[0] as StepContent;
        stepContent.ShouldNotBeNull();
        var step = stepContent.Step;

        var ingredient = step.Items.OfType<IngredientItem>().First();
        ingredient.ShouldNotBeNull();
        ingredient.Name.ShouldBe("butter");
        ingredient.Quantity?.GetNumericValue().ShouldBe(expectedValue);
        ingredient.Units.ShouldBe("cup");
    }

    [Theory]
    [InlineData("@ingredient{1}", 1.0)]
    [InlineData("@ingredient{0.5}", 0.5)]
    [InlineData("@ingredient{10}", 10.0)]
    [InlineData("@ingredient{0}", 0.0)]
    public void ParsesNumericQuantities(string input, double expectedQuantity)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue();
        var ingredient = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step.Items[0]
            .ShouldBeOfType<IngredientItem>();
        ingredient.Quantity?.GetNumericValue().ShouldBe(expectedQuantity);
    }

    [Theory]
    [InlineData("@ingredient{some}", "some")]
    [InlineData("@ingredient{a pinch}", "a pinch")]
    [InlineData("@ingredient{to taste}", "to taste")]
    public void ParsesTextQuantities(string input, string expectedQuantity)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue();
        var ingredient = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step.Items[0]
            .ShouldBeOfType<IngredientItem>();
        ingredient.Quantity.ShouldNotBeNull();
        ingredient.Quantity.ShouldBeAssignableTo<TextQuantity>();
        ((TextQuantity) ingredient.Quantity).Value.ShouldBe(expectedQuantity);
    }

    [Theory]
    [InlineData("@ingredient{1%cup}", "cup")]
    [InlineData("@ingredient{2%tablespoons}", "tablespoons")]
    [InlineData("@ingredient{500%ml}", "ml")]
    [InlineData("@ingredient{1%}", "")]
    public void ParsesUnitsCorrectly(string input, string expectedUnits)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue();
        var ingredient = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step.Items[0]
            .ShouldBeOfType<IngredientItem>();
        ingredient.Units.ShouldBe(expectedUnits);
    }

    [Fact]
    public void ParsesIngredientWithoutQuantityOrUnits()
    {
        var result = CooklangParser.Parse("@salt");
        
        result.Success.ShouldBeTrue();
        var ingredient = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step.Items[0]
            .ShouldBeOfType<IngredientItem>();
        ingredient.Name.ShouldBe("salt");
        ingredient.Quantity.ShouldBeNull();

        ingredient.Units.ShouldBe("");
    }

    [Fact]
    public void ParsesIngredientWithEmptyBraces()
    {
        var result = CooklangParser.Parse("@salt{}");
        
        result.Success.ShouldBeTrue();
        var ingredient = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step.Items[0]
            .ShouldBeOfType<IngredientItem>();
        ingredient.Name.ShouldBe("salt");
        ingredient.Quantity.ShouldBeNull();
        ingredient.Units.ShouldBe("");
    }

    [Fact]
    public void ParsesCookwareQuantity()
    {
        var result = CooklangParser.Parse("#pan{2}");
        
        result.Success.ShouldBeTrue();
        var cookware = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step.Items[0]
            .ShouldBeOfType<CookwareItem>();
        cookware.Name.ShouldBe("pan");
        cookware.Quantity?.GetNumericValue().ShouldBe(2.0);
    }

    [Fact]
    public void ParsesTimerQuantityAndUnits()
    {
        var result = CooklangParser.Parse("~timer{5%minutes}");
        
        result.Success.ShouldBeTrue();
        var timer = result.Recipe.Sections[0].Content[0]
            .ShouldBeOfType<StepContent>().Step.Items[0]
            .ShouldBeOfType<TimerItem>();
        timer.Name.ShouldBe("timer");
        timer.Quantity?.GetNumericValue().ShouldBe(5.0);
        timer.Units.ShouldBe("minutes");
    }
}