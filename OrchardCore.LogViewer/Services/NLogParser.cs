using System.Globalization;
using System.Text.RegularExpressions;
using OrchardCore.LogViewer.Models;

namespace OrchardCore.LogViewer.Services;

public partial class NLogParser
{
    [GeneratedRegex(
        @"^(?<timestamp>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d+)\|(?<level>[A-Z]+)\|(?<logger>[^|]*)\|(?<tenant>[^|]*)\|(?<message>.*)$",
        RegexOptions.Compiled)]
    private static partial Regex LogLineRegex();

    public IEnumerable<LogEntry> Parse(TextReader reader)
    {
        LogEntry? current = null;
        var index = 0;

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var match = LogLineRegex().Match(line);
            if (match.Success)
            {
                if (current != null)
                    yield return current;

                current = new LogEntry
                {
                    Index = ++index,
                    Timestamp = DateTime.ParseExact(
                        match.Groups["timestamp"].Value,
                        ["yyyy-MM-dd HH:mm:ss.ffff", "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss.ff", "yyyy-MM-dd HH:mm:ss.f", "yyyy-MM-dd HH:mm:ss.fffff", "yyyy-MM-dd HH:mm:ss.ffffff"],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None),
                    Level = ParseLevel(match.Groups["level"].Value),
                    Logger = match.Groups["logger"].Value,
                    TenantName = match.Groups["tenant"].Value,
                    Message = match.Groups["message"].Value,
                };
            }
            else if (current != null)
            {
                // Continuation line (stack trace / exception detail)
                current.Exception = current.Exception == null
                    ? line
                    : current.Exception + Environment.NewLine + line;
            }
        }

        if (current != null)
            yield return current;
    }

    private static LogLevel ParseLevel(string level) => level.ToUpperInvariant() switch
    {
        "TRACE" => LogLevel.Trace,
        "DEBUG" => LogLevel.Debug,
        "INFO" or "INFORMATION" => LogLevel.Info,
        "WARN" or "WARNING" => LogLevel.Warn,
        "ERROR" => LogLevel.Error,
        "FATAL" or "CRITICAL" => LogLevel.Fatal,
        _ => LogLevel.Info,
    };
}
