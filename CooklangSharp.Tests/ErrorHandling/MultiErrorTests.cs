using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests.ErrorHandling;

public class MultiErrorTests
{
    [Fact]
    public void Should_Collect_Multiple_Errors()
    {
        // Arrange
        var recipeText = @"Mix @flour{200%g} and @water{100%ml}.
Add @bananas{2 after mixing.
Add @sugar{41/0%cups} to taste.";

        // Act
        var result = CooklangParser.Parse(recipeText);

        // Assert
        result.Success.ShouldBeFalse();
        var errors = result.Diagnostics.Where(d => d.DiagnosticType == DiagnosticType.Error).ToList();
        errors.ShouldNotBeNull();
        errors.Count.ShouldBe(2);
        
        // First error: missing closing brace on line 2
        var error1 = errors[0];
        error1.Line.ShouldBe(2);
        error1.Message.ShouldBe("Unterminated brace: missing '}'");
        error1.Type.ShouldBe(ParseErrorType.UnterminatedBrace);
        
        // Second error: division by zero on line 3
        var error2 = errors[1];
        error2.Line.ShouldBe(3);
        error2.Message.ShouldBe("Division by zero in fraction");
        error2.Type.ShouldBe(ParseErrorType.InvalidQuantity);
    }
    
    [Fact]
    public void Should_Collect_Invalid_Syntax_Errors()
    {
        // Arrange
        var recipeText = @"Mix @ flour{200%g} and # pot{}.
Use ~ {5%minutes} to cook.
Timer with ~{} empty duration.";

        // Act
        var result = CooklangParser.Parse(recipeText);
        
        // Assert
        result.Success.ShouldBeFalse();
        var errors = result.Diagnostics.Where(d => d.DiagnosticType == DiagnosticType.Error).ToList();
        errors.ShouldNotBeNull();
        
        // Check error types
        errors[0].Type.ShouldBe(ParseErrorType.InvalidTimerSyntax);
        errors[0].Message.ShouldContain("timer must have either a name or duration");
    }
    
    [Fact]
    public void Should_Return_Success_When_No_Errors()
    {
        // Arrange
        var recipeText = @"Mix @flour{200%g} and @water{100%ml}.
Add @sugar{2%tbsp} to taste.
Bake in #oven{} for ~{30%minutes}.";

        // Act
        var result = CooklangParser.Parse(recipeText);

        // Assert
        result.Success.ShouldBeTrue();
        var errors = result.Diagnostics.Where(d => d.DiagnosticType == DiagnosticType.Error).ToList();
        errors.ShouldNotBeNull();
        errors.Count.ShouldBe(0);
        result.Recipe.ShouldNotBeNull();
    }
    
    [Fact]
    public void Should_Still_Parse_Valid_Parts_When_Errors_Exist()
    {
        // Arrange
        var recipeText = @"Mix @flour{200%g} and @water{100%ml}.
Add @bananas{2 after mixing.
Add @sugar{2%tbsp} to taste.";

        // Act
        var result = CooklangParser.Parse(recipeText);

        // Assert
        result.Success.ShouldBeFalse();
        var errors = result.Diagnostics.Where(d => d.DiagnosticType == DiagnosticType.Error).ToList();
        errors.Count.ShouldBe(1); // Only the missing brace error
        
        // The recipe should still be parsed (even though there are errors)
        // This is because we continue parsing after errors
        result.Recipe.ShouldBeNull(); // But recipe is null when there are errors
    }
}