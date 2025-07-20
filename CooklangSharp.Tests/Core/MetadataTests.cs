using Shouldly;

namespace CooklangSharp.Tests.Core;

public class MetadataTests
{
    [Fact]
    public void ParsesRecipeWithoutMetadata()
    {
        var source = "Mix @flour{200%g} and @water{100%ml}.";
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
    }
}