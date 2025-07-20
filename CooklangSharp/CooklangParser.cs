using CooklangSharp.Core;
using CooklangSharp.Models;

namespace CooklangSharp;

public static class CooklangParser
{
    public static ParseResult Parse(string source, bool strictMode = true)
    {
        if (strictMode)
        {
            var enhancedParser = new EnhancedParser();
            return enhancedParser.ParseRecipe(source);
        }

        // Use original parser for backward compatibility
        try
        {
            var parser = new Parser();
            var recipe = parser.ParseRecipe(source);
            return ParseResult.CreateSuccess(recipe);
        }
        catch (Exception ex)
        {
            return ParseResult.CreateError(ex.Message);
        }
    }
}