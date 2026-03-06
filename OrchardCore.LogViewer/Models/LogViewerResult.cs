namespace OrchardCore.LogViewer.Models;

public class LogViewerResult
{
    public List<LogEntry> Entries { get; set; } = [];
    public int TotalEntries { get; set; }
    public Dictionary<LogLevel, int> LevelCounts { get; set; } = [];
    public int CurrentPage { get; set; }
    public int PerPage { get; set; }
    public int TotalPages { get; set; }
}
