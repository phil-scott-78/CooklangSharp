namespace CooklangSharp.Models;

public record ParseError
{
    public string Message { get; init; } = string.Empty;
    public int Line { get; init; }
    public int Column { get; init; }
    public int Length { get; init; } = 1;
    public string? Context { get; init; }
    public ParseErrorType Type { get; init; }
}

public enum ParseErrorType
{
    InvalidIngredientSyntax,
    InvalidCookwareSyntax,
    InvalidTimerSyntax,
    InvalidSectionHeader,
    InvalidMetadata,
    UnterminatedBrace,
    UnterminatedParenthesis,
    InvalidQuantity,
    UnexpectedCharacter,
    Other
}

public record ParseResult
{
    public bool Success { get; init; }
    public Recipe? Recipe { get; init; }
    public ParseError? Error { get; init; }
    
    // Legacy properties for backward compatibility
    public string? ErrorMessage => Error?.Message;
    public int? ErrorPosition => Error != null ? (Error.Line - 1) * 100 + Error.Column : null;

    public static ParseResult CreateSuccess(Recipe recipe) => new()
    {
        Success = true,
        Recipe = recipe
    };

    public static ParseResult CreateError(string message, int line = 1, int column = 1, int length = 1, string? context = null, ParseErrorType type = ParseErrorType.Other) => new()
    {
        Success = false,
        Error = new ParseError
        {
            Message = message,
            Line = line,
            Column = column,
            Length = length,
            Context = context,
            Type = type
        }
    };
    
    public static ParseResult CreateError(ParseError error) => new()
    {
        Success = false,
        Error = error
    };
}