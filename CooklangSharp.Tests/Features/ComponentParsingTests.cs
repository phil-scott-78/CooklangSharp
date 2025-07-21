using System.Collections.Generic;
using System.Linq;
using CooklangSharp;
using CooklangSharp.Models;
using Shouldly;
using Xunit;

namespace CooklangSharp.Tests.Features;

public class ComponentParsingTests
{
    #region Helper Methods
    
    private static IngredientItem GetFirstIngredient(ParseResult result)
    {
        return result.Recipe!.Sections[0].Content[0].ShouldBeOfType<StepContent>()
            .Step.Items.OfType<IngredientItem>().First();
    }
    
    private static CookwareItem GetFirstCookware(ParseResult result)
    {
        return result.Recipe!.Sections[0].Content[0].ShouldBeOfType<StepContent>()
            .Step.Items.OfType<CookwareItem>().First();
    }
    
    private static TimerItem GetFirstTimer(ParseResult result)
    {
        return result.Recipe!.Sections[0].Content[0].ShouldBeOfType<StepContent>()
            .Step.Items.OfType<TimerItem>().First();
    }
    
    private static Step GetFirstStep(ParseResult result)
    {
        return result.Recipe!.Sections[0].Content[0].ShouldBeOfType<StepContent>().Step;
    }
    
    #endregion
    
    #region Ingredient Tests
    
    [Theory]
    [InlineData("@salt", "salt", "Simple ingredient without amount")]
    [InlineData("@pepper", "pepper", "Single word ingredient")]
    [InlineData("@red bell pepper{}", "red bell pepper", "Multi-word ingredient")]
    [InlineData("@olive oil{}", "olive oil", "Two-word ingredient")]
    [InlineData("@black-eyed peas{}", "black-eyed peas", "Ingredient with hyphen")]
    [InlineData("@crème fraîche{}", "crème fraîche", "Ingredient with accented characters")]
    public void Should_Parse_Ingredient_Names(string input, string expectedName, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var ingredient = GetFirstIngredient(result);
        ingredient.Name.ShouldBe(expectedName);
        ingredient.Quantity.ShouldBe("some");
        ingredient.Units.ShouldBe("");
    }
    
    [Theory]
    [InlineData("@sugar{1}", "sugar", 1, "", "Integer quantity without unit")]
    [InlineData("@flour{2.5}", "flour", 2.5, "", "Decimal quantity without unit")]
    [InlineData("@butter{1/2}", "butter", 0.5, "", "Fractional quantity without unit")]
    [InlineData("@eggs{3}", "eggs", 3, "", "Simple integer quantity")]
    [InlineData("@milk{1.75}", "milk", 1.75, "", "Decimal with two decimal places")]
    [InlineData("@yeast{0.25}", "yeast", 0.25, "", "Decimal less than one")]
    [InlineData("@chocolate chips{3/4}", "chocolate chips", 0.75, "", "Fraction less than one")]
    [InlineData("@walnuts{1 1/2}", "walnuts", "1 1/2", "", "Mixed fraction")]
    public void Should_Parse_Ingredient_Quantities(string input, string expectedName, object expectedQuantity, string expectedUnits, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var ingredient = GetFirstIngredient(result);
        ingredient.Name.ShouldBe(expectedName);
        ingredient.Quantity.ShouldBe(expectedQuantity);
        ingredient.Units.ShouldBe(expectedUnits);
    }
    
    [Theory]
    [InlineData("@water{1%cup}", "water", 1, "cup", "Simple quantity with unit")]
    [InlineData("@flour{500%g}", "flour", 500, "g", "Metric unit")]
    [InlineData("@sugar{2%tablespoons}", "sugar", 2, "tablespoons", "Long unit name")]
    [InlineData("@rice{1.5%cups}", "rice", 1.5, "cups", "Decimal quantity with unit")]
    [InlineData("@butter{1/4%pound}", "butter", 0.25, "pound", "Fractional quantity with unit")]
    [InlineData("@olive oil{2%tbsp}", "olive oil", 2, "tbsp", "Abbreviation unit")]
    [InlineData("@heavy cream{200%ml}", "heavy cream", 200, "ml", "Metric liquid measure")]
    [InlineData("@baking powder{1%tsp}", "baking powder", 1, "tsp", "Common abbreviation")]
    public void Should_Parse_Ingredient_With_Units(string input, string expectedName, double expectedQuantity, string expectedUnits, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var ingredient = GetFirstIngredient(result);
        ingredient.Name.ShouldBe(expectedName);
        ingredient.Quantity.ShouldBe(expectedQuantity);
        ingredient.Units.ShouldBe(expectedUnits);
    }
    
