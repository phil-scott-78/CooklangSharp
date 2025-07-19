using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CooklangSharp.Core;
using CooklangSharp.Models;
using CooklangSharp.Tests.Models;
using Shouldly;

namespace CooklangSharp.Tests;

public class CanonicalTests
{
    private readonly string _canonicalPath;

    public CanonicalTests()
    {
        _canonicalPath = PathUtils.GetPath("../cooklang-spec/tests/canonical.yaml");
    }

    [Fact]
    public void BasicTextParsing()
    {
        var result = CooklangParser.Parse("Add a bit of chilli");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Steps.Count.ShouldBe(1);
        result.Recipe.Steps[0].Items.Count.ShouldBe(1);
        result.Recipe.Steps[0].Items[0].Type.ShouldBe("text");
    }

    [Fact]
    public void CommentOnlyLine()
    {
        var result = CooklangParser.Parse("-- testing comments");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Steps.Count.ShouldBe(0);
    }

    [Fact]
    public void BasicMetadata()
    {
        var source = """
            ---
            sourced: babooshka
            ---
            """;
        
        var result = CooklangParser.Parse(source);
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Steps.Count.ShouldBe(0);
        result.Recipe.Metadata.ShouldContainKey("sourced");
        result.Recipe.Metadata["sourced"].ShouldBe("babooshka");
    }

    [Fact]
    public void IngredientWithQuantityAndUnits()
    {
        var result = CooklangParser.Parse("@chilli{3%items}");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Steps.Count.ShouldBe(1);
        result.Recipe.Steps[0].Items.Count.ShouldBe(1);
        
        var ingredient = result.Recipe.Steps[0].Items[0] as IngredientItem;
        ingredient.ShouldNotBeNull();
        ingredient.Name.ShouldBe("chilli");
        ingredient.Quantity.ShouldBe(3.0);
        ingredient.Units.ShouldBe("items");
    }

    [Fact]
    public void IngredientSingleWord()
    {
        var result = CooklangParser.Parse("@chilli");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Steps.Count.ShouldBe(1);
        result.Recipe.Steps[0].Items.Count.ShouldBe(1);
        
        var ingredient = result.Recipe.Steps[0].Items[0] as IngredientItem;
        ingredient.ShouldNotBeNull();
        ingredient.Name.ShouldBe("chilli");
        ingredient.Quantity.ShouldBe("some");
        ingredient.Units.ShouldBe("");
    }

    [Fact]
    public void FractionQuantity()
    {
        var result = CooklangParser.Parse("@milk{1/2%cup}");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Steps.Count.ShouldBe(1);
        result.Recipe.Steps[0].Items.Count.ShouldBe(1);
        
        var ingredient = result.Recipe.Steps[0].Items[0] as IngredientItem;
        ingredient.ShouldNotBeNull();
        ingredient.Name.ShouldBe("milk");
        ingredient.Quantity.ShouldBe(0.5);
        ingredient.Units.ShouldBe("cup");
    }

    [Fact]
    public void MixedTextAndIngredients()
    {
        var result = CooklangParser.Parse("Add @chilli{3%items}, @ginger{10%g} and @milk{1%l}.");
        
        result.Success.ShouldBeTrue();
        result.Recipe.ShouldNotBeNull();
        result.Recipe.Steps.Count.ShouldBe(1);
        result.Recipe.Steps[0].Items.Count.ShouldBe(7);
        
        result.Recipe.Steps[0].Items[0].Type.ShouldBe("text");
        result.Recipe.Steps[0].Items[1].Type.ShouldBe("ingredient");
        result.Recipe.Steps[0].Items[2].Type.ShouldBe("text");
        result.Recipe.Steps[0].Items[3].Type.ShouldBe("ingredient");
        result.Recipe.Steps[0].Items[4].Type.ShouldBe("text");
        result.Recipe.Steps[0].Items[5].Type.ShouldBe("ingredient");
        result.Recipe.Steps[0].Items[6].Type.ShouldBe("text");
    }

    public static TheoryData<string, CanonicalTest> CanonicalTestCases => GetCanonicalTestCases();

