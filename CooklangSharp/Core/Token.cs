namespace CooklangSharp.Core;

/// <summary>
/// Represents a single token extracted by the Lexer. It is the smallest meaningful unit of syntax.
/// </summary>
/// <param name="Type">The category of the token.</param>
/// <param name="Value">The raw string value of the token from the source text.</param>
/// <param name="Line">The line number where the token appears.</param>
/// <param name="Column">The column number where the token begins.</param>
public record Token(TokenType Type, string Value, int Line, int Column)
{
    /// <summary>
    /// Represents a single token extracted by the Lexer. It is the smallest meaningful unit of syntax.
    /// </summary>
    /// <param name="Type">The category of the token.</param>
    /// <param name="Value">The raw char value of the token from the source text.</param>
    /// <param name="Line">The line number where the token appears.</param>
    /// <param name="Column">The column number where the token begins.</param>
    public Token(TokenType Type, char Value, int Line, int Column) : this(Type, Value.ToString(), Line, Column)
    {
        
    }
};