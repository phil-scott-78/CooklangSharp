namespace CooklangSharp.Models;

public record ParseResult
{
    public bool Success { get; init; }
    public Recipe? Recipe { get; init; }
    public string? Error { get; init; }
    public int? ErrorPosition { get; init; }

    public static ParseResult CreateSuccess(Recipe recipe) => new()
    {
        Success = true,
        Recipe = recipe
    };

    public static ParseResult CreateError(string error, int? position = null) => new()
    {
        Success = false,
        Error = error,
        ErrorPosition = position
    };
}