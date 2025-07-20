using CooklangSharp.Core;
using CooklangSharp.Models;

namespace CooklangSharp;

public static class CooklangParser
{
    public static ParseResult Parse(string source)
    {
        var parser = new Parser();
        return parser.ParseRecipe(source);
    }
}