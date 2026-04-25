using System.Collections.Concurrent;
using System.Text;

namespace ForbiddenWordsFinder;

internal sealed class ForbiddenWordScanner
{
    public ScanSummary Scan(
        ApplicationOptions options,
        IProgress<ScanProgress>? progress,
        PauseController pauseController,
        CancellationToken cancellationToken,
        Action<FoundFileInfo>? onMatch = null)
    {
        var startedAt = DateTime.Now;
        var forbiddenWords = options.ForbiddenWords
            .Select(word => word.Trim())
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(word => word.Length)
            .ToArray();

        if (forbiddenWords.Length == 0)
        {
            throw new InvalidOperationException("Введіть хоча б одне заборонене слово.");
        }

        Directory.CreateDirectory(options.OutputDirectory);
        var originalRoot = Path.Combine(options.OutputDirectory, "original");
        var redactedRoot = Path.Combine(options.OutputDirectory, "redacted");
        Directory.CreateDirectory(originalRoot);
        Directory.CreateDirectory(redactedRoot);

        var matches = new ConcurrentBag<FoundFileInfo>();
        var wordPopularity = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var totalScannedFiles = 0;
        var matchedFiles = 0;
        var totalReplacementCount = 0;

        var drives = DriveInfo.GetDrives()
            .Where(IsSearchableDrive)
            .ToArray();

        for (var driveIndex = 0; driveIndex < drives.Length; driveIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pauseController.WaitIfPaused(cancellationToken);

            var drive = drives[driveIndex];
            var filePaths = EnumerateFilesSafe(drive.RootDirectory.FullName).ToArray();
            var driveProcessed = 0;

            progress?.Report(new ScanProgress(
                "drive-start",
                drive.Name,
                driveIndex + 1,
                drives.Length,
                filePaths.Length,
                0,
                matchedFiles,
                null));

            Parallel.ForEach(
                filePaths,
                new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount)
                },
                filePath =>
                {
                    pauseController.WaitIfPaused(cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    var processedInDrive = Interlocked.Increment(ref driveProcessed);
                    Interlocked.Increment(ref totalScannedFiles);

                    var document = TryReadTextFile(filePath);
                    if (document is null)
                    {
                        progress?.Report(new ScanProgress(
                            "file-scan",
                            drive.Name,
                            driveIndex + 1,
                            drives.Length,
                            filePaths.Length,
                            processedInDrive,
                            matchedFiles,
                            filePath));
                        return;
                    }

                    var redaction = RedactContent(document.Content, forbiddenWords);
                    if (!redaction.HasChanges)
                    {
                        progress?.Report(new ScanProgress(
                            "file-scan",
                            drive.Name,
                            driveIndex + 1,
                            drives.Length,
                            filePaths.Length,
                            processedInDrive,
                            matchedFiles,
                            filePath));
                        return;
                    }

                    var relativePath = Path.GetRelativePath(drive.RootDirectory.FullName, filePath);
                    var originalCopyPath = BuildOutputPath(originalRoot, drive, relativePath);
                    var redactedCopyPath = BuildOutputPath(redactedRoot, drive, relativePath);

                    CopyFile(filePath, originalCopyPath);
                    WriteTextFile(redactedCopyPath, redaction.Content, document.Encoding, cancellationToken);

                    var replacementCount = redaction.WordCounts.Values.Sum();
                    Interlocked.Increment(ref matchedFiles);
                    Interlocked.Add(ref totalReplacementCount, replacementCount);

                    foreach (var pair in redaction.WordCounts)
                    {
                        wordPopularity.AddOrUpdate(pair.Key, pair.Value, (_, current) => current + pair.Value);
                    }

                    var match = new FoundFileInfo(
                        filePath,
                        originalCopyPath,
                        redactedCopyPath,
                        GetFileSizeSafe(filePath),
                        replacementCount);

                    matches.Add(match);
                    onMatch?.Invoke(match);

                    progress?.Report(new ScanProgress(
                        "match",
                        drive.Name,
                        driveIndex + 1,
                        drives.Length,
                        filePaths.Length,
                        processedInDrive,
                        matchedFiles,
                        filePath));
                });

            progress?.Report(new ScanProgress(
                "drive-end",
                drive.Name,
                driveIndex + 1,
                drives.Length,
                filePaths.Length,
                filePaths.Length,
                matchedFiles,
                null));
        }

