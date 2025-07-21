# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CooklangSharp is a .NET project implementing a C# parser for the Cooklang recipe markup language, built for .NET 9.0. It provides a robust parsing solution with comprehensive error handling and diagnostic capabilities.

## Architecture

The project uses a two-phase parsing architecture:

### Core Components
- **CooklangParser.cs**: Main public API with static `Parse` method
- **Core/Lexer.cs**: Tokenizes Cooklang text, handles comment preprocessing
- **Core/Parser.cs**: Builds AST from tokens using the sly parser framework
- **Models/Recipe.cs**: Domain models (Recipe, Section, Step, Ingredient, Cookware, Timer)
- **Models/ParseResult.cs**: Result type with Success/Failure states and diagnostics

### Project Structure
- `CooklangSharp/`: Main library project
- `CooklangSharp.Tests/`: Comprehensive test suite
- `CooklangSharp.Demo/`: Example console application

### Key Design Patterns
- **Immutable records**: All models use C# records
- **Result pattern**: ParseResult encapsulates success/failure with diagnostics
- **Token-based parsing**: Lexer → Parser pipeline
- **Diagnostic system**: Detailed error reporting with line/column positions

## Development Commands

### Build and Test
```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run all tests
dotnet test

# Run tests with verbose output, this allows capturing Console.WriteLine statements
dotnet test --logger "console;verbosity=detailed"

# Run a specific test
dotnet test --filter "FullyQualifiedName~TestName"


### Build Configurations
```bash
# Release build
dotnet build --configuration Release

# Run the demo application
dotnet run --project CooklangSharp.Demo
```

## Testing Strategy

The test suite is organized into categories:
- **Compliance/**: Canonical tests against official Cooklang spec
- **Core/**: Basic parsing, metadata, quantity parsing tests
- **Features/**: Feature-specific tests (comments, sections, notes, modifiers)
- **ErrorHandling/**: Error position and multi-error scenario tests

Test files use xUnit v3 with Shouldly assertions. Many tests read YAML specifications using YamlDotNet.

When troubleshooting, you can use Console.WriteLine and the ToYaml() extension method to output generated
recipes. Ensure to test with verbose output.

IMPORTANT: DO NOT TRY TO CREATE SINGLE FILE SCRIPTS TO TEST OR NEW PROJECTS. ALWAYS TEST WITH UNIT TESTS

## Dependencies

- **xunit.v3**: Testing framework
- **Shouldly** (4.3.0): Assertion library
- **YamlDotNet** (16.3.0): YAML parsing for test data

## Common Tasks

### Adding New Parser Features
1. Update token types in `Core/TokenType.cs` if needed
2. Modify lexer rules in `Core/Lexer.cs` for new syntax
3. Update parser rules in `Core/Parser.cs`
4. Add/update model types in `Models/`
5. Add tests in appropriate test category

### Debugging Parser Issues
- Enable verbose test output to see detailed parsing steps
- Use Console.WriteLine and the ToYaml() extension method to output generated.recipes. Ensure to test with verbose output.
- 
### Running Specific Tests
```bash
# Run tests by category
dotnet test --filter "Category=Canonical" --logger "console;verbosity=detailed"

# Run tests by class
dotnet test --filter "FullyQualifiedName~BasicParsingTests" --logger "console;verbosity=detailed"

# Run individual test
dotnet test --filter "DisplayName~specific_test_name" --logger "console;verbosity=detailed"
```

## API Usage

```csharp
using CooklangSharp;

var result = CooklangParser.Parse(recipeText);
if (result.Success)
{
    var recipe = result.Recipe;
    // Access recipe.Sections, recipe.Metadata, etc.
}
else
{
    foreach (var diagnostic in result.Diagnostics)
    {
        Console.WriteLine($"{diagnostic.Severity} at {diagnostic.Line}:{diagnostic.Column}: {diagnostic.Message}");
    }
}
```