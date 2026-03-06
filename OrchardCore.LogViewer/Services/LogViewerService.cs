using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using OrchardCore.LogViewer.Models;

namespace OrchardCore.LogViewer.Services;

public class LogViewerService : ILogViewerService
{
    private readonly IHostEnvironment _env;
    private readonly NLogParser _parser = new();

    public LogViewerService(IHostEnvironment env)
    {
        _env = env;
    }

    private string LogDirectory =>
        Path.Combine(_env.ContentRootPath, "App_Data", "logs");

    public IEnumerable<LogFileInfo> GetLogFiles()
    {
        var dir = LogDirectory;
        if (!Directory.Exists(dir))
            return [];

        return Directory
            .GetFiles(dir, "*.log", SearchOption.TopDirectoryOnly)
            .Select(f =>
            {
                var info = new FileInfo(f);
                return new LogFileInfo
                {
                    Name = info.Name,
                    FullPath = info.FullName,
                    SizeBytes = info.Length,
                    LastModified = info.LastWriteTimeUtc,
                };
            })
            .OrderByDescending(f => f.LastModified);
    }

    public LogViewerResult GetEntries(LogSearchQuery query)
    {
        var files = GetLogFiles().ToList();

        // Default to the most recently modified log file when none is specified
        var fileName = query.FileName ?? files.FirstOrDefault()?.Name;

        if (fileName == null)
            return Empty(query);

        var fileInfo = files.FirstOrDefault(f => f.Name == fileName);
        if (fileInfo == null)
            return Empty(query);

        var allEntries = ParseFile(fileInfo.FullPath).ToList();

        // Build level counts from all entries (before filtering)
        var levelCounts = allEntries
            .GroupBy(e => e.Level)
            .ToDictionary(g => g.Key, g => g.Count());

        // Apply filters
        IEnumerable<LogEntry> filtered = allEntries;

        if (query.Level.HasValue)
            filtered = filtered.Where(e => e.Level == query.Level.Value);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            filtered = ApplySearch(filtered, query.SearchTerm);

        // Sort
        filtered = query.NewestFirst
            ? filtered.OrderByDescending(e => e.Timestamp).ThenByDescending(e => e.Index)
            : filtered.OrderBy(e => e.Timestamp).ThenBy(e => e.Index);

        var totalEntries = filtered.Count();
        var perPage = query.PerPage > 0 ? query.PerPage : 25;
        var totalPages = (int)Math.Ceiling(totalEntries / (double)perPage);
        var currentPage = Math.Max(1, Math.Min(query.Page, totalPages == 0 ? 1 : totalPages));

        var paged = filtered
            .Skip((currentPage - 1) * perPage)
            .Take(perPage)
            .ToList();

        return new LogViewerResult
        {
            Entries = paged,
            TotalEntries = totalEntries,
            LevelCounts = levelCounts,
            CurrentPage = currentPage,
            PerPage = perPage,
            TotalPages = totalPages,
        };
    }

    public Stream? GetFileStream(string fileName)
    {
        var files = GetLogFiles().ToList();
        var fileInfo = files.FirstOrDefault(f => f.Name == fileName);
        if (fileInfo == null)
            return null;

        return new FileStream(
            fileInfo.FullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite);
    }

    private IEnumerable<LogEntry> ParseFile(string fullPath)
    {
        using var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        foreach (var entry in _parser.Parse(reader))
            yield return entry;
    }

    private static IEnumerable<LogEntry> ApplySearch(
        IEnumerable<LogEntry> entries,
        string searchTerm)
    {
        // Detect regex: /pattern/
        if (searchTerm.StartsWith('/') && searchTerm.EndsWith('/') && searchTerm.Length > 2)
        {
            var pattern = searchTerm[1..^1];
            Regex? regex = null;
            try { regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled); }
            catch { /* invalid regex — fall back to literal search */ }

            if (regex != null)
            {
                return entries.Where(e =>
                    regex.IsMatch(e.Message) ||
                    regex.IsMatch(e.Logger) ||
                    regex.IsMatch(e.TenantName) ||
                    (e.Exception != null && regex.IsMatch(e.Exception)));
            }
        }

        // Plain case-insensitive contains
        return entries.Where(e =>
            e.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            e.Logger.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            e.TenantName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            (e.Exception != null && e.Exception.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
    }

    private static LogViewerResult Empty(LogSearchQuery query) => new()
    {
        Entries = [],
        TotalEntries = 0,
        LevelCounts = [],
        CurrentPage = 1,
        PerPage = query.PerPage > 0 ? query.PerPage : 25,
        TotalPages = 0,
    };
}
