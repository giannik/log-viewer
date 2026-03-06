using OrchardCore.LogViewer.Models;

namespace OrchardCore.LogViewer.Services;

public interface ILogViewerService
{
    IEnumerable<LogFileInfo> GetLogFiles();
    LogViewerResult GetEntries(LogSearchQuery query);
    Stream? GetFileStream(string fileName);
}
