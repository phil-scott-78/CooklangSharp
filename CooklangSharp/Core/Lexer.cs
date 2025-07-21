using System.Collections.Immutable;
using CooklangSharp.Models;

namespace CooklangSharp.Core;

/// <summary>
/// The Lexer scans the raw source text and breaks it down into a sequence of tokens.
/// This process, also called tokenization, simplifies the work for the Parser.
/// </summary>
public class Lexer(string source)
{
    private readonly ImmutableList<Token>.Builder _tokens = ImmutableList.CreateBuilder<Token>();

    // Normalize line endings to ensure consistent parsing across platforms.

    /// <summary>
    /// Preprocesses the source to handle comments properly while maintaining step continuity.
    /// </summary>
    private static string PreprocessSource(string source)
    {
        // First, remove block comments
        source = RemoveBlockComments(source);
        
        // Only preprocess if there are actual comments or the specific metadata break case
        if (!source.Contains(ParserConstants.CommentMarker))
        {
            return source; // No preprocessing needed
        }
        
        var lines = source.Split(ParserConstants.NewLine);
        var processedLines = ImmutableList.CreateBuilder<string>();
        
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            
            // Skip full comment lines (they'll be handled by tokenizer)
            if (line.Trim().StartsWith(ParserConstants.CommentMarker))
            {
                processedLines.Add(line); // Keep the line as-is for tokenizer to ignore
                continue;
            }
            
            // Handle comments: Strip " --" comments but preserve step structure
            var commentIndex = FindCommentIndex(line);
            if (commentIndex >= 0)
            {
                line = line[..commentIndex];
                
                // If this line has content after comment removal and the next line exists and is not empty,
                // and the next line is not a special construct, merge the next line as continuation
                if (!string.IsNullOrWhiteSpace(line) && i + 1 < lines.Length)
                {
                    var nextLine = lines[i + 1].Trim();
                    // Don't merge if the next line is a comment-only line
                    if (!string.IsNullOrWhiteSpace(nextLine) && 
                        !nextLine.StartsWith(ParserConstants.MetadataMarker) && 
                        !nextLine.StartsWith(ParserConstants.SectionMarker + ParserConstants.SectionMarker) &&
                        !nextLine.StartsWith(ParserConstants.FrontMatterDelimiter) &&
                        !nextLine.StartsWith(ParserConstants.CommentMarker))
                    {
                        line += $"  {nextLine}";
                        i++; // Skip the next line since we merged it
                    }
                }
            }
            
            processedLines.Add(line);
        }
        
