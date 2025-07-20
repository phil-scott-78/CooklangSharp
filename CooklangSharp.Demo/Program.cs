using CooklangSharp;
using CooklangSharp.Models;
using Errata;
using Spectre.Console;
using Diagnostic = Errata.Diagnostic;

var source = """
             ---
             yield: 1 kg
             ---
             
             Add @ingredient{quantity}(modifier with (nested but @another{broken
             """;


var result = CooklangParser.Parse(source);
if (result.Recipe != null)
{
    // Print metadata
    foreach (var meta in result.Recipe.Metadata)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold]{meta.Key}[/]: {meta.Value}");
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
                            if (!string.IsNullOrWhiteSpace(ing.Note))
                            {
                                markup.AppendLine($" ([grey]{ing.Note.EscapeMarkup()}[/])");
                            } 
                            
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

if (result.Diagnostics.Count > 0)
{
    var inMemorySourceRepository = new InMemorySourceRepository();
    const string sourceId = "recipe.cook";
    inMemorySourceRepository.Register(sourceId, source);
    var report = new Report(inMemorySourceRepository);

    var diagnostic = result.Diagnostics.Any(i => i.DiagnosticType == DiagnosticType.Error) 
        ? Diagnostic.Error("Parser error") 
        : Diagnostic.Warning("Parser warning");
    
    foreach (var d in result.Diagnostics)
    {
        var color = Color.Yellow;
        if (d.DiagnosticType == DiagnosticType.Error) color = Color.Red;

        diagnostic = diagnostic.WithLabel(new Label(sourceId, new Location(d.Line, d.Column), d.Message)
            .WithLength(d.Length)
            .WithColor(color));
    }

    report.AddDiagnostic(diagnostic);
    report.Render(AnsiConsole.Console);
}
