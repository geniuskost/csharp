namespace ForbiddenWordsFinder;

internal static class ConsoleRunner
{
    public static async Task RunAsync(ApplicationOptions options)
    {
        if (options.ForbiddenWords.Count == 0)
        {
            Console.Error.WriteLine("Вкажіть заборонені слова через --words або --words-file.");
            return;
        }

        var scanner = new ForbiddenWordScanner();
        var pauseController = new PauseController();
        using var cancellationTokenSource = new CancellationTokenSource();

        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        var progress = new Progress<ScanProgress>(progressInfo =>
        {
            if (progressInfo.Stage is "drive-start")
            {
                Console.WriteLine($"Scanning {progressInfo.CurrentDrive} ({progressInfo.DriveIndex}/{progressInfo.DriveCount}) with {progressInfo.FilesInDrive} files.");
                return;
            }

            if (progressInfo.Stage is "match")
            {
                Console.WriteLine($"Match in {progressInfo.CurrentFilePath}");
            }
        });

        try
        {
            var summary = await Task.Run(
                () => scanner.Scan(
                    options,
                    progress,
                    pauseController,
                    cancellationTokenSource.Token),
                cancellationTokenSource.Token);

            var reportPath = ReportWriter.Write(summary);
            summary = summary with { ReportPath = reportPath };

            Console.WriteLine();
            Console.WriteLine($"Matched files: {summary.MatchedFiles}");
            Console.WriteLine($"Total replacements: {summary.TotalReplacementCount}");
            Console.WriteLine($"Report saved to: {summary.ReportPath}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation stopped.");
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
        }
    }
}
