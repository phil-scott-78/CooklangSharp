using CooklangSharp.Models;
using CooklangSharp.Tests.Models;
using Shouldly;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
[assembly: CaptureConsole]

namespace CooklangSharp.Tests.Compliance;

public class CanonicalTests
{
    public static TheoryData<string, CanonicalTest> ValidCanonicalTestCases => GetCanonicalTestCases(excludeInvalidTests: false);

    private static TheoryData<string, CanonicalTest> GetCanonicalTestCases(bool excludeInvalidTests = false)
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
            // Skip invalid tests in strict mode - these are tests that contain
            // intentionally invalid syntax that should be treated as text in normal mode
            // but should fail in strict mode
            if (excludeInvalidTests && test.Key.StartsWith("testInvalid"))
                continue;
                
            data.Add(test.Key, test.Value);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(ValidCanonicalTestCases))]
    public void CanonicalTestSuite(string testName, CanonicalTest test)
    {
        RunSingleCanonicalTest(testName, test);
    }

    private void RunSingleCanonicalTest(string testName, CanonicalTest test)
    {
        var result = CooklangParser.Parse(test.Source);
        Console.WriteLine("Expected: ");
        Console.WriteLine(test.ToYaml());
        Console.WriteLine();
        Console.WriteLine("Actual:");
        Console.WriteLine(result.ToYaml());
        
        
        result.Success.ShouldBeTrue($"Test {testName} failed to parse");
        result.Recipe.ShouldNotBeNull($"Test {testName} returned null recipe");
        
        CompareResults(testName, result.Recipe, test.Result);
    }

    private void CompareResults(string testName, Recipe actual, CanonicalResult expected)
    {
        // Compare metadata - for canonical tests, we expect YAML metadata to be in FrontMatter
        // Since we changed the design, we parse the expected metadata and verify it's in FrontMatter
        if (expected.Metadata.Count > 0)
        {
            // Check that front matter contains the expected metadata
            foreach (var kvp in expected.Metadata)
            {
                actual.FrontMatter.ShouldContain($"{kvp.Key}");
                actual.FrontMatter.ShouldContain(kvp.Value.ToString()!);
            }
        }

        // Get all steps from sections
        var allSteps = new List<Step>();
        foreach (var section in actual.Sections)
        {
            foreach (var content in section.Content)
            {
                if (content is StepContent stepContent)
                {
                    allSteps.Add(stepContent.Step);
                }
            }
        }

        // Compare steps
        allSteps.Count.ShouldBe(expected.Steps.Count, $"Test {testName}: Step count mismatch");

        for (int i = 0; i < expected.Steps.Count; i++)
        {
            var actualStep = allSteps[i];
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

    private void CompareQuantity(string testName, int stepIndex, int itemIndex, QuantityValue? actual, object? expected)
    {
        if (expected == null)
        {
            if (actual != null)
            {
                if (actual is TextQuantity { Value: "" })
                {
                    return; // This is acceptable
                }
            }
            return; // No quantity to compare
        }
        
        // Handle different quantity representations
        if (expected is string expectedStr)
        {
            if (actual is TextQuantity textQuantity)
            {
                textQuantity.Value.ShouldBe(expectedStr, 
                    $"Test {testName}: Step {stepIndex}, Item {itemIndex} quantity mismatch");
            }
            else if (actual is FractionalQuantity fractionalQuantity)
            {
                // Mixed fractions are represented as strings in canonical tests
                fractionalQuantity.GetNumericValue().ToString().ShouldBe(expectedStr,
                    $"Test {testName}: Step {stepIndex}, Item {itemIndex} quantity mismatch");
            }
            else
            {
                actual?.ToString().ShouldBe(expectedStr,
                    $"Test {testName}: Step {stepIndex}, Item {itemIndex} quantity mismatch");
            }
        }
        else if (expected is double expectedDouble)
        {
            var actualValue = actual?.GetNumericValue() ?? 0.0;
            actualValue.ShouldBe(expectedDouble, tolerance: 0.0001,
                $"Test {testName}: Step {stepIndex}, Item {itemIndex} quantity mismatch");
        }
        else if (expected is int expectedInt)
        {
            var actualValue = actual?.GetNumericValue() ?? 0.0;
            actualValue.ShouldBe(expectedInt, tolerance: 0.0001,
                $"Test {testName}: Step {stepIndex}, Item {itemIndex} quantity mismatch");
        }
        else
        {
            actual?.ToString().ShouldBe(expected.ToString(),
                $"Test {testName}: Step {stepIndex}, Item {itemIndex} quantity mismatch");
        }
    }
}

public static class YamlExtensions
{
    public static string ToYaml(this object obj)
    {
        var serializer = new SerializerBuilder()
            .WithDefaultScalarStyle(YamlDotNet.Core.ScalarStyle.Plain)
            .Build();
        
        return serializer.Serialize(obj);
    }
}