    private static TheoryData<string, CanonicalTest> GetCanonicalTestCases()
    {
        var data = new TheoryData<string, CanonicalTest>();
        var canonicalPath = PathUtils.GetPath("../cooklang-spec/tests/canonical.yaml");
        if (!File.Exists(canonicalPath))
        {
            throw new FileNotFoundException($"Canonical test file not found at: {canonicalPath}");
        }

        var yaml = File.ReadAllText(canonicalPath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var canonicalData = deserializer.Deserialize<CanonicalTestData>(yaml);
        foreach (var test in canonicalData.Tests)
        {
            data.Add(test.Key, test.Value);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(CanonicalTestCases))]
    public void CanonicalTheoryTest(string testName, CanonicalTest test)
    {
        RunSingleCanonicalTest(testName, test);
    }

    private void RunSingleCanonicalTest(string testName, CanonicalTest test)
    {
        var result = CooklangParser.Parse(test.Source);
        
        result.Success.ShouldBeTrue($"Test {testName} failed to parse");
        result.Recipe.ShouldNotBeNull($"Test {testName} returned null recipe");
        
        CompareResults(testName, result.Recipe, test.Result);
    }

    private void CompareResults(string testName, Recipe actual, CanonicalResult expected)
    {
        // Compare metadata
        actual.Metadata.Count.ShouldBe(expected.Metadata.Count, $"Test {testName}: Metadata count mismatch");
        
        foreach (var kvp in expected.Metadata)
        {
            actual.Metadata.ShouldContainKey(kvp.Key, $"Test {testName}: Missing metadata key {kvp.Key}");
            actual.Metadata[kvp.Key].ToString().ShouldBe(kvp.Value.ToString(), 
                $"Test {testName}: Metadata value mismatch for key {kvp.Key}");
        }

        // Compare steps
        actual.Steps.Count.ShouldBe(expected.Steps.Count, $"Test {testName}: Step count mismatch");
        
        for (int i = 0; i < expected.Steps.Count; i++)
        {
            var actualStep = actual.Steps[i];
            var expectedStep = expected.Steps[i];
            
            actualStep.Items.Count.ShouldBe(expectedStep.Count, 
                $"Test {testName}: Step {i} item count mismatch");
            
            for (int j = 0; j < expectedStep.Count; j++)
            {
                var actualItem = actualStep.Items[j];
                var expectedItem = expectedStep[j];
                
                CompareItem(testName, i, j, actualItem, expectedItem);
            }
        }
    }

    private void CompareItem(string testName, int stepIndex, int itemIndex, Item actual, CanonicalItem expected)
    {
        actual.Type.ShouldBe(expected.Type, 
            $"Test {testName}: Step {stepIndex}, Item {itemIndex} type mismatch");

        switch (expected.Type)
        {
            case "text":
                var textItem = actual as TextItem;
                textItem.ShouldNotBeNull($"Test {testName}: Expected TextItem at step {stepIndex}, item {itemIndex}");
                textItem.Value.ShouldBe(expected.Value, 
                    $"Test {testName}: Step {stepIndex}, Item {itemIndex} text value mismatch");
                break;

            case "ingredient":
                var ingredientItem = actual as IngredientItem;
                ingredientItem.ShouldNotBeNull($"Test {testName}: Expected IngredientItem at step {stepIndex}, item {itemIndex}");
                ingredientItem.Name.ShouldBe(expected.Name, 
                    $"Test {testName}: Step {stepIndex}, Item {itemIndex} ingredient name mismatch");
                CompareQuantity(testName, stepIndex, itemIndex, ingredientItem.Quantity, expected.Quantity);
                ingredientItem.Units.ShouldBe(expected.Units ?? "", 
                    $"Test {testName}: Step {stepIndex}, Item {itemIndex} ingredient units mismatch");
                break;

            case "cookware":
                var cookwareItem = actual as CookwareItem;
                cookwareItem.ShouldNotBeNull($"Test {testName}: Expected CookwareItem at step {stepIndex}, item {itemIndex}");
                cookwareItem.Name.ShouldBe(expected.Name, 
                    $"Test {testName}: Step {stepIndex}, Item {itemIndex} cookware name mismatch");
                CompareQuantity(testName, stepIndex, itemIndex, cookwareItem.Quantity, expected.Quantity);
                break;

            case "timer":
                var timerItem = actual as TimerItem;
                timerItem.ShouldNotBeNull($"Test {testName}: Expected TimerItem at step {stepIndex}, item {itemIndex}");
                timerItem.Name.ShouldBe(expected.Name ?? "", 
                    $"Test {testName}: Step {stepIndex}, Item {itemIndex} timer name mismatch");
                CompareQuantity(testName, stepIndex, itemIndex, timerItem.Quantity, expected.Quantity);
                timerItem.Units.ShouldBe(expected.Units ?? "", 
                    $"Test {testName}: Step {stepIndex}, Item {itemIndex} timer units mismatch");
                break;
        }
    }

    private void CompareQuantity(string testName, int stepIndex, int itemIndex, object actual, object? expected)
    {
        if (actual == null)
        {
            return;
        }
        if (expected == null)
        {
            return; // No quantity to compare
        }
        
        // Handle different quantity representations
        if (expected is string expectedStr && actual is string actualStr)
        {
            actualStr.ShouldBe(expectedStr, 
                $"Test {testName}: Step {stepIndex}, Item {itemIndex} quantity mismatch");
        }
        else if (expected is double expectedDouble && actual is double actualDouble)
        {
            actualDouble.ShouldBe(expectedDouble, tolerance: 0.0001,
                $"Test {testName}: Step {stepIndex}, Item {itemIndex} quantity mismatch");
        }
        else if (expected is int expectedInt)
        {
            if (actual is double actualAsDouble)
            {
                actualAsDouble.ShouldBe(expectedInt, tolerance: 0.0001,
                    $"Test {testName}: Step {stepIndex}, Item {itemIndex} quantity mismatch");
            }
            else if (actual is int actualAsInt)
            {
                actualAsInt.ShouldBe(expectedInt,
                    $"Test {testName}: Step {stepIndex}, Item {itemIndex} quantity mismatch");
            }
            else
            {
                actual.ToString().ShouldBe(expectedInt.ToString(),
                    $"Test {testName}: Step {stepIndex}, Item {itemIndex} quantity mismatch");
            }
        }
        else
        {
            actual.ToString().ShouldBe(expected.ToString(),
                $"Test {testName}: Step {stepIndex}, Item {itemIndex} quantity mismatch");
        }
    }
}