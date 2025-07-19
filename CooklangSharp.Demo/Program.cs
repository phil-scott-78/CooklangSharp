using CooklangSharp.Core;
using CooklangSharp.Models;
using Spectre.Console;

var source = """
             = Dough
             
             Mix @flour{200%g} and @water{100%ml} together until smooth.
             
             == Filling ==
             
             Combine @cheese{100%g} and @spinach{50%g}, then season to taste.
             """;


var result = CooklangParser.Parse(source);
if (result.Success && result.Recipe != null)
{
    // Print metadata
    foreach (var meta in result.Recipe.Metadata)
    {
        AnsiConsole.MarkupLine($"[bold]{meta.Key}[/]: {meta.Value}");
    }

    AnsiConsole.MarkupLine("[bold underline]Steps:[/]");
    for (int i = 0; i < result.Recipe.Steps.Count; i++)
    {
        var markup = new System.Text.StringBuilder();
        markup.Append($"[bold]{i + 1}.[/] ");
        foreach (var item in result.Recipe.Steps[i].Items)
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
                    var cquant = c.Quantity is null || c.Quantity.ToString() == "0" ? "" : $"{c.Quantity}";
                    var cunits = string.IsNullOrWhiteSpace(c.Units) ? "" : $" {c.Units}";
                    var cdetail = (cquant != "" || cunits != "") ? $"({cquant}{cunits})" : "";
                    markup.Append($"[orange1]{c.Name}[/]{cdetail}");
                    break;
                case TimerItem timer:
                    markup.Append($"[green]{timer.Name} ({timer.Quantity}{(string.IsNullOrWhiteSpace(timer.Units) ? "" : " " + timer.Units)})[/]");
                    break;
                default:
                    markup.Append("[grey]Unknown Item[/]");
                    break;
            }
        }
        AnsiConsole.MarkupLine(markup.ToString());
    }
}
else
{
    AnsiConsole.MarkupLine($"[red]Parse failed: {result.Error}[/]");
}
