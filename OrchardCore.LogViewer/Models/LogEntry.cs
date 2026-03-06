namespace OrchardCore.LogViewer.Models;

public class LogEntry
{
    public int Index { get; set; }
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Logger { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
}
