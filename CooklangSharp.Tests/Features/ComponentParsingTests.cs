using System;
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
        ingredient.Quantity.ShouldBeNull(); // No quantity specified
        ingredient.Units.ShouldBe("");
    }
    
    [Theory]
    [InlineData("@sugar{1}", "sugar", 1, "", "Integer quantity without unit")]
    [InlineData("@flour{2.5}", "flour", 2.5, "", "Decimal quantity without unit")]
    [InlineData("@butter{1/2}", "butter", "1/2", "", "Fractional quantity without unit")]
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
        
        // Handle different expected quantity types
        if (expectedQuantity is int intValue)
        {
            ingredient.Quantity.ShouldBeOfType<RegularQuantity>()
                .Value.ShouldBe((double)intValue);
        }
        else if (expectedQuantity is double doubleValue)
        {
            if (doubleValue % 1 == 0) // Whole number as double
            {
                ingredient.Quantity.ShouldBeOfType<RegularQuantity>()
                    .Value.ShouldBe(doubleValue);
            }
            else // Fraction result or regular (like 0.5 from 1/2)
            {
                ingredient.Quantity?.GetNumericValue().ShouldBe(doubleValue);
            }
        }
        else if (expectedQuantity is string stringValue)
        {
            // Mixed fractions are stored as FractionalQuantity
            ingredient.Quantity.ShouldBeOfType<FractionalQuantity>();
        }
        
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
        
        if (expectedQuantity % 1 == 0) // Whole number
        {
            ingredient.Quantity.ShouldBeOfType<RegularQuantity>()
                .Value.ShouldBe(expectedQuantity);
        }
        else // Fraction result
        {
            ingredient.Quantity?.GetNumericValue().ShouldBe(expectedQuantity);
        }
        
        ingredient.Units.ShouldBe(expectedUnits);
    }
    
    [Theory]
    [InlineData("@salt{}", "salt", "", "Empty braces")]
    [InlineData("@pepper{  }", "pepper", "", "Empty braces with spaces")]
    [InlineData("@herbs{}", "herbs", "", "Empty amount specification")]
    public void Should_Parse_Ingredient_With_Empty_Braces(string input, string expectedName, string expectedUnits, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var ingredient = GetFirstIngredient(result);
        ingredient.Name.ShouldBe(expectedName);
        ingredient.Quantity.ShouldBeNull(); // Empty braces result in null quantity
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
        cookware.Quantity.ShouldBeOfType<RegularQuantity>()
            .Value.ShouldBe(expectedQuantity);
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
        cookware.Quantity.ShouldBeOfType<RegularQuantity>()
            .Value.ShouldBe((double)expectedQuantity);
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
        cookware.Quantity.ShouldBeOfType<RegularQuantity>()
            .Value.ShouldBe(expectedQuantity);
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
        timer.Quantity.ShouldBeOfType<RegularQuantity>()
            .Value.ShouldBe((double)expectedDuration);
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
        timer.Quantity.ShouldBeOfType<RegularQuantity>()
            .Value.ShouldBe((double)expectedDuration);
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
        timer.Quantity.ShouldBeOfType<RegularQuantity>()
            .Value.ShouldBe(expectedDuration);
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
        
        if (expectedDuration is double doubleValue)
        {
            timer.Quantity.ShouldBeOfType<FractionalQuantity>()
                .GetNumericValue().ShouldBe(doubleValue);
        }
        else if (expectedDuration is string)
        {
            // Mixed fraction
            timer.Quantity.ShouldBeOfType<FractionalQuantity>();
        }
        
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
        cookware.Quantity.ShouldBeOfType<RegularQuantity>()
            .Value.ShouldBe(expectedQuantity);
        cookware.Note.ShouldBe(expectedNote);
    }
    
    #endregion
    
    #region FractionalNumber Tests
    
    [Theory]
    [InlineData("@flour{1/2}", 0, 1, 2, "Simple fraction")]
    [InlineData("@sugar{1/4}", 0, 1, 4, "Quarter fraction")]
    [InlineData("@milk{3/4}", 0, 3, 4, "Three quarters")]
    [InlineData("@butter{2/3}", 0, 2, 3, "Two thirds")]
    [InlineData("@salt{1/8}", 0, 1, 8, "One eighth")]
    public void Should_Parse_Simple_Fractions_To_FractionalQuantity(string input, int expectedWhole, int expectedNumerator, int expectedDenominator, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var ingredient = GetFirstIngredient(result);
        
        var fraction = ingredient.Quantity.ShouldBeOfType<FractionalQuantity>();
        fraction.Whole.ShouldBe(expectedWhole);
        fraction.Numerator.ShouldBe(expectedNumerator);
        fraction.Denominator.ShouldBe(expectedDenominator);
        
        // Verify decimal conversion
        fraction.GetNumericValue().ShouldBe((double)expectedNumerator / expectedDenominator);
    }
    
    [Theory]
    [InlineData("@flour{2 1/3}", 2, 1, 3, "2 1/3", "Mixed fraction with single digit")]
    [InlineData("@sugar{1 1/2}", 1, 1, 2, "1 1/2", "One and a half")]
    [InlineData("@butter{3 3/4}", 3, 3, 4, "3 3/4", "Three and three quarters")]
    [InlineData("@oil{5 2/3}", 5, 2, 3, "5 2/3", "Five and two thirds")]
    public void Should_Parse_Mixed_Fractions_To_FractionalQuantity(string input, int expectedWhole, int expectedNumerator, int expectedDenominator, string expectedQuantityString, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var ingredient = GetFirstIngredient(result);
        
        var fraction = ingredient.Quantity.ShouldBeOfType<FractionalQuantity>();
        fraction.Whole.ShouldBe(expectedWhole);
        fraction.Numerator.ShouldBe(expectedNumerator);
        fraction.Denominator.ShouldBe(expectedDenominator);
        
        // Verify string representation
        fraction.ToString().ShouldBe(expectedQuantityString);
        
        // Verify decimal conversion
        var expectedDecimal = expectedWhole + (double)expectedNumerator / expectedDenominator;
        fraction.GetNumericValue().ShouldBe(expectedDecimal);
    }
    
    [Theory]
    [InlineData("@sugar{2}", 2, "Whole number")]
    [InlineData("@flour{5}", 5, "Whole number five")]
    [InlineData("@eggs{12}", 12, "Dozen")]
    public void Should_Parse_Whole_Numbers_As_RegularQuantity(string input, int expectedValue, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var ingredient = GetFirstIngredient(result);
        
        var regular = ingredient.Quantity.ShouldBeOfType<RegularQuantity>();
        regular.Value.ShouldBe((double)expectedValue);
        regular.ToString().ShouldBe(expectedValue.ToString());
    }
    
    [Theory]
    [InlineData("@sugar{0.5}", 0.5, "Decimal 0.5")]
    [InlineData("@flour{0.25}", 0.25, "Decimal 0.25")]
    [InlineData("@butter{0.75}", 0.75, "Decimal 0.75")]
    [InlineData("@milk{1.5}", 1.5, "Decimal 1.5")]
    [InlineData("@oil{2.25}", 2.25, "Decimal 2.25")]
    public void Should_Parse_Decimals_As_RegularQuantity(string input, double expectedValue, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var ingredient = GetFirstIngredient(result);
        
        var regular = ingredient.Quantity.ShouldBeOfType<RegularQuantity>();
        regular.Value.ShouldBe(expectedValue);
    }
    
    [Theory]
    [InlineData("@salt{some}", "some", "Non-numeric quantity")]
    [InlineData("@pepper{a pinch}", "a pinch", "Text quantity")]
    [InlineData("@herbs{}", "", "Empty quantity")]
    [InlineData("@spices{  }", "", "Whitespace quantity")]
    public void Should_Handle_Non_Numeric_Quantities(string input, string expectedQuantity, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var ingredient = GetFirstIngredient(result);
        
        if (expectedQuantity == "")
        {
            ingredient.Quantity.ShouldBeNull();
        }
        else
        {
            var text = ingredient.Quantity.ShouldBeOfType<TextQuantity>();
            text.Value.ShouldBe(expectedQuantity);
        }
    }
    
    [Theory]
    [InlineData("@flour{2 1/3%cups}", "cups", typeof(FractionalQuantity), "Mixed fraction with units")]
    [InlineData("@sugar{1/2%tsp}", "tsp", typeof(FractionalQuantity), "Simple fraction with units")]
    [InlineData("@butter{3%tbsp}", "tbsp", typeof(RegularQuantity), "Whole number with units")]
    [InlineData("@milk{0.75%liters}", "liters", typeof(RegularQuantity), "Decimal with units")]
    public void Should_Parse_Quantities_With_Units(string input, string expectedUnits, Type expectedQuantityType, string description)
    {
        var result = CooklangParser.Parse(input);
        
        result.Success.ShouldBeTrue($"Failed to parse: {description}");
        var ingredient = GetFirstIngredient(result);
        
        ingredient.Quantity.ShouldNotBeNull();
        ingredient.Quantity.GetType().ShouldBe(expectedQuantityType);
        ingredient.Units.ShouldBe(expectedUnits);
    }
    
    [Fact]
    public void FractionalQuantity_Should_Convert_To_String_Correctly()
    {
        // Whole number only
        var whole = new FractionalQuantity(5, 0, 1);
        whole.ToString().ShouldBe("5");
        
        // Simple fraction
        var fraction = new FractionalQuantity(0, 1, 2);
        fraction.ToString().ShouldBe("1/2");
        
        // Mixed fraction
        var mixed = new FractionalQuantity(2, 1, 3);
        mixed.ToString().ShouldBe("2 1/3");
    }
    
    [Fact]
    public void FractionalQuantity_Should_Calculate_Numeric_Value_Correctly()
    {
        // Whole number
        var whole = new FractionalQuantity(5, 0, 1);
        whole.GetNumericValue().ShouldBe(5.0);
        
        // Simple fraction
        var fraction = new FractionalQuantity(0, 1, 2);
        fraction.GetNumericValue().ShouldBe(0.5);
        
        // Mixed fraction
        var mixed = new FractionalQuantity(2, 1, 3);
        var mixedValue = mixed.GetNumericValue() ?? 0.0;
        mixedValue.ShouldBeInRange(2.333, 2.334);
    }
    
    [Fact]
    public void QuantityValue_Types_Should_Work_Correctly()
    {
        // RegularQuantity
        var regular = new RegularQuantity(5.5);
        regular.GetNumericValue().ShouldBe(5.5);
        regular.ToString().ShouldBe("5.5");
        
        var wholeRegular = new RegularQuantity(5.0);
        wholeRegular.ToString().ShouldBe("5");
        
        // TextQuantity
        var text = new TextQuantity("a pinch");
        text.GetNumericValue().ShouldBeNull();
        text.ToString().ShouldBe("a pinch");
    }
    
    #endregion
}