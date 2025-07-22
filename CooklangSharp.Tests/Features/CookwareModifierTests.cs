using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests.Features;

public class CookwareModifierTests
{
    [Fact]
    public void ParsesCookwareWithModifier()
    {
        var source = "Mix in #bowl{}(large, clean).";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var step = result.Recipe.Sections[0].Content[0] as StepContent;
        step.ShouldNotBeNull();
        
        var cookware = step.Step.Items.OfType<CookwareItem>().First();
        cookware.Name.ShouldBe("bowl");
        cookware.Quantity?.GetNumericValue().ShouldBe(1);
        cookware.Note.ShouldBe("large, clean");
    }
    
    [Fact]
    public void ParsesCookwareWithQuantityAndModifier()
    {
        var source = "Use #pan{2}(non-stick).";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var step = result.Recipe.Sections[0].Content[0] as StepContent;
        step.ShouldNotBeNull();
        
        var cookware = step.Step.Items.OfType<CookwareItem>().First();
        cookware.Name.ShouldBe("pan");
        cookware.Quantity?.GetNumericValue().ShouldBe(2.0);
        cookware.Note.ShouldBe("non-stick");
    }
    
    [Fact]
    public void ParsesCookwareWithoutModifier()
    {
        var source = "Mix in #bowl{}.";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var step = result.Recipe.Sections[0].Content[0] as StepContent;
        step.ShouldNotBeNull();
        
        var cookware = step.Step.Items.OfType<CookwareItem>().First();
        cookware.Name.ShouldBe("bowl");
        cookware.Quantity.GetNumericValue().ShouldBe(1);
        cookware.Note.ShouldBeNull();
    }
    
    [Fact]
    public void ParsesMultipleCookwareWithMixedModifiers()
    {
        var source = "Prepare #mixing bowl{}(large) and #whisk{}(clean and dry).";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var step = result.Recipe.Sections[0].Content[0] as StepContent;
        step.ShouldNotBeNull();
        
        var cookware = step.Step.Items.OfType<CookwareItem>().ToList();
        cookware.Count.ShouldBe(2);
        
        cookware[0].Name.ShouldBe("mixing bowl");
        cookware[0].Note.ShouldBe("large");
        
        cookware[1].Name.ShouldBe("whisk");
        cookware[1].Note.ShouldBe("clean and dry");
    }
    
    [Fact]
    public void HandlesEmptyModifier()
    {
        var source = "Use #pan{}().";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var step = result.Recipe.Sections[0].Content[0] as StepContent;
        step.ShouldNotBeNull();
        
        var cookware = step.Step.Items.OfType<CookwareItem>().First();
        cookware.Name.ShouldBe("pan");
        cookware.Note.ShouldBe("");
    }
    
    [Fact]
    public void HandlesNestedParenthesesInModifier()
    {
        var source = "Use #pot{}(large (at least 5L)).";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        var step = result.Recipe.Sections[0].Content[0] as StepContent;
        step.ShouldNotBeNull();
        
        var cookware = step.Step.Items.OfType<CookwareItem>().First();
        cookware.Name.ShouldBe("pot");
        cookware.Note.ShouldBe("large (at least 5L)");
    }
}