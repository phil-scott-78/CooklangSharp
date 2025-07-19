# Cooklang Parser Tasks

## Core Project Setup
- [x] Setup C# Project Structure (Library + Test)
- [ ] Create core models and data structures
- [ ] Implement YAML test-runner for integration testing

## Lexer Tokens
- [ ] Tokenize Metadata Delimiter (`---`)
- [ ] Tokenize Comments (`--`)
- [ ] Tokenize Special Characters (`@`, `#`, `~`, `{`, `}`, `%`)
- [ ] Tokenize Text/Words
- [ ] Tokenize Numbers and Fractions
- [ ] Tokenize Whitespace and Newlines

## Parser Components - Metadata
- [ ] Parse Metadata Block (between `---` delimiters)
- [ ] Parse Metadata Key-Value Pairs
- [ ] Handle Multi-word Metadata Keys

## Parser Components - Basic Elements
- [ ] Parse Comment-only lines (`--`)
- [ ] Parse Comments after content
- [ ] Parse Plain Text runs
- [ ] Handle Multiple Lines and Step Separation
- [ ] Handle Empty Lines

## Parser Components - Ingredients (`@`)
- [ ] Parse Ingredient: single-word (`@salt`)
- [ ] Parse Ingredient: multi-word (`@hot chilli{}`)
- [ ] Parse Ingredient: quantity only (`@milk{250}`)
- [ ] Parse Ingredient: quantity and units (`@milk{250%ml}`)
- [ ] Parse Ingredient: text quantities (`@thyme{few%sprigs}`)
- [ ] Parse Ingredient: fractions (`@milk{1/2%cup}`)
- [ ] Parse Ingredient: decimals (`@water{1.5%cups}`)
- [ ] Parse Ingredient: with leading numbers (`@1000 island dressing{}`)
- [ ] Parse Ingredient: with emoji (`@🧂`)
- [ ] Handle Invalid Ingredient Syntax

## Parser Components - Cookware (`#`)
- [ ] Parse Cookware: single-word (`#pan`)
- [ ] Parse Cookware: multi-word (`#frying pan{}`)
- [ ] Parse Cookware: with quantity (`#frying pan{2}`)
- [ ] Parse Cookware: text quantities (`#frying pan{two small}`)
- [ ] Parse Cookware: with leading numbers (`#7-inch nonstick frying pan{}`)
- [ ] Handle Invalid Cookware Syntax

## Parser Components - Timers (`~`)
- [ ] Parse Timer: single-word name only (`~rest`)
- [ ] Parse Timer: anonymous with time (`~{10%minutes}`)
- [ ] Parse Timer: named with time (`~potato{42%minutes}`)
- [ ] Parse Timer: fractions (`~{1/2%hour}`)
- [ ] Parse Timer: decimals (`~{1.5%minutes}`)
- [ ] Handle Invalid Timer Syntax

## Advanced Parsing Features
- [ ] Handle Unicode Characters (degrees, emoji, special punctuation)
- [ ] Handle Whitespace Variations (spaces, tabs, Unicode whitespace)
- [ ] Parse Fractions with Spaces (`1 / 2`)
- [ ] Handle Edge Cases (leading numbers, punctuation boundaries)
- [ ] Error Handling and Recovery

## Finalization
- [ ] Pass all tests in `canonical.yaml`
- [ ] Integration test runner
- [ ] Code cleanup and documentation
- [ ] Performance optimization