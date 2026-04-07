using System.Diagnostics;

namespace Practice;

internal static class FileSearchService
{
    public static SearchResult SearchSequential(string rootDirectory, string extension, bool includeSubdirectories)
    {
        var normalizedExtension = NormalizeExtension(extension);
        var directories = GetDirectoriesToSearch(rootDirectory, includeSubdirectories);

        var stopwatch = Stopwatch.StartNew();
        var count = 0;

        foreach (var directory in directories)
        {
            foreach (var file in GetFilesSafe(directory))
            {
                if (HasMatchingExtension(file, normalizedExtension))
                {
                    count++;
                }
            }
        }

        stopwatch.Stop();
        return new SearchResult("Послідовно", count, stopwatch.Elapsed);
    }

    public static SearchResult SearchParallel(string rootDirectory, string extension, bool includeSubdirectories)
    {
        var normalizedExtension = NormalizeExtension(extension);
        var directories = GetDirectoriesToSearch(rootDirectory, includeSubdirectories);

        var stopwatch = Stopwatch.StartNew();
        var count = 0;

        Parallel.ForEach(directories, directory =>
        {
            var localCount = 0;

            foreach (var file in GetFilesSafe(directory))
            {
                if (HasMatchingExtension(file, normalizedExtension))
                {
                    localCount++;
                }
            }

            if (localCount > 0)
            {
                Interlocked.Add(ref count, localCount);
            }
        });

        stopwatch.Stop();
        return new SearchResult("Паралельно", count, stopwatch.Elapsed);
    }

    private static List<string> GetDirectoriesToSearch(string rootDirectory, bool includeSubdirectories)
    {
        var directories = new List<string> { rootDirectory };

        if (!includeSubdirectories)
        {
            return directories;
        }

        var stack = new Stack<string>();
        stack.Push(rootDirectory);

        while (stack.Count > 0)
        {
            var currentDirectory = stack.Pop();

            foreach (var subdirectory in GetDirectoriesSafe(currentDirectory))
            {
                directories.Add(subdirectory);
                stack.Push(subdirectory);
            }
        }

        return directories;
    }

    private static string[] GetDirectoriesSafe(string directoryPath)
    {
        try
        {
            return Directory.GetDirectories(directoryPath);
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static string[] GetFilesSafe(string directoryPath)
    {
        try
        {
            return Directory.GetFiles(directoryPath);
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static bool HasMatchingExtension(string filePath, string normalizedExtension)
    {
        return string.Equals(Path.GetExtension(filePath), normalizedExtension, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeExtension(string extension)
    {
        var value = extension.Trim();
        value = value.TrimStart('*');
        value = value.TrimStart('.');

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Вкажіть розширення файлу.", nameof(extension));
        }

        return "." + value;
    }
}

internal sealed record SearchResult(string Mode, int Count, TimeSpan Elapsed);