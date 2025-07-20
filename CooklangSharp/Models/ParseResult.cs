using System.Diagnostics.CodeAnalysis;

namespace CooklangSharp.Models;

public enum DiagnosticType
{
    Warning,
    Error
}

public record Diagnostic
{
    public string Message { get; init; } = string.Empty;
    public int Line { get; init; }
    public int Column { get; init; }
    public int Length { get; init; } = 1;
    public string? Context { get; init; }
    public ParseErrorType Type { get; init; }
    public DiagnosticType DiagnosticType { get; init; }
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
    
    [MemberNotNull(nameof(Recipe))] 
    public bool Success { get; private init; }
    
    public Recipe? Recipe { get; private init; }
    public List<Diagnostic> Diagnostics { get; private init; } = [];
    
    
    public static ParseResult CreateSuccess(Recipe recipe, List<Diagnostic>? warnings = null) => new()
    {
        Success = true,
        Recipe = recipe,
        Diagnostics = warnings ?? []
    };

    public static ParseResult CreateError(List<Diagnostic> diagnostics) => new()
    {
        Success = false,
        Diagnostics = diagnostics
    };
}