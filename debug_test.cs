using CooklangSharp.Core;
using CooklangSharp.Models;

class DebugTest
{
    static void Main()
    {
        // Test case 1: Complex line position
        var source1 = "Mix @flour{200%g}, @water{100%ml}, and @ invalid ingredient here.";
        var result1 = CooklangParser.Parse(source1, strictMode: true);
        
        Console.WriteLine("=== Test Case 1: Complex Line ===");
        Console.WriteLine($"Source: {source1}");
        Console.WriteLine($"Success: {result1.Success}");
        
        if (!result1.Success)
        {
            var errors = result1.Diagnostics.Where(d => d.DiagnosticType == DiagnosticType.Error);
            foreach (var error in errors)
            {
                Console.WriteLine($"Error at Line {error.Line}, Column {error.Column}: {error.Message}");
                Console.WriteLine($"Type: {error.Type}");
                Console.WriteLine($"Context: {error.Context}");
            }
        }
        
        Console.WriteLine();
        
        // Test case 2: Invalid syntax with space
        var source2 = """
            This is line 1.
            This is line 2 with @invalid {syntax}.
            This is line 3.
            """;
        var result2 = CooklangParser.Parse(source2, strictMode: true);
        
        Console.WriteLine("=== Test Case 2: Invalid Syntax ===");
        Console.WriteLine($"Source: {source2}");
        Console.WriteLine($"Success: {result2.Success}");
        
        if (!result2.Success)
        {
            var errors = result2.Diagnostics.Where(d => d.DiagnosticType == DiagnosticType.Error);
            foreach (var error in errors)
            {
                Console.WriteLine($"Error at Line {error.Line}, Column {error.Column}: {error.Message}");
                Console.WriteLine($"Type: {error.Type}");
                Console.WriteLine($"Context: {error.Context}");
            }
        }
    }
}