    [Theory]
    [InlineData("@salt{}", "salt", "some", "", "Empty braces")]
    [InlineData("@pepper{  }", "pepper", "some", "", "Empty braces with spaces")]
    [InlineData("@herbs{}", "herbs", "some", "", "Empty amount specification")]
    public void Should_Parse_Ingredient_With_Empty_Braces(string input, string expectedName, string expectedQuantity, string expectedUnits, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var ingredient = GetFirstIngredient(result);
        ingredient.Name.ShouldBe(expectedName);
        ingredient.Quantity.ShouldBe(expectedQuantity);
        ingredient.Units.ShouldBe(expectedUnits);
    }
    
    [Theory]
    [InlineData("  @salt", "salt", "Leading spaces")]
    [InlineData("@salt  ", "salt", "Trailing spaces")]
    [InlineData("@red  bell  pepper{}", "red  bell  pepper", "Multiple spaces between words")]
    public void Should_Handle_Ingredient_Whitespace(string input, string expectedName, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var ingredient = GetFirstIngredient(result);
        ingredient.Name.ShouldBe(expectedName);
    }
    
    #endregion
    
    #region Cookware Tests
    
    [Theory]
    [InlineData("#pot", "pot", 1, "Simple cookware")]
    [InlineData("#skillet", "skillet", 1, "Single word cookware")]
    [InlineData("#cast iron skillet{}", "cast iron skillet", 1, "Multi-word cookware")]
    [InlineData("#mixing bowl{}", "mixing bowl", 1, "Two-word cookware")]
    [InlineData("#non-stick pan{}", "non-stick pan", 1, "Cookware with hyphen")]
    [InlineData("#sauté pan{}", "sauté pan", 1, "Cookware with accented character")]
    public void Should_Parse_Cookware_Names(string input, string expectedName, object expectedQuantity, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var cookware = GetFirstCookware(result);
        cookware.Name.ShouldBe(expectedName);
        cookware.Quantity.ShouldBe(expectedQuantity);
    }
    
    [Theory]
    [InlineData("#pot{1}", "pot", 1, "Single pot")]
    [InlineData("#bowls{2}", "bowls", 2, "Multiple bowls")]
    [InlineData("#sheet pan{3}", "sheet pan", 3, "Multiple sheet pans")]
    [InlineData("#measuring cups{4}", "measuring cups", 4, "Set of measuring cups")]
    [InlineData("#knives{2}", "knives", 2, "Multiple knives")]
    public void Should_Parse_Cookware_Quantities(string input, string expectedName, int expectedQuantity, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var cookware = GetFirstCookware(result);
        cookware.Name.ShouldBe(expectedName);
        cookware.Quantity.ShouldBe(expectedQuantity);
    }
    
    [Theory]
    [InlineData("#whisk{}", "whisk", 1, "Empty braces")]
    [InlineData("#spatula{  }", "spatula", 1, "Empty braces with spaces")]
    [InlineData("#ladle{}", "ladle", 1, "No quantity specified")]
    public void Should_Parse_Cookware_With_Empty_Braces(string input, string expectedName, object expectedQuantity, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var cookware = GetFirstCookware(result);
        cookware.Name.ShouldBe(expectedName);
        cookware.Quantity.ShouldBe(expectedQuantity);
    }
    
    [Theory]
    [InlineData("  #pot", "pot", "Leading spaces")]
    [InlineData("#pot  ", "pot", "Trailing spaces")]
    [InlineData("#large  heavy  pot{}", "large  heavy  pot", "Multiple spaces between words")]
    public void Should_Handle_Cookware_Whitespace(string input, string expectedName, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var cookware = GetFirstCookware(result);
        cookware.Name.ShouldBe(expectedName);
    }
    
    #endregion
    
    #region Timer Tests
    
