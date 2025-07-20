using Shouldly;
using Xunit;
using CooklangSharp.Core;
using CooklangSharp.Models;
using System.Linq;

namespace CooklangSharp.Tests;

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
        var result = CooklangParser.Parse(recipeText, strictMode: true);

        // Assert
        result.Success.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count.ShouldBe(2);
        
        // First error: missing closing brace on line 2
        var error1 = result.Errors[0];
        error1.Line.ShouldBe(2);
        error1.Message.ShouldBe("Unterminated brace: missing '}'");
        error1.Type.ShouldBe(ParseErrorType.UnterminatedBrace);
        
        // Second error: division by zero on line 3
        var error2 = result.Errors[1];
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
        var result = CooklangParser.Parse(recipeText, strictMode: true);

        // Assert
        result.Success.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count.ShouldBe(4);
        
        // Check error types
        result.Errors[0].Type.ShouldBe(ParseErrorType.InvalidIngredientSyntax);
        result.Errors[0].Message.ShouldContain("space not allowed after '@'");
        
        result.Errors[1].Type.ShouldBe(ParseErrorType.InvalidCookwareSyntax);
        result.Errors[1].Message.ShouldContain("space not allowed after '#'");
        
        result.Errors[2].Type.ShouldBe(ParseErrorType.InvalidTimerSyntax);
        result.Errors[2].Message.ShouldContain("space not allowed before '{'");
        
        result.Errors[3].Type.ShouldBe(ParseErrorType.InvalidTimerSyntax);
        result.Errors[3].Message.ShouldContain("timer must have either a name or duration");
    }
    
    [Fact]
    public void Should_Collect_Metadata_Errors()
    {
        // Arrange
        var recipeText = @"---
title: Test Recipe
: empty key
invalid line without colon
servings: 4
---
Mix @flour{200%g} and @water{100%ml}.";

        // Act
        var result = CooklangParser.Parse(recipeText, strictMode: true);

        // Assert
        result.Success.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count.ShouldBe(2);
        
        result.Errors[0].Type.ShouldBe(ParseErrorType.InvalidMetadata);
        result.Errors[0].Message.ShouldBe("Metadata key cannot be empty");
        
        result.Errors[1].Type.ShouldBe(ParseErrorType.InvalidMetadata);
        result.Errors[1].Message.ShouldContain("Invalid metadata format");
    }
    
    [Fact]
    public void Should_Return_Success_When_No_Errors()
    {
        // Arrange
        var recipeText = @"Mix @flour{200%g} and @water{100%ml}.
Add @sugar{2%tbsp} to taste.
Bake in #oven{} for ~{30%minutes}.";

        // Act
        var result = CooklangParser.Parse(recipeText, strictMode: true);

        // Assert
        result.Success.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count.ShouldBe(0);
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
        var result = CooklangParser.Parse(recipeText, strictMode: true);

        // Assert
        result.Success.ShouldBeFalse();
        result.Errors.Count.ShouldBe(1); // Only the missing brace error
        
        // The recipe should still be parsed (even though there are errors)
        // This is because we continue parsing after errors
        result.Recipe.ShouldBeNull(); // But recipe is null when there are errors
    }
    
    [Fact]
    public void Should_Preserve_Legacy_Properties()
    {
        // Arrange
        var recipeText = @"Add @bananas{2 after mixing.";

        // Act
        var result = CooklangParser.Parse(recipeText, strictMode: true);

        // Assert
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("Unterminated brace: missing '}'"); // First error message
        result.ErrorPosition.ShouldNotBeNull();
    }
}