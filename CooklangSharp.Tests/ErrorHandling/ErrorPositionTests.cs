using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests.ErrorHandling;

public class ErrorPositionTests
{
    [Fact]
    public void ReportsMultiLineErrorCorrectly()
    {
        var source = """
            Mix @flour{200%g} with @water{100%ml}.
            
            Add @sugar{unterminated brace
            
            Mix well.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeFalse();
        var firstError = result.Diagnostics.FirstOrDefault(d => d.DiagnosticType == DiagnosticType.Error);
        firstError.ShouldNotBeNull();
        firstError.Type.ShouldBe(ParseErrorType.UnterminatedBrace);
        firstError.Line.ShouldBe(3); // Third line where the error occurs
        firstError.Context!.ShouldContain("{");
    }

    [Fact]
    public void DemonstrationPositionalErrorOnQuantity()
    {
        var invalidRecipe = """
                            Mix @flour{200%g} and @water{100%ml}.
                            Add @sugar{1/0%cups} to taste.
                            """;
        
        var result = CooklangParser.Parse(invalidRecipe);
        
        result.Success.ShouldBeFalse();
        var firstError = result.Diagnostics.FirstOrDefault(d => d.DiagnosticType == DiagnosticType.Error);
        firstError.ShouldNotBeNull();
        firstError.Type.ShouldBe(ParseErrorType.InvalidQuantity);
        firstError.Line.ShouldBe(2);
        firstError.Column.ShouldBe(12); // Position of '0' in "Add @sugar{1/0%cups} to taste."
        firstError.Message.ShouldBe("Division by zero in fraction");
    }

    [Fact]
    public void ProvideBetterErrorForComplexNesting()
    {
        var source = "Add @ingredient{quantity}(modifier with (nested but @another{broken";

        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeFalse();
        var firstError = result.Diagnostics.FirstOrDefault(d => d.DiagnosticType == DiagnosticType.Error);
        firstError.ShouldNotBeNull();
        // Should report the first error it encounters
        firstError.Line.ShouldBe(1);
        firstError.Context.ShouldNotBeNull();
    }

    [Fact]
    public void ReportsCorrectPositionForUnterminatedParenthesis()
    {
        var source = "Add @onion{1}(chopped and never closed";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeFalse();
        var firstError = result.Diagnostics.FirstOrDefault(d => d.DiagnosticType == DiagnosticType.Error);
        firstError.ShouldNotBeNull();
        firstError.Type.ShouldBe(ParseErrorType.UnterminatedParenthesis);
        firstError.Line.ShouldBe(1);
        firstError.Column.ShouldBe(14); // Position after the opening parenthesis
    }
}