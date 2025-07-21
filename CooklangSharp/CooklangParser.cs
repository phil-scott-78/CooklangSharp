using CooklangSharp.Core;
using CooklangSharp.Models;

namespace CooklangSharp;

/// <summary>
/// Provides methods for parsing Cooklang recipe text into structured recipe objects.
/// </summary>
public static class CooklangParser
{
    /// <summary>
    /// Parses Cooklang recipe text and returns a structured recipe object or error diagnostics.
    /// </summary>
    /// <param name="source">The Cooklang recipe text to parse.</param>
    /// <returns>A <see cref="ParseResult"/> containing the parsed recipe or error information.</returns>
    public static ParseResult Parse(string source)
    {
        var parser = new Parser();
        return parser.ParseRecipe(source);
    }
}