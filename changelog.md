# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Added
- Initial project setup with C# class library and unit test projects
- Created `tasks.md` with complete development roadmap derived from EBNF grammar
- Created `changelog.md` for tracking development progress
- Project configured with .NET 9.0, xUnit testing framework, Shouldly assertions, and YamlDotNet for canonical test parsing
- Core model classes: Recipe, Step, Item hierarchy (TextItem, IngredientItem, CookwareItem, TimerItem)
- Complete cooklang parser implementation based on EBNF grammar specification
- Metadata block parsing (between `---` delimiters)
- Comment parsing (`--` but not `---`)
- Ingredient parsing (`@`) with single-word, multi-word, quantities, units, and fractions
- Cookware parsing (`#`) with single-word, multi-word, and quantities
- Timer parsing (`~`) with single-word, anonymous, and named variants
- Multi-line step parsing with proper whitespace handling
- Text parsing for mixed content with proper token merging
- Fraction parsing with leading zero detection (e.g., "01/2" stays as text)
- Invalid syntax detection (spaces after component markers are treated as text)
- Context-aware spacing for continuation lines (2 spaces for components, 1 space for text)
- Comprehensive canonical test runner that passes all tests from cooklang-spec
- **ALL CANONICAL TESTS PASSING** - Complete implementation of cooklang specification