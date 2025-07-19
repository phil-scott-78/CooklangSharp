using CooklangSharp.Models;

namespace CooklangSharp.Core;

public static class CooklangParser
{
    public static ParseResult Parse(string source)
    {
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