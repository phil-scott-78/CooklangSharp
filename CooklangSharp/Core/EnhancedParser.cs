using System.Text;
using CooklangSharp.Models;

namespace CooklangSharp.Core;

internal class EnhancedParser
{
    private PositionTracker? _positionTracker;
    private List<ParseError> _errors = new();

    public ParseResult ParseRecipe(string source)
    {
        try
        {
            _positionTracker = new PositionTracker(source);
            _errors = new List<ParseError>();
            
            if (string.IsNullOrEmpty(source))
            {
                return ParseResult.CreateSuccess(new Recipe());
            }

            var lines = source.ReplaceLineEndings("\n").Split("\n");
            var lineIndex = 0;
            var recipe = new Recipe();

            // Parse metadata block if present
            if (lines.Length > 0 && lines[0].Trim() == "---")
            {
                var metadataResult = ParseMetadataBlock(lines, ref lineIndex);
                recipe = recipe with { Metadata = metadataResult.Metadata };
            }

            // Parse recipe content with sections
            var sectionsResult = ParseSections(lines, lineIndex);
            recipe = recipe with { Sections = sectionsResult.Sections };

            if (_errors.Count > 0)
            {
                return ParseResult.CreateErrorWithMultiple(_errors);
            }

            return ParseResult.CreateSuccess(recipe);
        }
        catch (Exception ex)
        {
            _errors.Add(new ParseError
            {
                Message = $"Unexpected error: {ex.Message}",
                Line = _positionTracker?.Line ?? 1,
                Column = _positionTracker?.Column ?? 1,
                Length = 1,
                Context = _positionTracker?.GetContext(),
                Type = ParseErrorType.Other
            });
            return ParseResult.CreateErrorWithMultiple(_errors);
        }
    }

    private void AddError(ParseError error)
    {
        _errors.Add(error);
    }

    private MetadataResult ParseMetadataBlock(string[] lines, ref int lineIndex)
    {
        var metadata = new Dictionary<string, object>();
        
        if (lines[lineIndex].Trim() != "---")
            return MetadataResult.CreateSuccess(metadata);
        
        _positionTracker!.SetPosition(lineIndex, 0);
        lineIndex++; // Skip opening ---
        
        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex];
            _positionTracker.SetPosition(lineIndex, 0);
            
            if (line.Trim() == "---")
            {
                lineIndex++; // Skip closing ---
                break;
            }
            
            if (line.Contains(':'))
            {
                var colonIndex = line.IndexOf(':');
                var key = line[..colonIndex].Trim();
                var value = line[(colonIndex + 1)..].Trim();
                
                if (string.IsNullOrWhiteSpace(key))
                {
                    AddError(new ParseError
                    {
                        Message = "Metadata key cannot be empty",
                        Line = lineIndex + 1,
                        Column = 1,
                        Length = colonIndex,
                        Context = line,
                        Type = ParseErrorType.InvalidMetadata
                    });
                }
                else
                {
                    metadata[key] = value;
                }
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                AddError(new ParseError
                {
                    Message = "Invalid metadata format. Expected 'key: value'",
                    Line = lineIndex + 1,
                    Column = 1,
                    Length = line.Length,
                    Context = line,
                    Type = ParseErrorType.InvalidMetadata
                });
            }
            
