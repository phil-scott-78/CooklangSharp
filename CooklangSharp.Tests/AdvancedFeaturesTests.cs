using CooklangSharp.Core;
using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests;

public class AdvancedFeaturesTests
{
    [Fact]
    public void ParsesNotesCorrectly()
    {
        var source = """
            > Don't burn the roux!
            
            Mash @potato{2%kg} until smooth.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Sections.Count.ShouldBe(1);
        
        var section = result.Recipe.Sections[0];
        section.Name.ShouldBeNull(); // Default section
        section.Content.Count.ShouldBe(2);
        
        // First item should be a note
        var noteContent = section.Content[0] as NoteContent;
        noteContent.ShouldNotBeNull();
        noteContent.Type.ShouldBe("text");
        noteContent.Value.ShouldBe("Don't burn the roux!");
        
        // Second item should be a step
        var stepContent = section.Content[1] as StepContent;
        stepContent.ShouldNotBeNull();
        stepContent.Type.ShouldBe("step");
        stepContent.Step.Number.ShouldBe(1);
    }
    
    [Fact]
    public void ParsesSectionsCorrectly()
    {
        var source = """
            = Dough
            
            Mix @flour{200%g} and @water{100%ml}.
            
            == Filling ==
            
            Mix @onion{1} and @garlic{2%cloves}.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Sections.Count.ShouldBe(2);
        
        // First section
        var doughSection = result.Recipe.Sections[0];
        doughSection.Name.ShouldBe("Dough");
        doughSection.Content.Count.ShouldBe(1);
        var doughStep = (doughSection.Content[0] as StepContent)?.Step;
        doughStep.ShouldNotBeNull();
        doughStep.Number.ShouldBe(1);
        
        // Second section
        var fillingSection = result.Recipe.Sections[1];
        fillingSection.Name.ShouldBe("Filling");
        fillingSection.Content.Count.ShouldBe(1);
        var fillingStep = (fillingSection.Content[0] as StepContent)?.Step;
        fillingStep.ShouldNotBeNull();
        fillingStep.Number.ShouldBe(1); // Step numbering resets per section
    }
    
    [Fact]
    public void ParsesIngredientModifiersCorrectly()
    {
        var source = """
            Mix @onion{1}(peeled and finely chopped) and @garlic{2%cloves}(peeled and minced) into paste.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        // Get the first section's first step content
        var section = result.Recipe.Sections[0];
        var stepContent = section.Content[0] as StepContent;
        stepContent.ShouldNotBeNull();
        var step = stepContent.Step;

        var ingredients = step.Items.OfType<IngredientItem>().ToList();
        ingredients.Count.ShouldBe(2);
        
        // First ingredient with modifier
        var onion = ingredients[0];
        onion.Name.ShouldBe("onion");
        onion.Quantity.ShouldBe(1.0);
        onion.Units.ShouldBe("");
        onion.Note.ShouldBe("peeled and finely chopped");
        
        // Second ingredient with modifier
        var garlic = ingredients[1];
        garlic.Name.ShouldBe("garlic");
        garlic.Quantity.ShouldBe(2.0);
        garlic.Units.ShouldBe("cloves");
        garlic.Note.ShouldBe("peeled and minced");
    }
    
    [Fact]
    public void ParsesComplexRecipeWithAllFeatures()
    {
        var source = """
            = Dough
            
            > Make sure to use a good mixer
            
            Mix @flour{200%g} and @water{100%ml} together until smooth.
            
            == Filling ==
            
            Mix @onion{1}(peeled and finely chopped) and @garlic{2%cloves}(peeled and minced) into paste.
            
            Combine @cheese{100%g} and @spinach{50%g}, then season to taste.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Sections.Count.ShouldBe(2);
        
        // Dough section
        var doughSection = result.Recipe.Sections[0];
        doughSection.Name.ShouldBe("Dough");
        doughSection.Content.Count.ShouldBe(2);
        
        // Note in dough section
        var note = doughSection.Content[0] as NoteContent;
        note.ShouldNotBeNull();
        note.Value.ShouldBe("Make sure to use a good mixer");
        
        // Step in dough section
        var doughStepContent = doughSection.Content[1] as StepContent;
        doughStepContent.ShouldNotBeNull();
        doughStepContent.Step.Number.ShouldBe(1);
        
        // Filling section
        var fillingSection = result.Recipe.Sections[1];
        fillingSection.Name.ShouldBe("Filling");
        fillingSection.Content.Count.ShouldBe(2);
        
        // First step in filling with modifiers
        var fillingStep1 = (fillingSection.Content[0] as StepContent)?.Step;
        fillingStep1.ShouldNotBeNull();
        fillingStep1.Number.ShouldBe(1);
        
        var ingredientsWithModifiers = fillingStep1.Items.OfType<IngredientItem>()
            .Where(i => i.Note != null).ToList();
        ingredientsWithModifiers.Count.ShouldBe(2);
        
        // Second step in filling
        var fillingStep2 = (fillingSection.Content[1] as StepContent)?.Step;
        fillingStep2.ShouldNotBeNull();
        fillingStep2.Number.ShouldBe(2);
    }
    
    [Fact]
    public void HandlesEmptySectionNames()
    {
        var source = """
            ===
            Step in unnamed section.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Sections.Count.ShouldBe(1);
        result.Recipe.Sections[0].Name.ShouldBeNull();
    }
    
    [Fact]
    public void HandlesNestedParenthesesInModifiers()
    {
        var source = """
            Add @sauce{100%ml}(homemade (see recipe on page 5)) to the dish.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        var section = result.Recipe.Sections[0];
        var stepContent = section.Content[0] as StepContent;
        stepContent.ShouldNotBeNull();
        var step = stepContent.Step;

        var ingredient = step.Items.OfType<IngredientItem>().First();
        ingredient.ShouldNotBeNull();
        ingredient.Name.ShouldBe("sauce");
        ingredient.Quantity.ShouldBe(100.0);
        ingredient.Units.ShouldBe("ml");
        ingredient.Note.ShouldBe("homemade (see recipe on page 5)");
    }

    [Theory]
    [InlineData("1/2", 0.5)]
    [InlineData("1/4", 0.25)]
    [InlineData("3/4", 0.75)]
    [InlineData("1.5", 1.5)]
    [InlineData("2", 2.0)]
    public void ParsesFractionalIngredientQuantities(string quantityText, double expectedValue)
    {
        var source = $"Add @butter{{{quantityText}%cup}} to the mixture.";

        var result = CooklangParser.Parse(source);

        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();

        var section = result.Recipe.Sections[0];
        var stepContent = section.Content[0] as StepContent;
        stepContent.ShouldNotBeNull();
        var step = stepContent.Step;

        var ingredient = step.Items.OfType<IngredientItem>().First();
        ingredient.ShouldNotBeNull();
        ingredient.Name.ShouldBe("butter");
        ingredient.Quantity.ShouldBe(expectedValue);
        ingredient.Units.ShouldBe("cup");
    }
}