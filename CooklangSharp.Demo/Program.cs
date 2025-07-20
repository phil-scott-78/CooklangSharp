using CooklangSharp;
using CooklangSharp.Core;
using CooklangSharp.Models;
using Errata;
using Spectre.Console;

var source = """
             Mix @flour{200%g} and @water{100%ml}.
             Add @sugar{41/0%cups} and @bananas{2 to taste.
             """;


var result = CooklangParser.Parse(source, true);
if (result is { Success: true, Recipe: not null })
{
    // Print metadata
    foreach (var meta in result.Recipe.Metadata)
    {
        AnsiConsole.MarkupLine($"[bold]{meta.Key}[/]: {meta.Value}");
    }

    var markup = new System.Text.StringBuilder();
    foreach (var recipeSection in result.Recipe.Sections)
    {
        markup.AppendLine();
        markup.AppendLine(recipeSection.Name);
        markup.AppendLine("===================");
        
        for (var i = 0; i < recipeSection.Content.Count; i++)
        {
            markup.Append($"[bold]{i + 1}.[/] ");
            if (recipeSection.Content[i] is StepContent stepContent)
            {
                foreach (var item in stepContent.Step.Items)
                {
                    switch (item)
                    {
                        case TextItem t:
                            markup.Append(t.Value);
                            break;
                        case IngredientItem ing:
                            var quant = ing.Quantity.ToString() == "0" ? "" : $"{ing.Quantity}";
                            var units = string.IsNullOrWhiteSpace(ing.Units) ? "" : $" {ing.Units}";
                            var detail = (quant != "" || units != "") ? $"({quant}{units})" : "";
                            markup.Append($"[blue]{ing.Name}[/]{detail}");
                            break;
                        case CookwareItem c:
                            var cquant = c.Quantity.ToString() == "0" ? "" : $"{c.Quantity}";
                            var cunits = string.IsNullOrWhiteSpace(c.Units) ? "" : $" {c.Units}";
                            var cdetail = (cquant != "" || cunits != "") ? $"({cquant}{cunits})" : "";
                            markup.Append($"[orange1]{c.Name}[/]{cdetail}");
                            break;
                        case TimerItem timer:
                            markup.Append(
                                $"[green]{timer.Name} ({timer.Quantity}{(string.IsNullOrWhiteSpace(timer.Units) ? "" : " " + timer.Units)})[/]");
                            break;
                        default:
                            markup.Append("[grey]Unknown Item[/]");
                            break;
                    }
                }
            }
        }
    }

    AnsiConsole.MarkupLine(markup.ToString());
}
else
{
    if (result.Errors.Count > 0)
    {
        var inMemorySourceRepository = new InMemorySourceRepository();
        var sourceId = "recipe.cook";
        inMemorySourceRepository.Register(sourceId, source);
        var report = new Report(inMemorySourceRepository);

        var diagnostic = Diagnostic.Error("Parser error");
        // Add all errors to the report
        for (int i = 0; i < result.Errors.Count; i++)
        {
            var error = result.Errors[i];
            diagnostic = diagnostic.WithLabel(new Label(sourceId, new Location(error.Line, error.Column), error.Message)
                        .WithLength(error.Length)
                        .WithPriority(i + 1)
                        .WithColor(Color.Red));
        }
        report.AddDiagnostic(diagnostic);
        report.Render(AnsiConsole.Console);
    }
    else
    {
        AnsiConsole.MarkupLine($"[red]Parse failed with unknown error[/]");    
    }
    
}