        return string.Join(ParserConstants.NewLine, processedLines.ToImmutable());
    }
    
    /// <summary>
    /// Removes block comments from the source.
    /// </summary>
    private static string RemoveBlockComments(string source)
    {
        // Simple regex-based approach to handle block comments
        var lines = source.Split(ParserConstants.NewLine);
        var result = ImmutableList.CreateBuilder<string>();
        var inBlockComment = false;
        
        foreach (var line in lines)
        {
            var processedLine = line;
            var commentStart = -1;

            // Check if a line contains block comment markers
            if (!inBlockComment)
            {
                commentStart = line.IndexOf(ParserConstants.OpenBlockCommentMarker, StringComparison.Ordinal);
                if (commentStart >= 0)
                {
                    inBlockComment = true;
                }
            }
            
            if (inBlockComment)
            {
                var commentEnd = line.IndexOf(ParserConstants.CloseBlockCommentMarker, StringComparison.Ordinal);
                if (commentEnd >= 0)
                {
                    inBlockComment = false;
                    
                    // If a comment started on this line, remove the comment portion
                    if (commentStart >= 0)
                    {
                        var before = line[..commentStart];
                        var after = line[(commentEnd + 2)..];
                        
                        // If there was a space before the comment and text after, preserve a single space
                        if (before.EndsWith(' ') && after.Length > 0 && !after.StartsWith(' '))
                        {
                            processedLine = $"{before.TrimEnd()} {after}";
                        }
                        else
                        {
                            processedLine = before + after;
                        }
                    }
                    else
                    {
                        // Comment started on the previous line, keep only content after -]
                        processedLine = line[(commentEnd + 2)..];
                    }
                }
                else if (commentStart >= 0)
                {
                    // Comment starts but doesn't end on this line
                    processedLine = line[..commentStart];
                }
                else
                {
                    // The entire line is inside comment
                    continue; // Skip this line entirely
                }
            }
            
            // Only add non-empty lines or lines that had content
            if (!string.IsNullOrWhiteSpace(processedLine) || (!inBlockComment && commentStart < 0))
            {
                result.Add(processedLine);
            }
        }
        
        return string.Join(ParserConstants.NewLine, result.ToImmutable());
    }
    
    /// <summary>
    /// Finds the index of a comment marker that is not part of metadata.
    /// </summary>
    private static int FindCommentIndex(string line)
    {
        var searchIndex = line.IndexOf(" " + ParserConstants.CommentMarker, StringComparison.Ordinal);
        while (searchIndex >= 0)
        {
            // Check if this is actually a "--" comment (not part of " ---")
            if (searchIndex + 3 >= line.Length || line[searchIndex + 3] != '-')
            {
                return searchIndex;
            }
            searchIndex = line.IndexOf(" " + ParserConstants.CommentMarker, searchIndex + 1, StringComparison.Ordinal);
        }
        return -1;
    }
    
    /// <summary>
    /// Performs the tokenization of the entire source string.
    /// </summary>
    /// <returns>A tuple containing the generated list of tokens and any diagnostics (errors/warnings) found.</returns>
    public (ImmutableList<Token> tokens, ImmutableList<Diagnostic> diagnostics) Tokenize()
    {
        var lines = PreprocessSource(source.ReplaceLineEndings(ParserConstants.NewLine.ToString()))
            .Split(ParserConstants.NewLine);

        for (var i = 0; i < lines.Length; i++)
        {
            TokenizeLine(lines[i], i + 1);

            // Add a Newline token after each line to preserve the document structure for the parser.
            if (i < lines.Length - 1)
            {
                _tokens.Add(new Token(TokenType.Newline, ParserConstants.NewLine.ToString(), i + 1, lines[i].Length + 1));
            }
        }

        // Add a final EndOfStream token to signal the end of the input.
        _tokens.Add(new Token(TokenType.EndOfStream, string.Empty, lines.Length, lines.LastOrDefault()?.Length + 1 ?? 1));
        return (_tokens.ToImmutable(), ImmutableList<Diagnostic>.Empty);
    }

    /// <summary>
    /// Tokenizes a single line of the source text.
    /// </summary>
    private void TokenizeLine(string line, int lineNumber)
    {
        var trimmedLine = line.Trim();

        // First, check for line-level constructs that consume the entire line.
        // YAML front matter lines are only ignored if they're part of a valid front matter block at the start
        if (trimmedLine.StartsWith(ParserConstants.FrontMatterDelimiter) && lineNumber == 1) 
        {
            return; // Opening front matter delimiter
        }
        
        if (trimmedLine.StartsWith(ParserConstants.CommentMarker) && !trimmedLine.StartsWith(ParserConstants.FrontMatterDelimiter)) return; // Comment-only lines are ignored.
        if (trimmedLine.StartsWith(ParserConstants.SectionMarker))
        {
            _tokens.Add(new Token(TokenType.SectionHeader, line, lineNumber, 1));
            return;
        }

        if (trimmedLine.StartsWith(ParserConstants.MetadataMarker))
        {
            _tokens.Add(new Token(TokenType.Metadata, line, lineNumber, 1));
            return;
        }

        if (trimmedLine.StartsWith(ParserConstants.NoteMarker))
        {
            _tokens.Add(new Token(TokenType.Note, line, lineNumber, 1));
            return;
        }

        // If it's not a line-level construct, it's a step. Process its content.
        // Comments are already stripped in preprocessing.
        var i = 0;
        while (i < line.Length)
        {
            var column = i + 1;
            var c = line[i];

            // Check for single-character special tokens.
            switch (c)
            {
                case ParserConstants.IngredientSymbol:
                    _tokens.Add(new Token(TokenType.At, ParserConstants.IngredientSymbol, lineNumber, column));
                    i++;
                    break;
                case ParserConstants.CookwareSymbol:
                    _tokens.Add(new Token(TokenType.Hash, ParserConstants.CookwareSymbol, lineNumber, column));
                    i++;
                    break;
                case ParserConstants.TimerSymbol:
                    _tokens.Add(new Token(TokenType.Tilde, ParserConstants.TimerSymbol, lineNumber, column));
                    i++;
                    break;
                case ParserConstants.OpenBrace:
                    _tokens.Add(new Token(TokenType.LBrace, ParserConstants.OpenBrace, lineNumber, column));
                    i++;
                    break;
                case ParserConstants.CloseBrace:
                    _tokens.Add(new Token(TokenType.RBrace, ParserConstants.CloseBrace, lineNumber, column));
                    i++;
                    break;
                case ParserConstants.OpenParen:
                    _tokens.Add(new Token(TokenType.LParen, ParserConstants.OpenParen, lineNumber, column));
                    i++;
                    break;
                case ParserConstants.CloseParen:
                    _tokens.Add(new Token(TokenType.RParen, ParserConstants.CloseParen, lineNumber, column));
                    i++;
                    break;
                case ParserConstants.Percent:
                    _tokens.Add(new Token(TokenType.Percent, ParserConstants.Percent, lineNumber, column));
                    i++;
                    break;
                default:
                    // If it's not a special character, it's part of a text block.
                    // Read until the next special character is found.
                    var start = i;
                    while (i < line.Length && !ParserConstants.SpecialCharacters.Contains(line[i]))
                    {
                        i++;
                    }

                    var value = line[start..i];
                    _tokens.Add(new Token(TokenType.Text, value, lineNumber, start + 1));
                    break;
            }
        }
    }
}