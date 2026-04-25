using System.Globalization;

namespace ForbiddenWordsFinder;

internal sealed record ApplicationOptions(
    IReadOnlyList<string> ForbiddenWords,
    string OutputDirectory,
    bool Headless)
{
    public static ApplicationOptions Parse(string[] args)
    {
        var headless = false;
        var outputDirectory = Path.Combine(Environment.CurrentDirectory, "ForbiddenWordsOutput");
        var words = new List<string>();

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index].Trim();

            switch (argument.ToLowerInvariant())
            {
                case "--nogui":
                case "--headless":
                    headless = true;
                    break;

                case "--output":
                    if (index + 1 < args.Length)
                    {
                        outputDirectory = args[++index];
                    }
                    break;

                case "--words":
                    if (index + 1 < args.Length)
                    {
                        words.AddRange(SplitWords(args[++index]));
                    }
                    break;

                case "--words-file":
                    if (index + 1 < args.Length)
                    {
                        var path = args[++index];
                        if (File.Exists(path))
                        {
                            words.AddRange(File.ReadAllLines(path));
                        }
                    }
                    break;
            }
        }

        return new ApplicationOptions(
            words.Select(word => word.Trim()).Where(word => !string.IsNullOrWhiteSpace(word)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            outputDirectory,
            headless);
    }

    private static IEnumerable<string> SplitWords(string value)
    {
        return value.Split(new[] { ',', ';', '|', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
