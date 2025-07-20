using Shouldly;

namespace CooklangSharp.Tests.Features;

public class FrontMatterTests
{
    [Fact]
    public void StandardParserReturnsFrontMatterAsString()
    {
        var source = @"---
title: Test Recipe
servings: 4
tags:
  - test
  - yaml
---

Cook the @dish.";

        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.FrontMatter.ShouldBe("title: Test Recipe\nservings: 4\ntags:\n  - test\n  - yaml");
    }

    [Fact]
    public void EnhancedParserReturnsFrontMatterAsString()
    {
        var source = @"---
title: Test Recipe
servings: 4
tags:
  - test
  - yaml
---

Cook the @dish.";

        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.FrontMatter.ShouldBe("title: Test Recipe\nservings: 4\ntags:\n  - test\n  - yaml");
    }

    [Fact]
    public void NoFrontMatterReturnsEmptyString()
    {
        var source = "Cook the @dish.";

        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.FrontMatter.ShouldBe(string.Empty);
    }

    [Fact]
    public void EmptyFrontMatterReturnsEmptyString()
    {
        var source = @"---
---

Cook the @dish.";

        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.FrontMatter.ShouldBe(string.Empty);
    }

    [Fact]
    public void ClassicMetadataStillInObsoleteProperty()
    {
        var source = @">> source: Test

Cook the @dish.

>> author: Chef";

        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.FrontMatter.ShouldBe(string.Empty);
        
#pragma warning disable CS0618 // Type or member is obsolete
        result.Recipe.Metadata.Count.ShouldBe(2);
        result.Recipe.Metadata["source"].ShouldBe("Test");
        result.Recipe.Metadata["author"].ShouldBe("Chef");
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public void CanHandleBothFrontMatterAndClassicMetadata()
    {
        var source = @"---
title: Test Recipe
servings: 4
---

>> source: Test

Cook the @dish.

>> author: Chef";

        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.FrontMatter.ShouldBe("title: Test Recipe\nservings: 4");
        
        result.Recipe.Metadata.Count.ShouldBe(2);
        result.Recipe.Metadata["source"].ShouldBe("Test");
        result.Recipe.Metadata["author"].ShouldBe("Chef");
    }
}