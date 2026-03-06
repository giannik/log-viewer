namespace OrchardCore.LogViewer.Models;

public class LogSearchQuery
{
    public string? FileName { get; set; }
    public LogLevel? Level { get; set; }
    public string? SearchTerm { get; set; }
    public bool NewestFirst { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PerPage { get; set; } = 25;
}
