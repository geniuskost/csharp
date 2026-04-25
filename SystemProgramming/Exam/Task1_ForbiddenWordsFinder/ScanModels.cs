using System.Text;

namespace ForbiddenWordsFinder;

internal sealed record FoundFileInfo(
    string SourcePath,
    string OriginalCopyPath,
    string RedactedCopyPath,
    long SizeBytes,
    int ReplacementCount);

internal sealed record ScanProgress(
    string Stage,
    string CurrentDrive,
    int DriveIndex,
    int DriveCount,
    int FilesInDrive,
    int FilesProcessedInDrive,
    int MatchedFiles,
    string? CurrentFilePath);

internal sealed record ScanSummary(
    DateTime StartedAt,
    DateTime FinishedAt,
    IReadOnlyList<string> ForbiddenWords,
    IReadOnlyList<FoundFileInfo> Matches,
    IReadOnlyDictionary<string, int> WordPopularity,
    int TotalScannedFiles,
    int MatchedFiles,
    int TotalReplacementCount,
    string OutputDirectory,
    string ReportPath);

internal sealed record TextDocument(string Content, Encoding Encoding);
