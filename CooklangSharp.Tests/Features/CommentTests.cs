using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests.Features;

public class CommentTests
{
    [Fact]
    public void ParsesCommentOnlyLine()
    {
        var result = CooklangParser.Parse("-- testing comments");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Sections.First().Content.Count.ShouldBe(0);
    }

    [Fact]
    public void IgnoresCommentsInSteps()
    {
        var source = """
            Mix @flour{200%g} and @water{100%ml}. -- Add slowly
            -- This is a full comment line
            Knead for ~{10%minutes}. -- Until smooth
            """;
        
        var result = CooklangParser.Parse(source);
        result.Success.ShouldBeTrue();
        result.Recipe.Sections[0].Content.Count.ShouldBe(2); // Only 2 steps, comments ignored
        
        var step1 = result.Recipe.Sections[0].Content[0] as StepContent;
        step1.ShouldNotBeNull();
        step1.Step.Number.ShouldBe(1);
        
        var step2 = result.Recipe.Sections[0].Content[1] as StepContent;
        step2.ShouldNotBeNull();
        step2.Step.Number.ShouldBe(2);
    }

    [Fact]
    public void HandlesCommentWithSpecialCharacters()
    {
        var source = """
            -- Comment with special chars: @#$%^&*()
            Mix ingredients.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.Sections[0].Content.Count.ShouldBe(1); // Only the step
    }

    [Fact]
    public void HandlesEmptyComment()
    {
        var source = """
            --
            Mix ingredients.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.Sections[0].Content.Count.ShouldBe(1);
    }

    [Fact]
    public void HandlesCommentWithWhitespace()
    {
        var source = """
            --   Comment with spaces   
            Mix ingredients.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.Sections[0].Content.Count.ShouldBe(1);
    }

    [Fact]
    public void HandlesCommentInSection()
    {
        var source = """
            = Preparation
            
            -- Prepare all ingredients first
            Wash @vegetables{1%bunch}.
            
            = Cooking
            
            -- Cook on medium heat
            Heat @oil{2%tbsp} in #pan{}.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.Sections.Count.ShouldBe(2);
        result.Recipe.Sections[0].Content.Count.ShouldBe(1); // Only step, comment ignored
        result.Recipe.Sections[1].Content.Count.ShouldBe(1); // Only step, comment ignored
    }

    [Fact]
    public void HandlesMultipleCommentsInRow()
    {
        var source = """
            -- First comment
            -- Second comment
            -- Third comment
            Mix ingredients.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.Sections[0].Content.Count.ShouldBe(1); // Only the step
    }

    [Fact]
    public void HandlesCommentWithCooklangSyntax()
    {
        var source = """
            -- Add @flour{200%g} and #bowl{} -- this is commented out
            Actually mix @flour{100%g} in #bowl{}.
            """;
        
        var result = CooklangParser.Parse(source);
        result.Success.ShouldBeTrue();
        result.Recipe.Sections[0].Content.Count.ShouldBe(1);
        
        var step = result.Recipe.Sections[0].Content[0] as StepContent;
        step.ShouldNotBeNull();
        
        // Should only parse the actual step, not the commented one
        var ingredients = step.Step.Items.OfType<IngredientItem>().ToList();
        ingredients.Count.ShouldBe(1);
        ingredients[0].Quantity.ShouldBe(100.0); // From the actual step, not the comment
    }

    [Fact]
    public void HandlesInlineCommentAfterStep()
    {
        var source = """
            Mix @flour{200%g} and @water{100%ml}. -- This comment is at the end
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var step = result.Recipe.Sections[0].Content[0] as StepContent;
        step.ShouldNotBeNull();
        
        // Should parse the ingredients before the comment
        var ingredients = step.Step.Items.OfType<IngredientItem>().ToList();
        ingredients.Count.ShouldBe(2);
        ingredients[0].Name.ShouldBe("flour");
        ingredients[1].Name.ShouldBe("water");
    }
}