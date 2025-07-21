# Cooklang Conversion Prompt

## Quick Reference Prompt

```
Convert this recipe to Cooklang format using these rules:

SYNTAX:
• Ingredients: @name or @name{amount} or @name{amount%unit}
  - Multi-word: @olive oil{} or @red bell pepper{2}
  - Fractions: @butter{1/2%cup} or @sugar{1 1/2%cups}
  - Notes: @carrots{2}(diced)
  
• Cookware: #name or #name{quantity}
  - Multi-word: #mixing bowl{} or #cast iron skillet{}
  - Notes: #pan{}(non-stick)
  
• Timers: ~{duration%unit} or ~action{duration%unit}
  - Examples: ~{10%minutes} or ~bake{25%minutes}
  
• Structure:
  - Metadata: YAML front matter between --- markers
  - Sections: == Section Name ==
  - Comments: -- comment

CONVERSION STEPS:
1. Add metadata at top as YAML front matter
2. Create logical sections with == ==
3. Mark all ingredients with @ (use {} for multi-word)
4. Mark all cookware with # (use {} for multi-word)
5. Convert all time references to ~ timers
6. Keep prep instructions in parentheses after ingredients

[PASTE RECIPE HERE]
```

## Detailed Prompt with Examples

```
Convert the following recipe to Cooklang format.

METADATA FORMAT (place at very top):
---
servings: 4
prep time: 20 minutes
cook time: 30 minutes
tags: [italian, vegetarian]
---

QUICK SYNTAX GUIDE:
- Ingredients: @salt, @olive oil{2%tbsp}, @tomatoes{3}(diced)
- Cookware: #pot, #mixing bowl{2}, #pan{}(non-stick)
- Timers: ~{30%minutes}, ~simmer{1%hour}
- Sections: == Preparation ==

IMPORTANT RULES:
1. Multi-word ingredients/cookware need {}: @bell pepper{} not @bell pepper
2. Empty {} means "some": @salt{} = "some salt"
3. Units use %: {2%cups} not {2 cups}
4. Prep notes in parentheses: @onion{1}(chopped)

Please convert this recipe:
[PASTE RECIPE HERE]
```

## Ultra-Concise Prompt

```
Convert to Cooklang:
- Metadata: YAML front matter (---servings: 4---)
- @ingredient{amount%unit} | Multi-word needs {}: @olive oil{}
- #cookware{qty} | Multi-word needs {}: #sheet pan{}
- ~timer{time%unit} | ~{10%min} or ~bake{30%min}
- Sections: == Name ==

[RECIPE HERE]
```

## Example-Based Prompt

```
Convert this recipe to Cooklang format like these examples:

METADATA:
---
servings: 4
cook time: 30 minutes
---

BEFORE: "Add 2 cups flour and 1 tsp salt to a bowl"
AFTER: Add @flour{2%cups} and @salt{1%tsp} to a #bowl{}

BEFORE: "Sauté onions for 5 minutes in a large pan"
AFTER: Sauté @onions{} for ~{5%minutes} in a #large pan{}

BEFORE: "2 lbs chicken breast, cubed"
AFTER: @chicken breast{2%lbs}(cubed)

[YOUR RECIPE HERE]
```

## Advanced Conversion Prompt

```
Convert to Cooklang with these advanced features:

METADATA (YAML front matter at top):
---
servings: 4
prep time: 20 minutes
cook time: 45 minutes
total time: 1 hour 5 minutes
tags: [vegetarian, gluten-free]
author: Chef Name
source: https://example.com
difficulty: medium
---

INGREDIENTS:
- Simple: @salt, @pepper
- With amount: @flour{2}, @milk{1%cup}
- Multi-word: @olive oil{}, @red wine vinegar{2%tbsp}
- Fractions: @butter{1/2%stick}, @sugar{1 1/4%cups}
- With prep: @onions{2}(diced), @garlic{3%cloves}(minced)

COOKWARE:
- Simple: #pot, #knife
- Multi-word: #cutting board{}, #dutch oven{}
- With quantity: #bowl{2}, #baking sheet{3}
- With notes: #pan{}(12-inch), #pot{}(with lid)

TIMERS:
- Anonymous: ~{10%minutes}
- Named: ~rest{30%minutes}, ~marinate{2%hours}
- Ranges: ~{25-30%minutes}

SECTIONS:
== Preparation ==
== Cooking ==
== Assembly ==
== Serving ==

SPECIAL CASES:
- "Some" or "to taste" → use {}
- Optional items → add -- optional comment
- Temperature notes → use -- comments
- Alternative methods → use separate sections

[PASTE YOUR RECIPE]
```

## Checklist Prompt

```
Convert to Cooklang and verify:
☐ Metadata as YAML front matter at top
☐ All ingredients marked with @
☐ Multi-word items use {} (@olive oil{} not @olive oil)
☐ Amounts use % separator ({2%cups} not {2 cups})
☐ All cookware marked with #
☐ Time durations converted to ~{X%unit}
☐ Logical sections with == Section ==
☐ Prep notes in parentheses

[RECIPE HERE]
```

## Common YAML Front Matter Fields

```yaml
---
# Required/Common
servings: 4
prep time: 15 minutes
cook time: 30 minutes
total time: 45 minutes

# Optional
yield: 24 cookies
course: main dish
cuisine: Italian
diet: [vegetarian, gluten-free]
tags: [easy, weeknight, family-friendly]
author: Your Name
source: https://example.com/recipe
source author: Original Chef
difficulty: easy|medium|hard
calories: 350 per serving
rating: 4.5

# Custom fields
leftovers: Refrigerate up to 3 days
freezable: yes
equipment: [blender, food processor]
season: summer
occasion: [potluck, holiday]
---
```