    [Theory]
    [InlineData("~{5%minutes}", "", 5, "minutes", "Anonymous timer")]
    [InlineData("~{10%min}", "", 10, "min", "Anonymous timer with abbreviation")]
    [InlineData("~{30%seconds}", "", 30, "seconds", "Timer in seconds")]
    [InlineData("~{2%hours}", "", 2, "hours", "Timer in hours")]
    [InlineData("~{45%mins}", "", 45, "mins", "Timer with mins abbreviation")]
    public void Should_Parse_Anonymous_Timers(string input, string expectedName, int expectedDuration, string expectedUnit, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var timer = GetFirstTimer(result);
        timer.Name.ShouldBe(expectedName);
        timer.Quantity.ShouldBe(expectedDuration);
        timer.Units.ShouldBe(expectedUnit);
    }
    
    [Theory]
    [InlineData("~bake{25%minutes}", "bake", 25, "minutes", "Named timer")]
    [InlineData("~rest{10%min}", "rest", 10, "min", "Named timer with abbreviation")]
    [InlineData("~marinate{2%hours}", "marinate", 2, "hours", "Named timer in hours")]
    [InlineData("~simmer{30%minutes}", "simmer", 30, "minutes", "Cooking action timer")]
    [InlineData("~cool down{15%mins}", "cool down", 15, "mins", "Multi-word timer name")]
    public void Should_Parse_Named_Timers(string input, string expectedName, int expectedDuration, string expectedUnit, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var timer = GetFirstTimer(result);
        timer.Name.ShouldBe(expectedName);
        timer.Quantity.ShouldBe(expectedDuration);
        timer.Units.ShouldBe(expectedUnit);
    }
    
    [Theory]
    [InlineData("~{1.5%hours}", "", 1.5, "hours", "Decimal hours")]
    [InlineData("~{2.5%minutes}", "", 2.5, "minutes", "Decimal minutes")]
    [InlineData("~{0.5%hours}", "", 0.5, "hours", "Half hour as decimal")]
    [InlineData("~cook{3.25%mins}", "cook", 3.25, "mins", "Named timer with decimal")]
    public void Should_Parse_Timers_With_Decimals(string input, string expectedName, double expectedDuration, string expectedUnit, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var timer = GetFirstTimer(result);
        timer.Name.ShouldBe(expectedName);
        timer.Quantity.ShouldBe(expectedDuration);
        timer.Units.ShouldBe(expectedUnit);
    }
    
    [Theory]
    [InlineData("~{1/2%hour}", "", 0.5, "hour", "Fractional timer")]
    [InlineData("~{3/4%hours}", "", 0.75, "hours", "Three quarters hour")]
    [InlineData("~{1 1/2%minutes}", "", "1 1/2", "minutes", "Mixed fraction timer")]
    public void Should_Parse_Timers_With_Fractions(string input, string expectedName, object expectedDuration, string expectedUnit, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var timer = GetFirstTimer(result);
        timer.Name.ShouldBe(expectedName);
        timer.Quantity.ShouldBe(expectedDuration);
        timer.Units.ShouldBe(expectedUnit);
    }
    
    [Theory]
    [InlineData("  ~{5%minutes}", "", "Leading spaces")]
    [InlineData("~{5%minutes}  ", "", "Trailing spaces")]
    [InlineData("~slow  cook{2%hours}", "slow  cook", "Multi-word timer with extra spaces")]
    public void Should_Handle_Timer_Whitespace(string input, string expectedName, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var timer = GetFirstTimer(result);
        timer.Name.ShouldBe(expectedName);
    }
    
    #endregion
    
    #region Combined Component Tests
    
    [Theory]
    [InlineData("Add @salt{1%tsp} to the #pot{1} and cook for ~{5%minutes}.", 1, 1, 1, "All three components")]
    [InlineData("Mix @flour{2%cups} and @water{1%cup} in a #bowl.", 2, 1, 0, "Multiple ingredients")]
    [InlineData("Bake in the #oven{} for ~baking{30%minutes} and ~cooling{10%minutes}.", 0, 1, 2, "Multiple timers")]
    [InlineData("Use #knife{1} and #cutting board{1} to prepare @vegetables{}.", 1, 2, 0, "Multiple cookware")]
    public void Should_Parse_Multiple_Components(string input, int expectedIngredients, int expectedCookware, int expectedTimers, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var step = GetFirstStep(result);
        step.Items.OfType<IngredientItem>().Count().ShouldBe(expectedIngredients);
        step.Items.OfType<CookwareItem>().Count().ShouldBe(expectedCookware);
        step.Items.OfType<TimerItem>().Count().ShouldBe(expectedTimers);
    }
    
