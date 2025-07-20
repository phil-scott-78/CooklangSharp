using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CooklangSharp.Tests;

public class XUnit
{
    [Fact]
    public void Debug_Method()
    {
        
        var source = "Mix @flour{200%g} and @water{100%ml}.\nAdd @sugar{1/0%cups} to taste.";


        var result = CooklangParser.Parse(source);
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(result);
        Console.WriteLine(yaml);
    }
}
