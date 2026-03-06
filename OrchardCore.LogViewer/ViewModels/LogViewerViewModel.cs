using OrchardCore.LogViewer.Models;

namespace OrchardCore.LogViewer.ViewModels;

public class LogViewerViewModel
{
    public IEnumerable<LogFileInfo> LogFiles { get; set; } = [];
    public LogViewerResult Result { get; set; } = new();
    public LogSearchQuery Query { get; set; } = new();
}