            lineIndex++;
        }
        
        return MetadataResult.CreateSuccess(metadata);
    }

    private SectionsResult ParseSections(string[] lines, int startIndex)
    {
        var sections = new List<Section>();
        var currentSection = new Section { Name = null, Content = new List<SectionContent>() };
        var stepNumber = 1;
        
        for (var i = startIndex; i < lines.Length; i++)
        {
            var line = lines[i];
            _positionTracker!.SetPosition(i, 0);
            
            // Check if this is a section header
            if (IsSectionLine(line))
            {
                var sectionResult = ValidateAndParseSectionHeader(line, i + 1);
                
                // Save current section if it has content
                if (currentSection.Content.Count > 0)
                {
                    sections.Add(currentSection);
                    stepNumber = 1; // Reset step numbering for new section
                }
                
                currentSection = new Section { Name = sectionResult.SectionName, Content = new List<SectionContent>() };
                continue;
            }
            
            // Check if this is a note line
            if (IsNoteLine(line))
            {
                var note = ParseNote(line);
                currentSection.Content.Add(new NoteContent { Value = note });
                continue;
            }
            
            // Skip empty lines and comment-only lines
            if (string.IsNullOrWhiteSpace(line) || IsCommentLine(line))
                continue;
            
            // Parse this line and any continuation lines as a step
            var stepResult = ParseStepWithContinuations(lines, ref i);
                
            if (stepResult.Step!.Items.Count > 0)
            {
                var step = stepResult.Step with { Number = stepNumber++ };
                currentSection.Content.Add(new StepContent { Step = step });
            }
        }
        
        // Add final section if it has content
        if (currentSection.Content.Count > 0)
        {
            sections.Add(currentSection);
        }
        
        // If no sections were created, create a default section with no name
        if (sections.Count == 0)
        {
            sections.Add(new Section { Name = null, Content = new List<SectionContent>() });
        }
        
        return SectionsResult.CreateSuccess(sections);
    }

    private SectionHeaderResult ValidateAndParseSectionHeader(string line, int lineNumber)
    {
        var trimmed = line.Trim();
        
        if (!trimmed.StartsWith("="))
        {
            AddError(new ParseError
            {
                Message = "Section header must start with '='",
                Line = lineNumber,
                Column = 1,
                Length = 1,
                Context = line,
                Type = ParseErrorType.InvalidSectionHeader
            });
            return SectionHeaderResult.CreateSuccess(null);
        }
        
        // Count leading equals
        var leadingEquals = 0;
        while (leadingEquals < trimmed.Length && trimmed[leadingEquals] == '=')
        {
            leadingEquals++;
        }
        
        // Count trailing equals
        var trailingEquals = 0;
        var endIndex = trimmed.Length - 1;
        while (endIndex >= 0 && trimmed[endIndex] == '=')
        {
            trailingEquals++;
            endIndex--;
        }
        
        // Extract section name
        var startIndex = leadingEquals;
        var nameLength = trimmed.Length - leadingEquals - trailingEquals;
        
        string? sectionName = null;
        if (nameLength > 0)
        {
            sectionName = trimmed.Substring(startIndex, nameLength).Trim();
            if (string.IsNullOrEmpty(sectionName))
                sectionName = null;
        }
        
        return SectionHeaderResult.CreateSuccess(sectionName);
    }

    private StepResult ParseStepWithContinuations(string[] lines, ref int lineIndex)
    {
        var allItems = new List<Item>();
        var currentLine = lines[lineIndex];
        
        // Parse the first line
        var stepResult = ParseStep(currentLine, lineIndex + 1);
        allItems.AddRange(stepResult.Step!.Items);
        
        // Check if the next lines are continuations
        while (lineIndex + 1 < lines.Length)
        {
            var nextLine = lines[lineIndex + 1];
            
            // If next line is empty, comment-only, or starts a new section/note, we're done
            if (string.IsNullOrWhiteSpace(nextLine) || IsCommentLine(nextLine) || 
                IsSectionLine(nextLine) || IsNoteLine(nextLine))
                break;
            
            lineIndex++; // Move to the next line
            var continuationResult = ParseStep(nextLine, lineIndex + 1);
            
            // Add continuation with appropriate spacing
            if (continuationResult.Step!.Items.Count > 0)
            {
                var leadingWhitespace = GetLeadingWhitespace(nextLine);
                
                if (continuationResult.Step.Items[0] is TextItem firstTextItem)
                {
                    if (string.IsNullOrEmpty(leadingWhitespace))
                    {
                        var hasComponents = allItems.Any(item => item is not TextItem);
                        leadingWhitespace = hasComponents ? "  " : " ";
                    }
                    
                    continuationResult.Step.Items[0] = new TextItem { Value = leadingWhitespace + firstTextItem.Value };
                }
                else if (!string.IsNullOrEmpty(leadingWhitespace))
                {
                    allItems.Add(new TextItem { Value = leadingWhitespace });
                }
                
                allItems.AddRange(continuationResult.Step.Items);
            }
        }
        
        // Merge consecutive text items
        var mergedItems = MergeConsecutiveTextItems(allItems);
        return StepResult.CreateSuccess(new Step { Items = mergedItems });
    }

    private StepResult ParseStep(string line, int lineNumber)
    {
        var items = new List<Item>();

        // Remove comments from end of line
        var commentIndex = FindCommentStart(line);
        if (commentIndex >= 0)
        {
            line = line[..commentIndex].TrimEnd();
        }
        
        if (string.IsNullOrWhiteSpace(line))
        {
            return StepResult.CreateSuccess(new Step { Items = items });
        }
        
        var position = 0;
        
        while (position < line.Length)
        {
            var itemResult = ParseNextItem(line, ref position, lineNumber);
            if (itemResult.Item != null)
            {
                items.Add(itemResult.Item);
            }
        }
        
        return StepResult.CreateSuccess(new Step { Items = items });
    }

    private ItemResult ParseNextItem(string line, ref int position, int lineNumber)
    {
        if (position >= line.Length)
            return ItemResult.CreateSuccess(null);
            
        var ch = line[position];
        
        return ch switch
        {
            '@' => ParseIngredient(line, ref position, lineNumber),
            '#' => ParseCookware(line, ref position, lineNumber),
            '~' => ParseTimer(line, ref position, lineNumber),
            _ => ParseText(line, ref position)
        };
    }

    private ItemResult ParseIngredient(string line, ref int position, int lineNumber)
    {
        position++; // Skip @
        
        // Check for space immediately after @
        if (position < line.Length && char.IsWhiteSpace(line[position]))
        {
            AddError(new ParseError
            {
                Message = "Invalid ingredient syntax: space not allowed after '@'",
                Line = lineNumber,
                Column = position + 1,
                Length = 1,
                Context = line,
                Type = ParseErrorType.InvalidIngredientSyntax
            });
            // Continue parsing anyway to find more errors
            while (position < line.Length && char.IsWhiteSpace(line[position]))
                position++;
        }
        
        var componentResult = ParseComponent(line, ref position, lineNumber, isTimer: false);
        var (name, quantity, units) = componentResult.Component;
        
        // Check for modifier (preparation instructions) in parentheses
        string? note = null;
        if (position < line.Length && line[position] == '(')
        {
            var modifierResult = ParseModifier(line, ref position, lineNumber);
            note = modifierResult.Modifier;
        }
        
        return ItemResult.CreateSuccess(new IngredientItem
        {
            Name = name,
            Quantity = quantity ?? "some",
            Units = units ?? "",
            Note = note
        });
    }

    private ItemResult ParseCookware(string line, ref int position, int lineNumber)
    {
        position++; // Skip #
        
        // Check for space immediately after #
        if (position < line.Length && char.IsWhiteSpace(line[position]))
        {
            AddError(new ParseError
            {
                Message = "Invalid cookware syntax: space not allowed after '#'",
                Line = lineNumber,
                Column = position + 1,
                Length = 1,
                Context = line,
                Type = ParseErrorType.InvalidCookwareSyntax
            });
            // Continue parsing anyway
            while (position < line.Length && char.IsWhiteSpace(line[position]))
                position++;
        }
        
        var componentResult = ParseComponent(line, ref position, lineNumber, isTimer: false);
        var (name, quantity, units) = componentResult.Component;
        
        return ItemResult.CreateSuccess(new CookwareItem
        {
            Name = name,
            Quantity = quantity ?? 1,
            Units = units ?? ""
        });
    }

    private ItemResult ParseTimer(string line, ref int position, int lineNumber)
    {
        position++; // Skip ~
        
        var componentResult = ParseComponent(line, ref position, lineNumber, isTimer: true);
        var (name, quantity, units) = componentResult.Component;
        
        // Validate timer: must have either a name or a quantity
        if (string.IsNullOrWhiteSpace(name) && (quantity == null || quantity.ToString() == ""))
        {
            AddError(new ParseError
            {
                Message = "Invalid timer syntax: timer must have either a name or duration",
                Line = lineNumber,
                Column = position,
                Length = 1,
                Context = line,
                Type = ParseErrorType.InvalidTimerSyntax
            });
        }
        
        return ItemResult.CreateSuccess(new TimerItem
        {
            Name = name,
            Quantity = quantity ?? "",
            Units = units ?? ""
        });
    }

    private ComponentResult ParseComponent(string line, ref int position, int lineNumber, bool isTimer)
    {
        string name;
        object? quantity = null;
        string? units = null;
        
        // Look ahead for the opening brace
        var bracePos = FindNextBrace(line, position);
        
        if (bracePos == -1)
        {
            // Single word component
            name = ParseSingleWord(line, ref position);
            return ComponentResult.CreateSuccess(isTimer ? (name, "", "") : (name, null, null));
        }
        
        // Multi-word component with braces
        if (bracePos > position)
        {
            var nameStr = line[position..bracePos];
            
            // For timers, check if there's leading whitespace before the brace (invalid syntax)
            if (isTimer && nameStr.TrimEnd() != nameStr)
            {
                AddError(new ParseError
                {
                    Message = "Invalid timer syntax: space not allowed before '{'",
                    Line = lineNumber,
                    Column = position + nameStr.TrimEnd().Length + 1,
                    Length = 1,
                    Context = line,
                    Type = ParseErrorType.InvalidTimerSyntax
                });
            }
            
            name = nameStr.Trim();
            position = bracePos;
        }
        else
        {
            name = "";
        }
        
        // Parse quantity and units from braces
        if (position < line.Length && line[position] == '{')
        {
            var openBracePosition = position; // Save the position of the opening brace
            var closeBrace = line.IndexOf('}', position);
            if (closeBrace == -1)
            {
                AddError(new ParseError
                {
                    Message = "Unterminated brace: missing '}'",
                    Line = lineNumber,
                    Column = position + 1,
                    Length = line.Length - position,
                    Context = line,
                    Type = ParseErrorType.UnterminatedBrace
                });
                // Continue parsing until end of line
                position = line.Length;
                return ComponentResult.CreateSuccess((name, quantity, units));
            }
            
            var content = line[(position + 1)..closeBrace].Trim();
            position = closeBrace + 1;
            
            if (!string.IsNullOrEmpty(content))
            {
                var quantityResult = ParseQuantityAndUnits(content, lineNumber, line, openBracePosition);
                quantity = quantityResult.Quantity;
                units = quantityResult.Units;
            }
        }
        
        return ComponentResult.CreateSuccess((name, quantity, units));
    }

    private ModifierResult ParseModifier(string line, ref int position, int lineNumber)
    {
        if (position >= line.Length || line[position] != '(')
            return ModifierResult.CreateSuccess(null);
            
        position++; // Skip opening parenthesis
        var start = position;
        var depth = 1;
        
        while (position < line.Length && depth > 0)
        {
            if (line[position] == '(')
                depth++;
            else if (line[position] == ')')
                depth--;
                
            if (depth > 0)
                position++;
        }
        
        if (depth > 0)
        {
            AddError(new ParseError
            {
                Message = "Unterminated parenthesis: missing ')'",
                Line = lineNumber,
                Column = start,
                Length = line.Length - start + 1,
                Context = line,
                Type = ParseErrorType.UnterminatedParenthesis
            });
            // Continue to end of line
            var modifier = line[start..];
            position = line.Length;
            return ModifierResult.CreateSuccess(modifier);
        }
        
        var modifierText = line[start..position];
        position++; // Skip closing parenthesis
        return ModifierResult.CreateSuccess(modifierText);
    }

    private QuantityResult ParseQuantityAndUnits(string content, int lineNumber, string line, int bracePosition)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return QuantityResult.CreateSuccess("some", "");
        }
        
        var percentIndex = content.IndexOf('%');
        
        if (percentIndex == -1)
        {
            // No units, just quantity
            var quantityResult = ParseQuantity(content.Trim(), lineNumber, line, bracePosition);
            return QuantityResult.CreateSuccess(quantityResult.Quantity!, "");
        }
        
        var quantityPart = content[..percentIndex].Trim();
        var unitsPart = content[(percentIndex + 1)..].Trim();
        
        var parsedQuantityResult = ParseQuantity(quantityPart, lineNumber, line, bracePosition);
        
        return QuantityResult.CreateSuccess(parsedQuantityResult.Quantity!, unitsPart);
    }

    private ParseQuantityResult ParseQuantity(string quantityStr, int lineNumber, string line, int bracePosition)
    {
        if (string.IsNullOrWhiteSpace(quantityStr))
        {
            return ParseQuantityResult.CreateSuccess("some");
        }
        
        quantityStr = quantityStr.Trim();
        
        // Try to parse as a fraction
        if (quantityStr.Contains('/'))
        {
            var parts = quantityStr.Split('/');
            if (parts.Length != 2)
            {
                AddError(new ParseError
                {
                    Message = $"Invalid fraction format: '{quantityStr}'",
                    Line = lineNumber,
                    Column = bracePosition + 2, // +1 for '{' and +1 for 1-based indexing
                    Length = quantityStr.Length,
                    Context = line,
                    Type = ParseErrorType.InvalidQuantity
                });
                return ParseQuantityResult.CreateSuccess(quantityStr);
            }
            
            var numeratorStr = parts[0].Trim();
            var denominatorStr = parts[1].Trim();
                
            // Check for leading zeros which should be treated as text
            if (numeratorStr.Length > 1 && numeratorStr.StartsWith('0') && char.IsDigit(numeratorStr[1]))
            {
                return ParseQuantityResult.CreateSuccess(quantityStr); // Return as text if the numerator has leading zeros
            }

            if (!double.TryParse(numeratorStr, out var numerator) || !double.TryParse(denominatorStr, out var denominator))
            {
                // If fraction parsing fails, return as text
                return ParseQuantityResult.CreateSuccess(quantityStr);
            }
            
            if (denominator != 0) return ParseQuantityResult.CreateSuccess(numerator / denominator);
            // Calculate the position of the '0' in the line
            // bracePosition is 0-based position of '{', 
            // +1 for '{', +length of numerator, +1 for '/', +1 for 1-based indexing
            var zeroPosition = bracePosition + 2;
                    
            AddError(new ParseError
            {
                Message = "Division by zero in fraction",
                Line = lineNumber,
                Column = zeroPosition,
                Length = numeratorStr.Length + 1 + denominatorStr.Length,
                Context = line,
                Type = ParseErrorType.InvalidQuantity
            });
            
            return ParseQuantityResult.CreateSuccess(quantityStr);

        }

        return double.TryParse(quantityStr, out var number) 
            ? ParseQuantityResult.CreateSuccess(number) 
            : ParseQuantityResult.CreateSuccess(quantityStr);
    }

    // Helper methods
    private static bool IsSectionLine(string line)
    {
        var trimmed = line.Trim();
        return trimmed.StartsWith('=') && (trimmed.Length > 1 || trimmed.Contains('='));
    }
    
    private static bool IsNoteLine(string line)
    {
        return line.TrimStart().StartsWith('>');
    }
    
    private static string ParseNote(string line)
    {
        var trimmed = line.TrimStart();
        if (!trimmed.StartsWith('>'))
        {
            return line;
        }
        
        var content = trimmed[1..];
        if (content.Length > 0 && content[0] == ' ')
        {
            content = content[1..];
        }
        return content;
    }
    
    private static bool IsCommentLine(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("--") && (trimmed.Length == 2 || trimmed[2] == ' ' || char.IsWhiteSpace(trimmed[2]));
    }
    
    private static int FindCommentStart(string line)
    {
        var index = line.IndexOf(" --", StringComparison.Ordinal);
        if (index >= 0)
        {
            var commentStart = index + 1;
            if (commentStart + 2 < line.Length && line[commentStart + 2] == '-')
            {
                return -1;
            }
            return commentStart;
        }
        
        if (!line.TrimStart().StartsWith("--"))
        {
            return -1;
        }
        
        var trimmed = line.TrimStart();
        if (trimmed.Length > 2 && trimmed[2] == '-')
        {
            return -1;
        }
        
        return line.IndexOf("--", StringComparison.Ordinal);
    }
    
    private static string GetLeadingWhitespace(string line)
    {
        var i = 0;
        while (i < line.Length && char.IsWhiteSpace(line[i]))
        {
            i++;
        }
        return line[..i];
    }
    
    private static string ParseSingleWord(string line, ref int position)
    {
        var start = position;
        
        while (position < line.Length)
        {
            var ch = line[position];
            
            if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || ch == '@' || ch == '#' || ch == '~')
            {
                break;
            }
            
            position++;
        }
        
        return line[start..position];
    }
    
    private static int FindNextBrace(string line, int startPos)
    {
        for (var i = startPos; i < line.Length; i++)
        {
            switch (line[i])
            {
                case '{':
                    return i;
                case '@':
                case '#':
                case '~':
                    return -1;
            }
        }
        return -1;
    }
    
    private static List<Item> MergeConsecutiveTextItems(List<Item> items)
    {
        if (items.Count <= 1)
            return items;
            
        var merged = new List<Item>();
        var currentTextValue = new StringBuilder();
        
        foreach (var item in items)
        {
            if (item is TextItem textItem)
            {
                currentTextValue.Append(textItem.Value);
            }
            else
            {
                if (currentTextValue.Length > 0)
                {
                    merged.Add(new TextItem { Value = currentTextValue.ToString() });
                    currentTextValue.Clear();
                }
                merged.Add(item);
            }
        }
        
        if (currentTextValue.Length > 0)
        {
            merged.Add(new TextItem { Value = currentTextValue.ToString() });
        }
        
        return merged;
    }
    
    private static ItemResult ParseText(string line, ref int position)
    {
        var start = position;
        
        while (position < line.Length)
        {
            var ch = line[position];
            if (ch is '@' or '#' or '~')
            {
                break;
            }
            position++;
        }
        
        var text = line[start..position];
        return ItemResult.CreateSuccess(new TextItem { Value = text });
    }
}

