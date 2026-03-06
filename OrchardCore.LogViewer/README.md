# OrchardCore.LogViewer

A fast, beautiful web-based log viewer for [Orchard Core](https://orchardcore.net/) applications. Inspired by the Laravel [opcodesio/log-viewer](https://github.com/opcodesio/log-viewer) project.

## Features

- 📂 Browse all log files from `App_Data/logs/`
- 🎨 Color-coded log level badges (Trace, Debug, Info, Warn, Error, Fatal)
- 🔍 Full-text search with optional regular expression support (`/pattern/`)
- 📊 Level filter badges with live entry counts
- ⚡ Dynamic interactions via **HTMX** — no JavaScript framework required
- 🌙 Dark mode (manual toggle + respects `prefers-color-scheme`)
- 📄 Server-side pagination with configurable page sizes
- ⬇️ Log file download
- 🔒 Orchard Core permission-gated (`ViewLogViewer`)
- 📌 Bookmarkable URLs — browser history is updated on every filter change

## Installation

1. Add the project to your Orchard Core solution:
   ```xml
   <ProjectReference Include="../OrchardCore.LogViewer/OrchardCore.LogViewer.csproj" />
   ```
2. Enable the module in the Orchard Core admin under **Configuration → Features → Log Viewer → Enable**.
3. Grant the `ViewLogViewer` permission to the appropriate roles under **Security → Roles**.

## Accessing the Log Viewer

Once the module is enabled, open your browser and navigate to:

```
https://<your-site>/Admin/LogViewer
```

> **Example:** `https://localhost:5001/Admin/LogViewer`

You can also reach it from the Orchard Core admin sidebar via **Configuration → Log Viewer**.

### Available endpoints

| URL | Description |
|-----|-------------|
| `/Admin/LogViewer` | Main log viewer page |
| `/Admin/LogViewer/Entries?fileName=<name>` | HTMX partial — entries for a specific file |
| `/Admin/LogViewer/Download/<fileName>` | Download a raw log file |

## Log Format

The module parses NLog's pipe-delimited format used by Orchard Core:

```
Timestamp|Level|Logger|TenantName|Message
```

Example:
```
2025-11-16 10:23:07.1234|ERROR|OrchardCore.Modules.ModularBackgroundService|Default|An error occurred.
2025-11-16 10:23:08.5678|INFO|Microsoft.Hosting.Lifetime|Default|Application started.
```

Multi-line entries (stack traces) are supported and displayed in collapsible rows.

## Usage

Navigate directly to **`/Admin/LogViewer`** in your browser, or use the admin sidebar: **Configuration → Log Viewer**.

### Search

- Plain text: case-insensitive substring match across message, logger, tenant, and exception fields
- Regular expression: wrap pattern in `/` slashes, e.g. `/System\.(IO|Net)/`

### Keyboard Shortcuts

| Key      | Action                   |
|----------|--------------------------|
| `/`      | Focus the search box     |
| `Escape` | Clear the search box     |
| `Enter`  | Expand/collapse exception row (when focused) |

## Architecture

| Layer      | Technology                        |
|------------|-----------------------------------|
| Backend    | ASP.NET Core 10 / Orchard Core    |
| Views      | Razor `.cshtml`                   |
| Dynamic UI | HTMX 2.x                         |
| Styles     | Plain CSS (`wwwroot/css/`)        |
| Scripts    | Vanilla JS (`wwwroot/js/`)        |

No npm build step is required. All static assets are served directly.

## Security

All routes are protected by the `ViewLogViewer` Orchard Core permission. Only users with this permission can access log data.

Log files are opened with `FileShare.ReadWrite` to avoid interfering with NLog which may be actively writing to the file.

## Future Considerations

The following features are **not yet implemented** but are planned for future versions:

- **Tenant filtering** — the NLog format includes a `TenantName` field; a future version could add a tenant dropdown to filter by tenant
- **Log file delete from UI** — currently only download is supported
- **Real-time log tailing** via SignalR
- **Configurable log path** via Orchard Core admin settings panel
- **Caching / indexing** for large files to speed up repeated searches (inspired by the original project's `LogIndex` approach)

## License

MIT
