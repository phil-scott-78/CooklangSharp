# Cooklang Recipe Conversion Guide

## Overview
This guide helps convert recipes from HTML, Markdown, or plain text into Cooklang format. Cooklang is a markup language for recipes that allows you to define ingredients, cookware, timers, and steps in a human-readable format.

## Cooklang Syntax Reference

### Basic Components

| Component | Syntax | Example |
|-----------|--------|---------|
| **Ingredient** | `@name` or `@name{}` | `@salt` or `@olive oil{}` |
| **Ingredient with amount** | `@name{amount}` | `@flour{2}` |
| **Ingredient with amount & unit** | `@name{amount%unit}` | `@water{500%ml}` |
| **Cookware** | `#name` or `#name{}` | `#pot` or `#mixing bowl{}` |
| **Cookware with quantity** | `#name{quantity}` | `#bowl{2}` |
| **Timer** | `~{duration%unit}` | `~{10%minutes}` |
| **Named timer** | `~name{duration%unit}` | `~bake{25%minutes}` |

### Special Cases

- **Multi-word names**: Use `{}` for ingredients/cookware with spaces: `@red bell pepper{}`, `#cast iron skillet{}`
- **Empty amount**: `@salt{}` means "some salt"
- **Fractions**: `@butter{1/2%cup}` or `@sugar{1 1/2%cups}`
- **Decimals**: `@milk{1.5%liters}`
- **Notes**: Add preparation notes with parentheses: `@carrots{2}(diced)`, `#pan{}(non-stick)`

### Structure Components

| Component | Syntax | Example |
|-----------|--------|---------|
| **Section** | `== Section Name ==` | `== Preparation ==` |
| **Comments** | `-- comment` | `-- This is optional` |
| **Block comments** | `[- comment -]` | `[- Multiple lines -]` |

### Metadata (YAML Front Matter)

Place metadata at the very top of the file between `---` markers:

```yaml
---
servings: 4
prep time: 20 minutes
cook time: 45 minutes
total time: 1 hour 5 minutes
tags: [italian, pasta, vegetarian]
author: Chef Name
source: https://example.com/recipe
---
```

## Conversion Prompt Template

Use this prompt to convert recipes to Cooklang format:

```
Convert the following recipe to Cooklang format:

[PASTE YOUR RECIPE HERE]

Guidelines:
1. Identify all ingredients and mark them with @ (use {} for multi-word ingredients)
2. Add quantities and units where specified (format: {amount%unit})
3. Mark all cookware with # (use {} for multi-word cookware)
4. Convert time references to timers using ~ (format: ~{duration%unit})
5. Organize into logical sections using == Section Name ==
6. Add metadata at the top using YAML front matter (servings, prep time, cook time, etc.)
7. Keep preparation notes in parentheses after ingredients
8. Preserve the recipe's natural flow and readability
```

## Conversion Examples

### Example 1: Simple Recipe Conversion

**Original (Plain Text):**
```
Tomato Basil Pasta
Serves 4

Ingredients:
- 400g pasta
- 2 cups cherry tomatoes
- 3 cloves garlic
- 1/4 cup olive oil
- Fresh basil leaves
- Salt and pepper

Instructions:
1. Boil water in a large pot and cook pasta for 10 minutes.
2. Meanwhile, heat olive oil in a pan and sauté garlic for 2 minutes.
3. Add tomatoes and cook for 5 minutes until soft.
4. Drain pasta and toss with tomato mixture.
5. Garnish with basil and season with salt and pepper.
```

**Converted to Cooklang:**
```
---
servings: 4
---

== Ingredients ==
Gather @pasta{400%g}, @cherry tomatoes{2%cups}, @garlic{3%cloves}, @olive oil{1/4%cup}, @fresh basil leaves{}, @salt, and @pepper.

== Cooking ==
Boil water in a #large pot{} and cook @pasta for ~{10%minutes}.

Meanwhile, heat @olive oil in a #pan{} and sauté @garlic for ~{2%minutes}.

Add @cherry tomatoes and cook for ~{5%minutes} until soft.

Drain pasta and toss with tomato mixture.

Garnish with @fresh basil leaves and season with @salt and @pepper.
```

### Example 2: Complex Recipe Conversion

**Original (Markdown):**
```markdown
# Beef Stew

**Prep:** 20 minutes  
**Cook:** 2 hours  
**Serves:** 6

## Ingredients
- 2 lbs beef chuck, cubed
- 3 carrots, diced
- 2 onions, chopped
- 4 potatoes, quartered
- 2 tbsp tomato paste
- 4 cups beef broth
- 1 tsp dried thyme
- 2 bay leaves
- Salt to taste

## Instructions
1. Season beef with salt and pepper. Brown in a Dutch oven over high heat.
2. Remove beef, reduce heat to medium. Add onions and cook until soft (5 min).
3. Add tomato paste, cook 1 minute. Add broth, thyme, and bay leaves.
4. Return beef, bring to boil. Reduce heat, cover, simmer 1 hour.
5. Add carrots and potatoes. Continue simmering 45 minutes until tender.
```

