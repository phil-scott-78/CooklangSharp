using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests.ErrorHandling;

public class ReportingTests
{
    [Fact]
    public void ReportsErrorForUnterminatedBrace()
    {
        var source = "Add @flour{200%g and @sugar{100";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeFalse();
        var firstError = result.Diagnostics.FirstOrDefault(d => d.DiagnosticType == DiagnosticType.Error);
        firstError.ShouldNotBeNull();
        firstError.Type.ShouldBe(ParseErrorType.UnterminatedBrace);
        firstError.Message.ShouldBe("Unterminated brace: missing '}'");
        firstError.Line.ShouldBe(1);
        firstError.Column.ShouldBe(11); // Position of the first unterminated brace in @flour{
        firstError.Context.ShouldNotBeNull();
    }
    
    [Fact]
    public void ReportsErrorForUnterminatedParenthesis()
    {
        var source = "Add @onion{1}(chopped and @garlic{2}";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeFalse();
        var firstError = result.Diagnostics.FirstOrDefault(d => d.DiagnosticType == DiagnosticType.Error);
        firstError.ShouldNotBeNull();
        firstError.Type.ShouldBe(ParseErrorType.UnterminatedParenthesis);
        firstError.Message.ShouldBe("Unterminated parenthesis: missing ')'");
        firstError.Line.ShouldBe(1);
        firstError.Column.ShouldBe(14); // After opening (
    }
    
    [Fact]
    public void ReportsErrorForInvalidTimerSyntax()
    {
        var source = "Wait ~{} for completion";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeFalse();
        var firstError = result.Diagnostics.FirstOrDefault(d => d.DiagnosticType == DiagnosticType.Error);
        firstError.ShouldNotBeNull();
        firstError.Type.ShouldBe(ParseErrorType.InvalidTimerSyntax);
        firstError.Message.ShouldBe("Invalid timer syntax: timer must have either a name or duration");
    }
    
    [Fact]
    public void ReportsErrorForInvalidTimerWithSpaceBeforeBrace()
    {
        var source = "Wait ~timer {5%minutes}";
        
        var result = CooklangParser.Parse(source);
        result.Success.ShouldBeFalse();
        var firstError = result.Diagnostics.FirstOrDefault(d => d.DiagnosticType == DiagnosticType.Error);
        firstError.ShouldNotBeNull();
        firstError.Type.ShouldBe(ParseErrorType.InvalidTimerSyntax);
        firstError.Message.ShouldBe("Invalid timer syntax: space not allowed before '{'");
        firstError.Line.ShouldBe(1);
        firstError.Column.ShouldBe(12); // Position of space before {
    }
    
    [Fact]
    public void ReportsErrorForDivisionByZeroInFraction()
    {
        var source = "Add @flour{1/0%cups}";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeFalse();
        var firstError = result.Diagnostics.FirstOrDefault(d => d.DiagnosticType == DiagnosticType.Error);
        firstError.ShouldNotBeNull();
        firstError.Type.ShouldBe(ParseErrorType.InvalidQuantity);
        firstError.Message.ShouldBe("Division by zero in fraction");
        firstError.Line.ShouldBe(1);
    }
    
    [Fact]
    public void AllowsValidSyntaxInStrictMode()
    {
        var validRecipe = """
            ---
            title: Test Recipe
            servings: 4
            ---
            
            Mix @flour{200%g} and @water{100%ml}.
            Bake in #oven{} for ~{30%minutes}.
            """;
        
        var result = CooklangParser.Parse(validRecipe);
        
        result.Success.ShouldBeTrue();
        var firstError = result.Diagnostics.FirstOrDefault(d => d.DiagnosticType == DiagnosticType.Error);
        firstError.ShouldBeNull();
        result.Recipe.ShouldNotBeNull();
    }
}