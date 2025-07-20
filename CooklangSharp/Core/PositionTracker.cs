namespace CooklangSharp.Core;

internal class PositionTracker(string source)
{
    private readonly string[] _lines = source.ReplaceLineEndings("\n").Split("\n");

    public int Line { get; private set; } = 1;
    public int Column { get; private set; } = 1;

    private string GetLineContent(int lineNumber)
    {
        if (lineNumber < 1 || lineNumber > _lines.Length)
            return string.Empty;
        return _lines[lineNumber - 1];
    }

    public void SetPosition(int lineIndex, int columnIndex)
    {
        Line = lineIndex + 1;
        Column = columnIndex + 1;
    }
    
    public string GetContext(int contextLength = 20)
    {
        var line = GetLineContent(Line);
        if (string.IsNullOrEmpty(line))
            return string.Empty;

        var start = Math.Max(0, Column - 1 - contextLength / 2);
        var end = Math.Min(line.Length, start + contextLength);
        start = Math.Max(0, end - contextLength);

        var context = line.Substring(start, end - start);
        var markerPosition = Column - 1 - start;
        
        if (markerPosition >= 0 && markerPosition < context.Length)
        {
            return context.Insert(markerPosition, "→").Insert(markerPosition + 2, "←");
        }

        return context;
    }
}