// Result classes for internal parser operations
internal record MetadataResult(bool Success, Dictionary<string, object> Metadata)
{
    public static MetadataResult CreateSuccess(Dictionary<string, object> metadata) => new(true, metadata);
}

internal record SectionsResult(bool Success, List<Section> Sections)
{
    public static SectionsResult CreateSuccess(List<Section> sections) => new(true, sections);
}

internal record SectionHeaderResult(bool Success, string? SectionName)
{
    public static SectionHeaderResult CreateSuccess(string? sectionName) => new(true, sectionName);
}

internal record StepResult(bool Success, Step? Step)
{
    public static StepResult CreateSuccess(Step step) => new(true, step);
}

internal record ItemResult(bool Success, Item? Item)
{
    public static ItemResult CreateSuccess(Item? item) => new(true, item);
}

internal record ComponentResult(bool Success, (string name, object? quantity, string? units) Component)
{
    public static ComponentResult CreateSuccess((string, object?, string?) component) => new(true, component);
}

internal record ModifierResult(bool Success, string? Modifier)
{
    public static ModifierResult CreateSuccess(string? modifier) => new(true, modifier);
}

internal record QuantityResult(bool Success, object? Quantity, string? Units)
{
    public static QuantityResult CreateSuccess(object quantity, string units) => new(true, quantity, units);
}

internal record ParseQuantityResult(bool Success, object? Quantity)
{
    public static ParseQuantityResult CreateSuccess(object quantity) => new(true, quantity);
}