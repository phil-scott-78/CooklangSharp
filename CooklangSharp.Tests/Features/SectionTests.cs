using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests.Features;

public class SectionTests
{
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
    public void ParsesSingleEqualsSection()
    {
        var source = """
            = Main Course
            
            Cook @chicken{1%whole} until done.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.Sections.Count.ShouldBe(1);
        result.Recipe.Sections[0].Name.ShouldBe("Main Course");
    }

    [Fact]
    public void ParsesDoubleEqualsSection()
    {
        var source = """
            == Side Dish ==
            
            Prepare @vegetables{500%g}.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.Sections.Count.ShouldBe(1);
        result.Recipe.Sections[0].Name.ShouldBe("Side Dish");
    }

    [Fact]
    public void ParsesTripleEqualsSection()
    {
        var source = """
            === Dessert ===
            
            Make @ice cream{1%cup}.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.Sections.Count.ShouldBe(1);
        result.Recipe.Sections[0].Name.ShouldBe("Dessert");
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
    public void ParsesMultipleSectionsWithSteps()
    {
        var source = """
            = Preparation
            
            Wash @vegetables{1%bunch}.
            
            Chop @onions{2}.
            
            = Cooking
            
            Heat @oil{2%tbsp} in #pan{}.
            
            Add ingredients.
            
            = Serving
            
            Plate and garnish.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.Sections.Count.ShouldBe(3);
        
        // Preparation section
        var prepSection = result.Recipe.Sections[0];
        prepSection.Name.ShouldBe("Preparation");
        prepSection.Content.Count.ShouldBe(2);
        
        // Cooking section
        var cookSection = result.Recipe.Sections[1];
        cookSection.Name.ShouldBe("Cooking");
        cookSection.Content.Count.ShouldBe(2);
        
        // Serving section
        var serveSection = result.Recipe.Sections[2];
        serveSection.Name.ShouldBe("Serving");
        serveSection.Content.Count.ShouldBe(1);
    }

    [Fact]
    public void ParsesSectionWithoutSteps()
    {
        var source = """
            = Empty Section
            
            = Next Section
            
            Do something.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.Sections.Count.ShouldBe(2);
        result.Recipe.Sections[0].Name.ShouldBe("Empty Section");
        result.Recipe.Sections[0].Content.Count.ShouldBe(0);
        result.Recipe.Sections[1].Name.ShouldBe("Next Section");
        result.Recipe.Sections[1].Content.Count.ShouldBe(1);
    }

    [Fact]
    public void ParsesSectionWithSpecialCharacters()
    {
        var source = """
            = Step 1: Pre-heat & Prep
            
            Pre-heat #oven{} to 350°F.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.Sections[0].Name.ShouldBe("Step 1: Pre-heat & Prep");
    }

    [Fact]
    public void ParsesDefaultSectionWithoutHeader()
    {
        // this should be treated as Mix @flour{200%g} and @water{100%ml}. Knead for ~{10%minutes}.
        var source = """
            Mix @flour{200%g} and @water{100%ml}.
            Knead for ~{10%minutes}.
            """;
        
        var result = CooklangParser.Parse(source);
        result.Success.ShouldBeTrue();
        result.Recipe.Sections.Count.ShouldBe(1);
        result.Recipe.Sections[0].Name.ShouldBeNull(); // Default section
        result.Recipe.Sections[0].Content.Count.ShouldBe(1); // One multi-line step
    }

    [Fact]
    public void ParsesSectionHeaderWithWhitespace()
    {
        var source = """
            =   Spaced Section Name   =
            
            Do something.
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.Sections[0].Name.ShouldBe("Spaced Section Name");
    }
}