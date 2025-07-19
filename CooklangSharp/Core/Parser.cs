using System.Text;
using CooklangSharp.Models;

namespace CooklangSharp.Core;

internal class Parser
{
    public Recipe ParseRecipe(string source)
    {
        var recipe = new Recipe();
        
        if (string.IsNullOrEmpty(source))
        {
            return recipe;
        }

        var lines = source.ReplaceLineEndings("\n").Split("\n");
        var lineIndex = 0;

        // Parse metadata block if present (only if file starts with ---)
        if (lines.Length > 0 && lines[0].Trim() == "---")
        {
            var metadata = ParseMetadataBlock(lines, ref lineIndex);
            recipe = recipe with { Metadata = metadata };
        }

        // Parse recipe content with sections
        var sections = ParseSections(lines, lineIndex);
        recipe = recipe with { Sections = sections };

        return recipe;
    }

    private static Dictionary<string, object> ParseMetadataBlock(string[] lines, ref int lineIndex)
    {
        var metadata = new Dictionary<string, object>();
        
        if (lines[lineIndex].Trim() != "---")
            return metadata;
        
        lineIndex++; // Skip opening ---
        
        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex];
            
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
                metadata[key] = value;
            }
            
            lineIndex++;
        }
        
        return metadata;
    }

    private List<Section> ParseSections(string[] lines, int startIndex)
    {
        var sections = new List<Section>();
        var currentSection = new Section { Name = null, Content = new List<SectionContent>() };
        var stepNumber = 1;
        
        for (var i = startIndex; i < lines.Length; i++)
        {
            var line = lines[i];
            
            // Check if this is a section header
            if (IsSectionLine(line))
            {
                // Save current section if it has content
                if (currentSection.Content.Count > 0)
                {
                    sections.Add(currentSection);
                    stepNumber = 1; // Reset step numbering for new section
                }
                
                // Parse section header and create new section
                var sectionName = ParseSectionHeader(line);
                currentSection = new Section { Name = sectionName, Content = new List<SectionContent>() };
                continue;
            }
            
            // Check if this is a note line
            if (IsNoteLine(line))
            {
                var note = ParseNote(line);
                currentSection.Content.Add(new NoteContent { Value = note });
                continue;
            }
            
            // Skip empty lines between steps
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
                
            // Skip comment-only lines
            if (IsCommentLine(line))
                continue;
            
            // Parse this line and any continuation lines as a step
            var step = ParseStepWithContinuations(lines, ref i);
            if (step.Items.Count > 0)
            {
                step = step with { Number = stepNumber++ };
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
        
        return sections;
    }
    
    private static bool IsSectionLine(string line)
    {
        var trimmed = line.Trim();
        return trimmed.StartsWith("=") && (trimmed.Length > 1 || trimmed.Contains("="));
    }
    
    private static string? ParseSectionHeader(string line)
    {
        var trimmed = line.Trim();
        
        // Remove leading = characters
        var start = 0;
        while (start < trimmed.Length && trimmed[start] == '=')
        {
            start++;
        }
        
        // Remove trailing = characters
        var end = trimmed.Length - 1;
        while (end >= 0 && trimmed[end] == '=')
        {
            end--;
        }
        
        // Extract section name
        if (start <= end)
        {
            var name = trimmed[start..(end + 1)].Trim();
            return string.IsNullOrEmpty(name) ? null : name;
        }
        
        return null;
    }
    
    private static bool IsNoteLine(string line)
    {
        return line.TrimStart().StartsWith(">");
    }
    
    private static string ParseNote(string line)
    {
        var trimmed = line.TrimStart();
        if (trimmed.StartsWith(">"))
        {
            // Remove the > and any space immediately after it
            var content = trimmed.Substring(1);
            if (content.Length > 0 && content[0] == ' ')
            {
                content = content.Substring(1);
            }
            return content;
        }
        return line;
    }

    private Step ParseStepWithContinuations(string[] lines, ref int lineIndex)
    {
        var allItems = new List<Item>();
        var currentLine = lines[lineIndex];
        
        // Parse the first line
        var step = ParseStep(currentLine);
        allItems.AddRange(step.Items);
        
        // Check if the next lines are continuations (non-empty, non-comment lines after the current line)
        while (lineIndex + 1 < lines.Length)
        {
            var nextLine = lines[lineIndex + 1];
            
            // If next line is empty, we're done with this step
            if (string.IsNullOrWhiteSpace(nextLine))
                break;
                
            // If next line is comment-only, we're done with this step
            if (IsCommentLine(nextLine))
                break;
            
            lineIndex++; // Move to the next line
            var continuationStep = ParseStep(nextLine);
            
            // Add continuation text with leading whitespace preserved
            if (continuationStep.Items.Count > 0)
            {
                // For continuation lines, we need to prepend the original line's leading whitespace
                var leadingWhitespace = GetLeadingWhitespace(nextLine);
                
                if (continuationStep.Items[0] is TextItem firstTextItem)
                {
                    // If no leading whitespace, add context-appropriate spacing
                    if (string.IsNullOrEmpty(leadingWhitespace))
                    {
                        // Use 2 spaces if the step contains ingredients/cookware/timers, 1 space for plain text
                        var hasComponents = allItems.Any(item => item is not TextItem);
                        leadingWhitespace = hasComponents ? "  " : " ";
                    }
                    
                    // Replace the first text item with one that includes the leading whitespace
                    continuationStep.Items[0] = new TextItem { Value = leadingWhitespace + firstTextItem.Value };
                }
                else if (!string.IsNullOrEmpty(leadingWhitespace))
                {
                    // If the first item is not text, prepend a text item with the whitespace
                    allItems.Add(new TextItem { Value = leadingWhitespace });
                }
                
                allItems.AddRange(continuationStep.Items);
            }
        }
        
        // Merge consecutive text items
        var mergedItems = MergeConsecutiveTextItems(allItems);
        return new Step { Items = mergedItems };
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
                // Non-text item found, commit any accumulated text
                if (currentTextValue.Length > 0)
                {
                    merged.Add(new TextItem { Value = currentTextValue.ToString() });
                    currentTextValue.Clear();
                }
                merged.Add(item);
            }
        }
        
        // Commit any remaining text
        if (currentTextValue.Length > 0)
        {
            merged.Add(new TextItem { Value = currentTextValue.ToString() });
        }
        
        return merged;
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

    private static int FindCommentStart(string line)
    {
        // Look for " --" (space followed by two dashes) to distinguish from "---"
        var index = line.IndexOf(" --", StringComparison.Ordinal);
        if (index >= 0)
        {
            // Verify it's not "---" by checking the character after "--"
            var commentStart = index + 1; // Position of the first "-"
            if (commentStart + 2 < line.Length && line[commentStart + 2] == '-')
            {
                // This is "---", not a comment
                return -1;
            }
            return commentStart;
        }
        
        // Also check for "--" at the beginning of line
        if (!line.TrimStart().StartsWith("--"))
        {
            return -1;
        }
        
        var trimmed = line.TrimStart();
        if (trimmed.Length > 2 && trimmed[2] == '-')
        {
            // This is "---", not a comment
            return -1;
        }
        
        return line.IndexOf("--", StringComparison.Ordinal);

    }

    private static bool IsCommentLine(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("--") && (trimmed.Length == 2 || trimmed[2] == ' ' || char.IsWhiteSpace(trimmed[2]));
    }

    private Step ParseStep(string line)
    {
        var items = new List<Item>();

        // Remove comments from end of line, including any trailing whitespace before the comment
        // Note: comments are exactly "--", not "---" or longer dashes
        var commentIndex = FindCommentStart(line);
        if (commentIndex >= 0)
        {
            // Trim whitespace before the comment
            line = line[..commentIndex].TrimEnd();
        }
        
        if (string.IsNullOrWhiteSpace(line))
        {
            return new Step { Items = items };
        }
        
        var position = 0;
        
        while (position < line.Length)
        {
            var item = ParseNextItem(line, ref position);
            if (item != null)
            {
                items.Add(item);
            }
        }
        
        return new Step { Items = items };
    }

    private Item? ParseNextItem(string line, ref int position)
    {
        if (position >= line.Length)
            return null;
            
        var ch = line[position];
        
        return ch switch
        {
            '@' => ParseIngredient(line, ref position),
            '#' => ParseCookware(line, ref position),
            '~' => ParseTimer(line, ref position),
            _ => ParseText(line, ref position)
        };
    }

    private Item ParseIngredient(string line, ref int position)
    {
        var originalPosition = position;
        position++; // Skip @
        
        // Check for space immediately after @
        if (position < line.Length && char.IsWhiteSpace(line[position]))
        {
            // Invalid: space after @
            position = originalPosition + 1; // Just consume the @
            return new TextItem { Value = "@" };
        }
        
        var (name, quantity, units) = ParseComponent(line, ref position, isTimer: false);
        
        // Check for modifier (preparation instructions) in parentheses
        string? note = null;
        if (position < line.Length && line[position] == '(')
        {
            note = ParseModifier(line, ref position);
        }
        
        return new IngredientItem
        {
            Name = name,
            Quantity = quantity ?? "some",
            Units = units ?? "",
            Note = note
        };
    }
    
    private static string? ParseModifier(string line, ref int position)
    {
        if (position >= line.Length || line[position] != '(')
            return null;
            
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
        
        if (depth == 0)
        {
            var modifier = line[start..position];
            position++; // Skip closing parenthesis
            return modifier;
        }
        
        // Unclosed parenthesis, treat as text
        position = start - 1; // Reset to opening parenthesis
        return null;
    }

    private Item ParseCookware(string line, ref int position)
    {
        var originalPosition = position;
        position++; // Skip #
        
        // Check for space immediately after #
        if (position < line.Length && char.IsWhiteSpace(line[position]))
        {
            // Invalid: space after #
            position = originalPosition + 1; // Just consume the #
            return new TextItem { Value = "#" };
        }
        
        var (name, quantity, units) = ParseComponent(line, ref position, isTimer: false);
        
        return new CookwareItem
        {
            Name = name,
            Quantity = quantity ?? 1,
            Units = units ?? ""
        };
    }

    private Item ParseTimer(string line, ref int position)
    {
        var originalPosition = position;
        position++; // Skip ~
        
        var (name, quantity, units) = ParseComponent(line, ref position, isTimer: true);
        
        // Validate timer: must have either a name or a quantity
        if (string.IsNullOrWhiteSpace(name) && (quantity == null || quantity.ToString() == ""))
        {
            // Invalid timer, treat as text
            position = originalPosition + 1; // Just consume the ~
            return new TextItem { Value = "~" };
        }
        
        return new TimerItem
        {
            Name = name,
            Quantity = quantity ?? "",
            Units = units ?? ""
        };
    }

    private (string name, object? quantity, string? units) ParseComponent(string line, ref int position, bool isTimer)
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
            return isTimer ?
                (name, "", "") 
                : (name, null, null);
        }
        
        // Multi-word component with braces
        if (bracePos > position)
        {
            var nameStr = line[position..bracePos];
            
            // For timers, check if there's leading whitespace before the brace (invalid syntax)
            if (isTimer && nameStr.TrimEnd() != nameStr)
            {
                // Invalid: space before brace in timer
                return ("", null, null); // Validation will catch this
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
            var closeBrace = line.IndexOf('}', position);
            if (closeBrace != -1)
            {
                var content = line[(position + 1)..closeBrace].Trim();
                position = closeBrace + 1;
                
                if (!string.IsNullOrEmpty(content))
                {
                    var (parsedQuantity, parsedUnits) = ParseQuantityAndUnits(content);
                    quantity = parsedQuantity;
                    units = parsedUnits;
                }
            }
        }
        
        return (name, quantity, units);
    }

    private static string ParseSingleWord(string line, ref int position)
    {
        var start = position;
        
        while (position < line.Length)
        {
            var ch = line[position];
            
            // Stop at whitespace, punctuation, or special characters
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
                // Only stop at component markers, not at whitespace or punctuation
                case '@':
                case '#':
                case '~':
                    return -1;
            }
        }
        return -1;
    }

    private (object quantity, string units) ParseQuantityAndUnits(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return ("some", "");
        }
        
        var percentIndex = content.IndexOf('%');
        
        if (percentIndex == -1)
        {
            // No units, just quantity
            var quantity = ParseQuantity(content.Trim());
            return (quantity, "");
        }
        
        var quantityPart = content[..percentIndex].Trim();
        var unitsPart = content[(percentIndex + 1)..].Trim();
        
        var parsedQuantity = ParseQuantity(quantityPart);
        
        return (parsedQuantity, unitsPart);
    }

    private static object ParseQuantity(string quantityStr)
    {
        if (string.IsNullOrWhiteSpace(quantityStr))
        {
            return "some";
        }
        
        quantityStr = quantityStr.Trim();
        
        // Try to parse as a fraction
        if (quantityStr.Contains('/'))
        {
            var parts = quantityStr.Split('/');
            if (parts.Length != 2) return quantityStr;
            
            var numeratorStr = parts[0].Trim();
            var denominatorStr = parts[1].Trim();
                
            // Check for leading zeros which should be treated as text
            if (numeratorStr.Length > 1 && numeratorStr.StartsWith('0') && char.IsDigit(numeratorStr[1]))
            {
                return quantityStr; // Return as text if the numerator has leading zeros
            }
                
            if (double.TryParse(numeratorStr, out var numerator) && 
                double.TryParse(denominatorStr, out var denominator) &&
                denominator != 0)
            {
                return numerator / denominator;
            }
            // If fraction parsing fails, return as text
            return quantityStr;
        }
        
        // Try to parse as a number
        if (double.TryParse(quantityStr, out var number))
        {
            return number;
        }
        
        // Return as text quantity
        return quantityStr;
    }

    private static TextItem ParseText(string line, ref int position)
    {
        var start = position;
        
        // Find the next component marker or end of line
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
        return new TextItem { Value = text };
    }
}