    [Theory]
    [InlineData("@salt@pepper", 2, "Adjacent ingredients without space")]
    [InlineData("#pot#pan", 2, "Adjacent cookware without space")]
    [InlineData("~{5%min}~{10%min}", 2, "Adjacent timers without space")]
    [InlineData("@salt{1%tsp}@pepper{1/2%tsp}", 2, "Adjacent ingredients with amounts")]
    public void Should_Parse_Adjacent_Components(string input, int expectedCount, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var step = GetFirstStep(result);
        
        if (input.StartsWith("@"))
            step.Items.OfType<IngredientItem>().Count().ShouldBe(expectedCount);
        else if (input.StartsWith("#"))
            step.Items.OfType<CookwareItem>().Count().ShouldBe(expectedCount);
        else if (input.StartsWith("~"))
            step.Items.OfType<TimerItem>().Count().ShouldBe(expectedCount);
    }
    
    #endregion
    
    #region Edge Cases
    
    [Theory]
    [InlineData("@", "Lone @ symbol")]
    [InlineData("#", "Lone # symbol")]
    [InlineData("~", "Lone ~ symbol")]
    public void Should_Handle_Edge_Cases(string input, string description)
    {
        var result = CooklangParser.Parse(input);
        
        // These should still parse successfully, but may produce different results
        result.Success.ShouldBeTrue($"Failed to parse edge case: {description}");
    }
    
    [Theory]
    [InlineData("@ingredient with parentheses{}", "ingredient with parentheses", "Complex name without special chars")]
    [InlineData("#pot/pan combo{}", "pot/pan combo", "Slash in cookware name")]
    [InlineData("~timer-name{5%min}", "timer-name", "Hyphen in timer name")]
    [InlineData("@café's special blend{}", "café's special blend", "Apostrophe and accents")]
    public void Should_Handle_Special_Characters_In_Names(string input, string expectedName, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var step = GetFirstStep(result);
        
        if (input.StartsWith("@"))
            step.Items.OfType<IngredientItem>().First().Name.ShouldBe(expectedName);
        else if (input.StartsWith("#"))
            step.Items.OfType<CookwareItem>().First().Name.ShouldBe(expectedName);
        else if (input.StartsWith("~"))
            step.Items.OfType<TimerItem>().First().Name.ShouldBe(expectedName);
    }
    
    #endregion
    
    #region Note Tests
    
    [Theory]
    [InlineData("@carrots{2}(diced)", "carrots", 2, "", "diced", "Ingredient with preparation note")]
    [InlineData("@onion{1}(finely chopped)", "onion", 1, "", "finely chopped", "Ingredient with detailed prep")]
    [InlineData("@flour{2%cups}(sifted)", "flour", 2, "cups", "sifted", "Ingredient with unit and note")]
    public void Should_Parse_Components_With_Notes(string input, string expectedName, object expectedQuantity, string expectedUnits, string expectedNote, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var step = GetFirstStep(result);
        
        if (input.StartsWith("@"))
        {
            var ingredient = step.Items.OfType<IngredientItem>().First();
            ingredient.Name.ShouldBe(expectedName);
            ingredient.Quantity.ShouldBe(expectedQuantity);
            ingredient.Units.ShouldBe(expectedUnits);
            ingredient.Note.ShouldBe(expectedNote);
        }
        else if (input.StartsWith("#"))
        {
            var cookware = step.Items.OfType<CookwareItem>().First();
            cookware.Name.ShouldBe(expectedName);
            cookware.Quantity.ShouldBe(expectedQuantity);
            cookware.Note.ShouldBe(expectedNote);
        }
    }
    
    [Theory]
    [InlineData("#pan{}(non-stick preferred)", "pan", 1, "non-stick preferred", "Cookware with preference note")]
    [InlineData("#baking dish{1}(9x13 inch)", "baking dish", 1, "9x13 inch", "Cookware with size note")]
    public void Should_Parse_Cookware_With_Notes(string input, string expectedName, object expectedQuantity, string expectedNote, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var cookware = GetFirstCookware(result);
        cookware.Name.ShouldBe(expectedName);
        cookware.Quantity.ShouldBe(expectedQuantity);
        cookware.Note.ShouldBe(expectedNote);
    }
    
    #endregion
}