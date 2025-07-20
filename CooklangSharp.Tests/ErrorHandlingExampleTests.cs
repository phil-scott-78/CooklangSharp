using CooklangSharp.Core;
using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests;

/// <summary>
/// Demonstrates the enhanced error handling capabilities with practical examples
/// </summary>
public class ErrorHandlingExampleTests
{
    [Fact]
    public void DemonstrateBasicErrorReporting()
    {
        var invalidRecipe = """
            Add @ ingredient with space after @
            """;
        
        // Normal mode - parses successfully, treats invalid syntax as text
        var normalResult = CooklangParser.Parse(invalidRecipe, strictMode: false);
        normalResult.Success.ShouldBeTrue();
        
        // Strict mode - reports detailed error
        var strictResult = CooklangParser.Parse(invalidRecipe, strictMode: true);
        strictResult.Success.ShouldBeFalse();
        strictResult.Error.ShouldNotBeNull();
        strictResult.Error.Message.ShouldBe("Invalid ingredient syntax: space not allowed after '@'");
        strictResult.Error.Line.ShouldBe(1);
        strictResult.Error.Column.ShouldBe(6);
        strictResult.Error.Type.ShouldBe(ParseErrorType.InvalidIngredientSyntax);
    }
    
    [Fact]
    public void DemonstrateDetailedErrorContext()
    {
        var invalidRecipe = """
            Mix @flour{200%g} and @water{100%ml}.
            Add @sugar{1/0%cups} to taste.
            """;
        
        var result = CooklangParser.Parse(invalidRecipe, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldBe("Division by zero in fraction");
        result.Error.Line.ShouldBe(2); // Error is on the second line
        result.Error.Type.ShouldBe(ParseErrorType.InvalidQuantity);
        result.Error.Context.ShouldNotBeNull();
    }
    
    [Fact]
    public void DemonstratePositionalErrorReporting()
    {
        var invalidRecipe = """
            Mix ingredients:
            Add @flour{unterminated
            """;
        
        var result = CooklangParser.Parse(invalidRecipe, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.UnterminatedBrace);
        result.Error.Line.ShouldBe(2); // Error is on the second line
        result.Error.Message.ShouldBe("Unterminated brace: missing '}'");
    }

    [Fact]
    public void DemonstrationPositionalErrorOnQuantity()
    {
        var invalidRecipe = """
                            Mix @flour{200%g} and @water{100%ml}.
                            Add @sugar{1/0%cups} to taste.
                            """;
        
        var result = CooklangParser.Parse(invalidRecipe, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(ParseErrorType.InvalidQuantity);
        result.Error.Line.ShouldBe(2);
        result.Error.Column.ShouldBe(14); // Position of '0' in "Add @sugar{1/0%cups} to taste."
        result.Error.Message.ShouldBe("Division by zero in fraction");
    }
    
    [Fact]
    public void DemonstrateBackwardCompatibility()
    {
        var invalidButAcceptableRecipe = """
            Message @ example{}
            Recipe # 10{}
            It is ~ {5}
            """;
        
        // These should parse successfully in normal mode (backward compatibility)
        var normalResult = CooklangParser.Parse(invalidButAcceptableRecipe, strictMode: false);
        normalResult.Success.ShouldBeTrue();
        
        // But fail in strict mode
        var strictResult = CooklangParser.Parse(invalidButAcceptableRecipe, strictMode: true);
        strictResult.Success.ShouldBeFalse();
        strictResult.Error.ShouldNotBeNull();
        strictResult.Error.Type.ShouldBe(ParseErrorType.InvalidIngredientSyntax);
    }
    
    [Theory]
    [InlineData("@ingredient{unterminated", ParseErrorType.UnterminatedBrace)]
    [InlineData("@ingredient{1}(unterminated", ParseErrorType.UnterminatedParenthesis)]
    [InlineData("~{}", ParseErrorType.InvalidTimerSyntax)]
    [InlineData("@ingredient{1/0}", ParseErrorType.InvalidQuantity)]
    public void ReportsAppropriateErrorTypes(string invalidSyntax, ParseErrorType expectedType)
    {
        var result = CooklangParser.Parse(invalidSyntax, strictMode: true);
        
        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Type.ShouldBe(expectedType);
        result.Error.Line.ShouldBe(1);
        result.Error.Column.ShouldBeGreaterThan(0);
    }
}