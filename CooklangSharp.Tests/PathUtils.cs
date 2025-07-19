using System.Runtime.CompilerServices;

namespace CooklangSharp.Tests;

internal static class PathUtils
{
    public static string GetPath(string filename)
    {
        return Path.Combine(GetPathViaCallerFilePath(), filename);
    }

    private static string GetPathViaCallerFilePath([CallerFilePath] string? callerPath = null)
    {
        return Path.GetDirectoryName(callerPath) ?? throw new InvalidOperationException();
    }
}