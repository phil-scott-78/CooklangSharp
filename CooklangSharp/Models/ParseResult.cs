using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace CooklangSharp.Models;

/// <summary>
/// Specifies the severity level of a parsing diagnostic.
/// </summary>
public enum DiagnosticType
{
    /// <summary>
    /// A warning that doesn't prevent successful parsing.
    /// </summary>
    Warning,
    
    /// <summary>
    /// An error that prevents successful parsing.
    /// </summary>
    Error
}

/// <summary>
/// Represents a parsing diagnostic with location and error information.
/// </summary>
public record Diagnostic
{
    /// <summary>
    /// Gets the diagnostic message describing the issue.
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the line number where the issue occurred (1-based).
    /// </summary>
    public int Line { get; init; }
    
    /// <summary>
    /// Gets the column position where the issue occurred (1-based).
    /// </summary>
    public int Column { get; init; }
    
    /// <summary>
    /// Gets the length of the problematic text.
    /// </summary>
    public int Length { get; init; } = 1;
    
    /// <summary>
    /// Gets optional context information about the error.
    /// </summary>
    public string? Context { get; init; }
    
    /// <summary>
    /// Gets the specific type of parse error.
    /// </summary>
    public ParseErrorType Type { get; init; }
    
    /// <summary>
    /// Gets the severity of the diagnostic.
    /// </summary>
    public DiagnosticType DiagnosticType { get; init; }
    
    /// <summary>
    /// Gets the severity as a string for display purposes.
    /// </summary>
    public string Severity => DiagnosticType.ToString();
    
    /// <summary>
    /// Gets an optional error code for the diagnostic.
    /// </summary>
    public string? Code => Type != ParseErrorType.Other ? $"CL{(int)Type:D4}" : null;
}

/// <summary>
/// Specifies the type of parsing error encountered.
/// </summary>
public enum ParseErrorType
{
    /// <summary>
    /// Invalid ingredient syntax (e.g., malformed @ingredient).
    /// </summary>
    InvalidIngredientSyntax,
    
    /// <summary>
    /// Invalid cookware syntax (e.g., malformed #cookware).
    /// </summary>
    InvalidCookwareSyntax,
    
    /// <summary>
    /// Invalid timer syntax (e.g., malformed ~timer).
    /// </summary>
    InvalidTimerSyntax,
    
    /// <summary>
    /// Invalid section header format.
    /// </summary>
    InvalidSectionHeader,
    
    /// <summary>
    /// Invalid metadata format.
    /// </summary>
    InvalidMetadata,
    
    /// <summary>
    /// Missing closing brace }.
    /// </summary>
    UnterminatedBrace,
    
    /// <summary>
    /// Missing closing parenthesis ).
    /// </summary>
    UnterminatedParenthesis,
    
    /// <summary>
    /// Invalid quantity format in amount specification.
    /// </summary>
    InvalidQuantity,
    
    /// <summary>
    /// Unexpected character in the input.
    /// </summary>
    UnexpectedCharacter,
    
    /// <summary>
    /// Other parsing errors not covered by specific types.
    /// </summary>
    Other
}

/// <summary>
/// Represents the result of parsing a Cooklang recipe.
/// </summary>
public record ParseResult
{
    /// <summary>
    /// Gets a value indicating whether the parsing was successful.
    /// </summary>
    [MemberNotNull(nameof(Recipe))] 
    public bool Success { get; private init; }
    
    /// <summary>
    /// Gets the parsed recipe if successful, otherwise null.
    /// </summary>
    public Recipe? Recipe { get; private init; }
    
    /// <summary>
    /// Gets the list of diagnostics (errors and warnings) encountered during parsing.
    /// </summary>
    public ImmutableList<Diagnostic> Diagnostics { get; private init; } = ImmutableList<Diagnostic>.Empty;
    
    /// <summary>
    /// Creates a successful parse result with the given recipe.
    /// </summary>
    /// <param name="recipe">The successfully parsed recipe.</param>
    /// <param name="warnings">Optional list of warnings encountered during parsing.</param>
    /// <returns>A successful parse result.</returns>
    public static ParseResult CreateSuccess(Recipe recipe, List<Diagnostic>? warnings = null) => new()
    {
        Success = true,
        Recipe = recipe,
        Diagnostics = warnings?.ToImmutableList() ?? ImmutableList<Diagnostic>.Empty
    };

    /// <summary>
    /// Creates a failed parse result with the given diagnostics.
    /// </summary>
    /// <param name="diagnostics">The list of errors that prevented successful parsing.</param>
    /// <returns>A failed parse result.</returns>
    public static ParseResult CreateError(List<Diagnostic> diagnostics) => new()
    {
        Success = false,
        Diagnostics = diagnostics.ToImmutableList()
    };
}