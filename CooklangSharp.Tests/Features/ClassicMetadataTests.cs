using CooklangSharp.Models;
using Shouldly;

namespace CooklangSharp.Tests.Features;

public class ClassicMetadataTests
{
    [Fact]
    public void StandardParserHandlesClassicMetadata()
    {
        var source = @">> source: Grandma's cookbook

Cook the @dish.

>> leftovers: Keep refrigerated for 3 days";

        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Metadata.Count.ShouldBe(2);
        result.Recipe.Metadata["source"].ShouldBe("Grandma's cookbook");
        result.Recipe.Metadata["leftovers"].ShouldBe("Keep refrigerated for 3 days");
    }

    [Fact]
    public void EnhancedParserHandlesClassicMetadataWithWarnings()
    {
        var source = @">> source: Grandma's cookbook

Cook the @dish.

>> leftovers: Keep refrigerated for 3 days";

        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Metadata.Count.ShouldBe(2);
        result.Recipe.Metadata["source"].ShouldBe("Grandma's cookbook");
        result.Recipe.Metadata["leftovers"].ShouldBe("Keep refrigerated for 3 days");
        
        // Should have warnings about classic metadata usage
        var warnings = result.Diagnostics.Where(d => d.DiagnosticType == DiagnosticType.Warning).ToList();
        warnings.Count.ShouldBe(2);
        warnings.All(w => w.Message.Contains("Classic metadata format")).ShouldBeTrue();
    }

    [Fact]
    public void MixedYamlAndClassicMetadata()
    {
        var source = @"---
title: My Recipe
servings: 4
---

>> source: Grandma's cookbook

Cook the @dish.

>> leftovers: Keep refrigerated for 3 days";

        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        // YAML front matter is returned as raw string
        result.Recipe.FrontMatter.ShouldContain("title: My Recipe");
        result.Recipe.FrontMatter.ShouldContain("servings: 4");
        
        // Only classic metadata is in the Metadata property
        result.Recipe.Metadata.Count.ShouldBe(2);
        result.Recipe.Metadata["source"].ShouldBe("Grandma's cookbook");
        result.Recipe.Metadata["leftovers"].ShouldBe("Keep refrigerated for 3 days");
        
        // Should have warnings only for classic metadata
        var warnings = result.Diagnostics.Where(d => d.DiagnosticType == DiagnosticType.Warning).ToList();
        warnings.Count.ShouldBe(2);
        warnings.All(w => w.Message.Contains("Classic metadata format")).ShouldBeTrue();
    }

    [Fact]
    public void ClassicMetadataAndYamlAreSeparate()
    {
        var source = @"---
source: Original Source
---

>> source: Grandma's cookbook

Cook the @dish.";

        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        
        // YAML front matter is in FrontMatter property
        result.Recipe.FrontMatter.ShouldContain("source: Original Source");
        
        // Classic metadata is in Metadata property
        result.Recipe.Metadata.Count.ShouldBe(1);
        result.Recipe.Metadata["source"].ShouldBe("Grandma's cookbook");
        
        var warnings = result.Diagnostics.Where(d => d.DiagnosticType == DiagnosticType.Warning).ToList();
        warnings.Count.ShouldBe(1);
    }

    [Fact]
    public void InvalidClassicMetadataIgnored()
    {
        var source = @">> invalid metadata without colon

Cook the @dish.

>> valid: metadata";

        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Metadata.Count.ShouldBe(1);
        result.Recipe.Metadata["valid"].ShouldBe("metadata");
        
        // Should only have warning for the valid metadata
        var warnings = result.Diagnostics.Where(d => d.DiagnosticType == DiagnosticType.Warning).ToList();
        warnings.Count.ShouldBe(1);
    }
}