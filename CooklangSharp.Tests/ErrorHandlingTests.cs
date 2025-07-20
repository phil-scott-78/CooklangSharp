using CooklangSharp.Core;
using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests;

public class ErrorHandlingTests
{
    [Fact]
    public void ReportsErrorForInvalidIngredientWithSpace()
    {
        var source = "Add @ ingredient with space after @";
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.InvalidIngredientSyntax);
        result.Error.Message.ShouldBe("Invalid ingredient syntax: space not allowed after '@'");
        result.Error.Line.ShouldBe(1);
        result.Error.Column.ShouldBe(6); // Position of space after @
        result.Error.Context.ShouldNotBeNull();
    }
    
    [Fact]
    public void ReportsErrorForInvalidCookwareWithSpace()
    {
        var source = "Use # pan with space after #";
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.InvalidCookwareSyntax);
        result.Error.Message.ShouldBe("Invalid cookware syntax: space not allowed after '#'");
        result.Error.Line.ShouldBe(1);
        result.Error.Column.ShouldBe(6); // Position of space after #
    }
    
    [Fact]
    public void ReportsErrorForUnterminatedBrace()
    {
        var source = "Add @flour{200%g and @sugar{100";
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.UnterminatedBrace);
        result.Error.Message.ShouldBe("Unterminated brace: missing '}'");
        result.Error.Line.ShouldBe(1);
        result.Error.Column.ShouldBe(11); // Position of the first unterminated brace in @flour{
        result.Error.Context.ShouldNotBeNull();
    }
    
    [Fact]
    public void ReportsErrorForUnterminatedParenthesis()
    {
        var source = "Add @onion{1}(chopped and @garlic{2}";
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.UnterminatedParenthesis);
        result.Error.Message.ShouldBe("Unterminated parenthesis: missing ')'");
        result.Error.Line.ShouldBe(1);
        result.Error.Column.ShouldBe(14); // After opening (
    }
    
    [Fact]
    public void ReportsErrorForInvalidTimerSyntax()
    {
        var source = "Wait ~{} for completion";
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.InvalidTimerSyntax);
        result.Error.Message.ShouldBe("Invalid timer syntax: timer must have either a name or duration");
    }
    
    [Fact]
    public void ReportsErrorForInvalidTimerWithSpaceBeforeBrace()
    {
        var source = "Wait ~timer {5%minutes}";
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.InvalidTimerSyntax);
        result.Error.Message.ShouldBe("Invalid timer syntax: space not allowed before '{'");
        result.Error.Line.ShouldBe(1);
        result.Error.Column.ShouldBe(12); // Position of space before {
    }
    
    [Fact]
    public void ReportsErrorForDivisionByZeroInFraction()
    {
        var source = "Add @flour{1/0%cups}";
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.InvalidQuantity);
        result.Error.Message.ShouldBe("Division by zero in fraction");
        result.Error.Line.ShouldBe(1);
    }
    
    [Fact]
    public void ReportsErrorForInvalidMetadataFormat()
    {
        var source = """
            ---
            invalid metadata line without colon
            ---
            """;
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.InvalidMetadata);
        result.Error.Message.ShouldBe("Invalid metadata format. Expected 'key: value'");
        result.Error.Line.ShouldBe(2); // Second line in the source
        result.Error.Context.ShouldBe("invalid metadata line without colon");
    }
    
    [Fact]
    public void ReportsErrorForEmptyMetadataKey()
    {
        var source = """
            ---
            : value without key
            ---
            """;
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.InvalidMetadata);
        result.Error.Message.ShouldBe("Metadata key cannot be empty");
        result.Error.Line.ShouldBe(2);
    }
    
    [Fact]
    public void ReportsMultiLineErrorCorrectly()
    {
        var source = """
            Mix @flour{200%g} with @water{100%ml}.
            
            Add @sugar{unterminated brace
            
            Mix well.
            """;
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.UnterminatedBrace);
        result.Error.Line.ShouldBe(3); // Third line where the error occurs
        result.Error.Context.ShouldContain("sugar{unterminated brace");
    }
    
    [Fact]
    public void ReportsErrorInSectionHeader()
    {
        var source = """
            not a section header
            
            Mix @flour{200%g}.
            """;
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        // This should succeed since it's just text, not an invalid section
        result.Success.ShouldBeTrue();
    }
    
    [Fact]
    public void ReportsCorrectPositionForInvalidFraction()
    {
        var source = "Use @flour{1/2/3%cups} in the recipe";
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.InvalidQuantity);
        result.Error.Message.ShouldBe("Invalid fraction format: '1/2/3'");
        result.Error.Line.ShouldBe(1);
    }
    
    [Fact]
    public void ProvideBetterErrorForComplexNesting()
    {
        var source = "Add @ingredient{quantity}(modifier with (nested but @another{broken";
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        // Should report the first error it encounters
        result.Error.Line.ShouldBe(1);
        result.Error.Context.ShouldNotBeNull();
    }
    
    [Theory]
    [InlineData("~timer {space}", ParseErrorType.InvalidTimerSyntax)]
    public void ReportsCorrectErrorTypeForSpaceBeforeBrace(string input, ParseErrorType expectedType)
    {
        var result = CooklangParser.Parse(input, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(expectedType);
        result.Error.Line.ShouldBe(1);
    }
    
    [Fact]
    public void IngredientWithSpaceInNameIsValid()
    {
        // Ingredient names with spaces should be valid - this is different from space after @
        var result = CooklangParser.Parse("@ingredient name{1}");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        var ingredient = result.Recipe.Sections[0].Content[0].ShouldBeOfType<StepContent>()
            .Step.Items[0].ShouldBeOfType<IngredientItem>();
        ingredient.Name.ShouldBe("ingredient name");
    }
    
    [Fact]
    public void CookwareWithSpaceInNameIsValid()
    {
        // Cookware names with spaces should be valid - this is different from space after #
        var result = CooklangParser.Parse("#large pan{}");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        var cookware = result.Recipe.Sections[0].Content[0].ShouldBeOfType<StepContent>()
            .Step.Items[0].ShouldBeOfType<CookwareItem>();
        cookware.Name.ShouldBe("large pan");
    }
    
    [Fact]
    public void SuccessfulParseHasNoError()
    {
        var source = "Mix @flour{200%g} with @water{100%ml}.";
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeTrue();
        result.Error.ShouldBeNull();
        result.Recipe.ShouldNotBeNull();
    }
    
    [Fact]
    public void LegacyErrorPropertiesStillWork()
    {
        var source = "Add @ ingredient with space";
        
        var result = CooklangParser.Parse(source, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNull();
        result.ErrorPosition.ShouldNotBeNull();
        
        // Legacy ErrorMessage should match Error.Message
        result.ErrorMessage.ShouldBe(result.Error!.Message);
    }
}