        return new ScanSummary(
            startedAt,
            DateTime.Now,
            forbiddenWords,
            matches.OrderBy(item => item.SourcePath, StringComparer.OrdinalIgnoreCase).ToArray(),
            wordPopularity.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase),
            totalScannedFiles,
            matchedFiles,
            totalReplacementCount,
            options.OutputDirectory,
            string.Empty);
    }

    private static bool IsSearchableDrive(DriveInfo drive)
    {
        try
        {
            return drive.IsReady && drive.DriveType is DriveType.Fixed or DriveType.Removable or DriveType.Network;
        }
        catch
        {
            return false;
        }
    }

    private static IEnumerable<string> EnumerateFilesSafe(string rootDirectory)
    {
        var stack = new Stack<string>();
        stack.Push(rootDirectory);

        while (stack.Count > 0)
        {
            var currentDirectory = stack.Pop();
            string[] files;
            string[] directories;

            try
            {
                files = Directory.GetFiles(currentDirectory);
                directories = Directory.GetDirectories(currentDirectory);
            }
            catch
            {
                continue;
            }

            foreach (var file in files)
            {
                yield return file;
            }

            foreach (var subdirectory in directories)
            {
                stack.Push(subdirectory);
            }
        }
    }

    private static TextDocument? TryReadTextFile(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);
            var content = reader.ReadToEnd();

            if (content.IndexOf('\0') >= 0)
            {
                return null;
            }

            return new TextDocument(content, reader.CurrentEncoding);
        }
        catch
        {
            return null;
        }
    }

    private static RedactionResult RedactContent(string content, IReadOnlyList<string> forbiddenWords)
    {
        var current = content;
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var hasChanges = false;

        foreach (var word in forbiddenWords)
        {
            current = ReplaceIgnoringCase(current, word, out var count);
            if (count > 0)
            {
                counts[word] = count;
                hasChanges = true;
            }
        }

        return new RedactionResult(current, counts, hasChanges);
    }

    private static string ReplaceIgnoringCase(string source, string word, out int count)
    {
        count = 0;
        if (string.IsNullOrWhiteSpace(word))
        {
            return source;
        }

        var builder = new StringBuilder(source.Length);
        var startIndex = 0;

        while (true)
        {
            var index = source.IndexOf(word, startIndex, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                break;
            }

            builder.Append(source, startIndex, index - startIndex);
            builder.Append("*******");
            count++;
            startIndex = index + word.Length;
        }

        if (count == 0)
        {
            return source;
        }

        builder.Append(source, startIndex, source.Length - startIndex);
        return builder.ToString();
    }

    private static void CopyFile(string sourcePath, string targetPath)
    {
        var targetDirectory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        File.Copy(sourcePath, targetPath, true);
    }

    private static void WriteTextFile(string path, string content, Encoding encoding, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content, encoding);
        cancellationToken.ThrowIfCancellationRequested();
    }

    private static string BuildOutputPath(string rootDirectory, DriveInfo drive, string relativePath)
    {
        var driveName = SanitizePathSegment(drive.Name.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var fullPath = Path.Combine(rootDirectory, driveName, relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return fullPath;
    }

    private static string SanitizePathSegment(string value)
    {
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidCharacter, '_');
        }

        return string.IsNullOrWhiteSpace(value) ? "Drive" : value;
    }

    private static long GetFileSizeSafe(string filePath)
    {
        try
        {
            return new FileInfo(filePath).Length;
        }
        catch
        {
            return 0;
        }
    }

    private sealed record RedactionResult(string Content, IReadOnlyDictionary<string, int> WordCounts, bool HasChanges);
}