**Converted to Cooklang:**
```
---
servings: 6
prep time: 20 minutes
cook time: 2 hours
---

== Preparation ==
@beef chuck{2%lbs}(cubed)
@carrots{3}(diced)
@onions{2}(chopped)
@potatoes{4}(quartered)

== Cooking ==
Season @beef chuck with @salt{} and @pepper{}. Brown in a #Dutch oven{} over high heat.

Remove beef, reduce heat to medium. Add @onions and cook until soft for ~{5%minutes}.

Add @tomato paste{2%tbsp}, cook for ~{1%minute}. Add @beef broth{4%cups}, @dried thyme{1%tsp}, and @bay leaves{2}.

Return beef, bring to boil. Reduce heat, cover, and simmer for ~{1%hour}.

Add @carrots and @potatoes. Continue simmering for ~{45%minutes} until tender.
```

### Example 3: Baking Recipe Conversion

**Original (HTML):**
```html
<h2>Chocolate Chip Cookies</h2>
<p>Makes: 24 cookies</p>

<h3>Ingredients:</h3>
<ul>
  <li>1 cup butter (softened)</li>
  <li>3/4 cup sugar</li>
  <li>3/4 cup brown sugar</li>
  <li>2 eggs</li>
  <li>1 tsp vanilla</li>
  <li>2 1/4 cups flour</li>
  <li>1 tsp baking soda</li>
  <li>1 tsp salt</li>
  <li>2 cups chocolate chips</li>
</ul>

<h3>Directions:</h3>
<ol>
  <li>Preheat oven to 375°F</li>
  <li>Cream butter and sugars until fluffy</li>
  <li>Beat in eggs and vanilla</li>
  <li>Mix flour, baking soda, and salt</li>
  <li>Stir in chocolate chips</li>
  <li>Drop on baking sheets</li>
  <li>Bake 9-11 minutes</li>
</ol>
```

**Converted to Cooklang:**
```
---
makes: 24 cookies
---

== Preparation ==
-- Preheat oven to 375°F
@butter{1%cup}(softened)

== Mixing ==
Cream @butter and @sugar{3/4%cup} and @brown sugar{3/4%cup} in a #mixing bowl{} until fluffy.

Beat in @eggs{2} and @vanilla{1%tsp}.

In a separate #bowl{}, mix @flour{2 1/4%cups}, @baking soda{1%tsp}, and @salt{1%tsp}.

Stir in @chocolate chips{2%cups}.

== Baking ==
Drop on #baking sheets{} and bake for ~baking{9-11%minutes}.
```

## Conversion Tips

### 1. Ingredient Recognition
- Look for measurement words: cup, tbsp, tsp, oz, g, kg, ml, L
- Numbers before ingredients usually indicate amounts
- "Some", "a pinch of", "to taste" → use empty braces `{}`

### 2. Cookware Identification
- Common cookware: pot, pan, skillet, bowl, baking sheet, Dutch oven
- Size descriptors become part of the name: `#large pot{}`
- Quantity words (2 bowls) → `#bowl{2}`

### 3. Timer Extraction
- Look for time phrases: "for X minutes", "until", "about X hours"
- Named timers for specific actions: `~marinate{2%hours}`, `~rest{10%minutes}`
- Ranges can be kept: `~{9-11%minutes}`

### 4. Section Organization
- Group by logical cooking phases: Prep, Cooking, Assembly, Serving
- Keep related steps together
- Use descriptive section names

### 5. Metadata Extraction
- Common metadata: servings, yield, prep time, cook time, total time
- Course type: appetizer, main, dessert
- Dietary info: vegetarian, gluten-free, etc.
- Format as YAML between --- markers:
```yaml
---
servings: 4
prep time: 15 minutes
cook time: 30 minutes
tags: [vegetarian, gluten-free]
---
```

## Advanced Formatting

### Recipe References
For ingredients that are other recipes:
```
@homemade pasta{200%g} -- Links to pasta.cook file
@tomato sauce{1%cup} -- Links to tomato-sauce.cook file
```

### Optional Ingredients
Use comments to indicate optional items:
```
@oregano{1%tsp} -- optional
@red pepper flakes{} -- to taste
```

### Multiple Cooking Methods
Use sections to separate different methods:
```
== Oven Method ==
Bake at 350°F for ~{45%minutes}.

== Stovetop Method ==
Simmer in a #covered pot{} for ~{1%hour}.
```

## Common Patterns

### Mise en Place Style
```
== Mise en Place ==
@onion{1}(diced)
@garlic{3%cloves}(minced)
@tomatoes{4}(chopped)
#cutting board{}
#knife{}
```

### Ingredient Groupings
```
== Dry Ingredients ==
In a #bowl{}, mix @flour{2%cups}, @baking powder{1%tsp}, and @salt{1/2%tsp}.

== Wet Ingredients ==
In another #bowl{}, whisk @eggs{2}, @milk{1%cup}, and @oil{1/4%cup}.
```

### Temperature Settings
```
-- Preheat oven to 350°F (175°C)
-- Heat oil to 375°F for deep frying
```

## Validation Checklist

After conversion, verify:
- [ ] All ingredients are marked with `@`
- [ ] Multi-word ingredients/cookware use `{}`
- [ ] Quantities follow `{amount%unit}` format
- [ ] All cookware is marked with `#`
- [ ] Time durations are converted to timers `~`
- [ ] Recipe has appropriate sections with `==`
- [ ] Metadata is at the top as YAML front matter
- [ ] Preparation notes are in parentheses
- [ ] Recipe flows naturally when read aloud