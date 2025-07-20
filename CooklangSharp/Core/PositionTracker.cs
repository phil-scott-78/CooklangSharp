namespace CooklangSharp.Core;

internal class PositionTracker
{
    private readonly string[] _lines;
    private int _currentLine;
    private int _currentColumn;

    public PositionTracker(string source)
    {
        _lines = source.ReplaceLineEndings("\n").Split("\n");
        _currentLine = 1;
        _currentColumn = 1;
    }

    public int Line => _currentLine;
    public int Column => _currentColumn;
    
    public string GetLineContent(int lineNumber)
    {
        if (lineNumber < 1 || lineNumber > _lines.Length)
            return string.Empty;
        return _lines[lineNumber - 1];
    }

    public void SetPosition(int lineIndex, int columnIndex)
    {
        _currentLine = lineIndex + 1;
        _currentColumn = columnIndex + 1;
    }
    
    public void SetPosition(int lineIndex, int columnIndex, int characterPosition, string line)
    {
        _currentLine = lineIndex + 1;
        _currentColumn = characterPosition + 1;
    }

    public string GetContext(int contextLength = 20)
    {
        var line = GetLineContent(_currentLine);
        if (string.IsNullOrEmpty(line))
            return string.Empty;

        var start = Math.Max(0, _currentColumn - 1 - contextLength / 2);
        var end = Math.Min(line.Length, start + contextLength);
        start = Math.Max(0, end - contextLength);

        var context = line.Substring(start, end - start);
        var markerPosition = _currentColumn - 1 - start;
        
        if (markerPosition >= 0 && markerPosition < context.Length)
        {
            return context.Insert(markerPosition, "→").Insert(markerPosition + 2, "←");
        }

        return context;
    }
}