# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CooklangSharp is a C# parser for the Cooklang recipe markup language (https://cooklang.org/). The project implements a parser that converts Cooklang-formatted text into structured recipe objects.

## Build Commands

```bash
# Clean build artifacts
dotnet clean

# Build the solution
dotnet build

# Run tests
dotnet test

# Run a specific test
dotnet test --filter "FullyQualifiedName~CanonicalTests.BasicTextParsing"

# Run tests with specific verbosity
dotnet test --logger:"console;verbosity=detailed"
```

## Architecture

### Core Components

- **CooklangParser** (`CooklangSharp/Core/CooklangParser.cs`): Public API entry point. Static class with `Parse` method that returns `ParseResult`.

- **Parser** (`CooklangSharp/Core/Parser.cs`): Internal parsing implementation. Handles:
  - Metadata parsing (YAML front matter)
  - Step parsing with support for multi-line steps
  - Component parsing (@ingredients, #cookware, ~timers)
  - Comment handling (-- comments)
  - Text parsing

### Domain Models (`CooklangSharp/Models/`)

- **Recipe**: Root object containing Steps and Metadata
- **Step**: Contains list of Items representing a cooking instruction
- **Item** (abstract): Base type for all step components
  - **TextItem**: Plain text
  - **IngredientItem**: Name, Quantity, Units
  - **CookwareItem**: Name, Quantity, Units
  - **TimerItem**: Name, Quantity, Units
- **ParseResult**: Success/Error result type

### Testing

- **CanonicalTests.cs**: Tests against the official Cooklang specification using test cases from `cooklang-spec/tests/canonical.yaml`
- Uses xUnit, Shouldly assertions, and YamlDotNet for parsing test data

### Project Structure

```
CooklangSharp/
├── CooklangSharp/          # Main library project
├── CooklangSharp.Tests/    # Test project
├── CooklangSharp.Demo/     # Demo console application
└── cooklang-spec/          # Git submodule with Cooklang specification
```

## Key Implementation Details

- Uses .NET 9.0 with nullable reference types enabled
- Parsing approach:
  - Single-pass parser implementation
  - Handles multi-line steps by detecting continuations
  - Special handling for comments to distinguish "--" from "---"
  - Quantity parsing supports numbers, fractions, and text quantities
  - Validates component syntax (e.g., no space after @, #, ~)

## Cooklang Syntax Quick Reference

- `@ingredient` or `@ingredient name{quantity%units}`
- `#cookware` or `#cookware item{}`
- `~timer{duration%units}` or `~named timer{duration%units}`
- `-- comment`
- Metadata block with `---` delimiters
- Empty lines separate steps