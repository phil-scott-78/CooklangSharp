namespace CooklangSharp.Core;

/// <summary>
/// Constants used throughout the parsing process to avoid magic strings and improve maintainability.
/// </summary>
internal static class ParserConstants
{
    // Default values
    public const string DefaultQuantity = "";
    
    // Comment and metadata markers
    public const string CommentMarker = "--";
    public const string MetadataMarker = ">>";
    public const string SectionMarker = "=";
    public const string NoteMarker = ">";
    public const string FrontMatterDelimiter = "---";
    public const string OpenBlockCommentMarker = "[-";
    public const string CloseBlockCommentMarker = "-]";
    
    // Special characters for tokenization
    public const string SpecialCharacters = "@#~{}()%";
    
    // Component symbols
    public const char IngredientSymbol = '@';
    public const char CookwareSymbol = '#';
    public const char TimerSymbol = '~';
    
    // Bracket symbols
    public const char OpenBrace = '{';
    public const char CloseBrace = '}';
    public const char OpenParen = '(';
    public const char CloseParen = ')';
    public const char Percent = '%';
    
    // Newline
    public const char NewLine = '\n';
    
    // Token type collections for common parsing scenarios

    public static readonly TokenType[] StepEndTokensWithNewline =
    [
        TokenType.Newline, 
        TokenType.EndOfStream, 
        TokenType.SectionHeader, 
        TokenType.Metadata, 
        TokenType.Note
    ];
    
    public static readonly TokenType[] ComponentStartTokens =
    [
        TokenType.At, 
        TokenType.Hash, 
        TokenType.Tilde
    ];
    
    public static readonly TokenType[] TextStopTokens =
    [
        TokenType.LBrace, 
        TokenType.LParen, 
        TokenType.Newline, 
        TokenType.EndOfStream, 
        TokenType.Percent, 
        TokenType.RBrace
    ];
}