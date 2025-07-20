namespace CooklangSharp.Core;

/// <summary>
/// Defines the different types of tokens that can be found in a Cooklang recipe.
/// </summary>
public enum TokenType
{
    // Special Characters that mark the start of a component or syntax
    At, // @
    Hash, // #
    Tilde, // ~
    LBrace, // {
    RBrace, // }
    LParen, // (
    RParen, // )
    Percent, // %

    // Represents any sequence of characters that is not a special character.
    Text,

    // Represents tokens that define the structure of the document on a line-by-line basis.
    SectionHeader, // === Section Name ===
    Metadata, // >> key: value
    Note, // > a note

    // Control tokens for the parser to manage flow.
    Newline,
    EndOfStream
}