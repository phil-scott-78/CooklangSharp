using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests.Features;

public class BlockCommentTests
{
    [Fact]
    public void BlockComment_Inline_ShouldBeParsedCorrectly()
    {
        // Arrange
        var recipe = "Mash @potato{2%kg} [- TODO change units to litres -]";
        
        // Act
        var result = CooklangParser.Parse(recipe);
        
        // Assert
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        var section = result.Recipe.Sections[0];
        var step = (section.Content[0] as StepContent)?.Step;
        step.ShouldNotBeNull();
        
        // Should have 3 items: "Mash ", ingredient, and " "
        step.Items.Count.ShouldBe(3);
        
        step.Items[0].ShouldBeOfType<TextItem>();
        ((TextItem)step.Items[0]).Value.ShouldBe("Mash ");
        
        step.Items[1].ShouldBeOfType<IngredientItem>();
        var ingredient = (IngredientItem)step.Items[1];
        ingredient.Name.ShouldBe("potato");
        ingredient.Quantity?.GetNumericValue().ShouldBe(2);
        ingredient.Units.ShouldBe("kg");
        
        step.Items[2].ShouldBeOfType<TextItem>();
        ((TextItem)step.Items[2]).Value.ShouldBe(" ");
    }
    
    [Fact]
    public void BlockComment_Multiline_ShouldBeParsedCorrectly()
    {
        // Arrange
        var recipe = @"[- This is a longer comment
   that spans multiple lines
   with various notes -]";
        
        // Act
        var result = CooklangParser.Parse(recipe);
        
        // Assert
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        // Block comment creates an empty default section
        result.Recipe.Sections.Count.ShouldBe(1);
        result.Recipe.Sections[0].Name.ShouldBeNull(); // Default section
        result.Recipe.Sections[0].Content.Count.ShouldBe(0); // No content
    }
    
    [Fact]
    public void MultipleComments_ShouldBeParsedCorrectly()
    {
        // Arrange
        var recipe = @"-- First comment
Stir well [- remember to scrape bottom -] -- another comment";
        
        // Act
        var result = CooklangParser.Parse(recipe);
        
        // Assert
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        var section = result.Recipe.Sections[0];
        var step = (section.Content[0] as StepContent)?.Step;
        step.ShouldNotBeNull();
        
        // Should only have "Stir well " as both line and block comments are ignored
        step.Items.Count.ShouldBe(1);
        step.Items[0].ShouldBeOfType<TextItem>();
        ((TextItem)step.Items[0]).Value.ShouldBe("Stir well ");
    }
}