using System.Text;

namespace ForbiddenWordsFinder;

internal static class ReportWriter
{
    public static string Write(ScanSummary summary)
    {
        var reportDirectory = Path.Combine(summary.OutputDirectory, "reports");
        Directory.CreateDirectory(reportDirectory);

        var reportPath = Path.Combine(reportDirectory, $"report_{summary.StartedAt:yyyyMMdd_HHmmss}.txt");
        var builder = new StringBuilder();

        builder.AppendLine("Forbidden Words Finder Report");
        builder.AppendLine(new string('=', 34));
        builder.AppendLine($"Started: {summary.StartedAt:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"Finished: {summary.FinishedAt:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"Output directory: {summary.OutputDirectory}");
        builder.AppendLine($"Forbidden words: {string.Join(", ", summary.ForbiddenWords)}");
        builder.AppendLine($"Scanned files: {summary.TotalScannedFiles}");
        builder.AppendLine($"Matched files: {summary.MatchedFiles}");
        builder.AppendLine($"Total replacements: {summary.TotalReplacementCount}");
        builder.AppendLine();

        builder.AppendLine("Matched files");
        builder.AppendLine(new string('-', 34));
        if (summary.Matches.Count == 0)
        {
            builder.AppendLine("No files with forbidden words were found.");
        }
        else
        {
            foreach (var item in summary.Matches)
            {
                builder.AppendLine($"Source: {item.SourcePath}");
                builder.AppendLine($"Size: {item.SizeBytes} bytes");
                builder.AppendLine($"Replacements: {item.ReplacementCount}");
                builder.AppendLine($"Original copy: {item.OriginalCopyPath}");
                builder.AppendLine($"Redacted copy: {item.RedactedCopyPath}");
                builder.AppendLine();
            }
        }

        builder.AppendLine("Top 10 forbidden words");
        builder.AppendLine(new string('-', 34));
        var topWords = summary.WordPopularity
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToArray();

        if (topWords.Length == 0)
        {
            builder.AppendLine("No replacements recorded.");
        }
        else
        {
            for (var index = 0; index < topWords.Length; index++)
            {
                var pair = topWords[index];
                builder.AppendLine($"{index + 1}. {pair.Key} - {pair.Value}");
            }
        }

        File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
        return reportPath;
    }
}
