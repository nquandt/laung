using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Antlr4.Runtime;
using Language;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

var settings = new JsonSerializerOptions()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

string prefix = "./Tests/";

Matcher matcher = new();
matcher.AddIncludePatterns(new[] { "/**/code.esx" });

PatternMatchingResult results = matcher.Execute(
    new DirectoryInfoWrapper(
        new DirectoryInfo(prefix)));

foreach (var result in results.Files)
{    
    var code = System.IO.File.ReadAllText(Path.Join(prefix, result.Path), Encoding.UTF8);
    var expectedRaw = System.IO.File.ReadAllText(Path.Join(prefix, result.Path.Replace("code.esx", "expected.json")), Encoding.UTF8);

    var expected = JsonSerializer.Deserialize<List<JsonToken>>(expectedRaw, settings)!;

    var allTokens = GetAllTokens(code);

    var resultt = JsonSerializer.Serialize(allTokens, settings);

    System.IO.File.WriteAllText(Path.Join(prefix, result.Path.Replace("code.esx", "result.json")), resultt, Encoding.UTF8);
    System.IO.File.WriteAllText(Path.Join(prefix, result.Path.Replace("code.esx", "ast.json")), DoStuff(code), Encoding.UTF8);

    if (!CompareJsonLists(allTokens, expected))
    {
        throw new Exception("Test failed");
    }

    Console.WriteLine("All good");
}









static string DoStuff(string input)
{
    AntlrInputStream inputStream = new AntlrInputStream(input);
    LanguageLexer speakLexer = new LanguageLexer(inputStream);
    CommonTokenStream commonTokenStream = new CommonTokenStream(speakLexer);
    LanguageParser speakParser = new LanguageParser(commonTokenStream);

    LanguageParser.Compilation_unitContext startContext = speakParser.compilation_unit();

    return startContext.ToStringTree();
}





static bool CompareJsonLists(IEnumerable<JsonToken> list1, IEnumerable<JsonToken> list2)
{
    return Enumerable.SequenceEqual(list1, list2, new JsonTokenComparer());
}

static IEnumerable<JsonToken> GetAllTokens(string input)
{
    var bytes = System.Text.Encoding.UTF8.GetBytes(input);

    var tokenizer = new Tokenizer(bytes);

    var allTokens = new List<JsonToken>();
    while (tokenizer.HasMoreTokens())
    {
        var token = tokenizer.GetNextToken();
        if (token != null)
        {
            allTokens.Add(new JsonToken()
            {
                Kind = token.Kind.ToString(),
                StringValue = token.StringValue
            });
        }
    }

    return allTokens;
}

class JsonTokenComparer : IEqualityComparer<JsonToken>
{
    public bool Equals(JsonToken? x, JsonToken? y)
    {
        //Console.WriteLine("x: " + JsonSerializer.Serialize(x));
        //Console.WriteLine("y: " + JsonSerializer.Serialize(y));
        if (x == null && y == null)
        {
            return true;
        }

        return x != null && y != null && x.Kind == y.Kind && x.StringValue == y.StringValue;
    }

    public int GetHashCode([DisallowNull] JsonToken obj)
    {
        return obj.GetHashCode();
    }
}

class JsonToken
{
    public string StringValue { get; set; } = default!;
    public string Kind { get; set; } = default!;
}