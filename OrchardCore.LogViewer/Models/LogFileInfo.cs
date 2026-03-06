namespace OrchardCore.LogViewer.Models;

public class LogFileInfo
{
    public string Name { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime LastModified { get; set; }
    public string FullPath { get; set; } = string.Empty;

    public string FormattedSize
    {
        get
        {
            if (SizeBytes >= 1_073_741_824)
                return $"{SizeBytes / 1_073_741_824.0:F1} GB";
            if (SizeBytes >= 1_048_576)
                return $"{SizeBytes / 1_048_576.0:F1} MB";
            if (SizeBytes >= 1_024)
                return $"{SizeBytes / 1_024.0:F1} KB";
            return $"{SizeBytes} B";
        }
    }
}
