using CooklangSharp.Models;

namespace CooklangSharp.Core;

public static class CooklangParser
{
    public static ParseResult Parse(string source)
    {
        return Parse(source, strictMode: false);
    }
    
    public static ParseResult Parse(string source, bool strictMode)
    {
        if (strictMode)
        {
            var enhancedParser = new EnhancedParser();
            return enhancedParser.ParseRecipe(source);
        }
        else
        {
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
}