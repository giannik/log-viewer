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

### Real-time email alerts on log criteria

Sending email notifications when log entries match given criteria (e.g. ERROR or FATAL level) can be achieved through several complementary approaches:

#### Option 1 — NLog `MailTarget` (built-in, zero extra dependencies)

NLog ships a [`MailTarget`](https://nlog-project.org/config/?tab=targets&search=mailtarget) that can be configured entirely in `NLog.config`. Example rule that e-mails on ERROR and above:

```xml
<targets>
  <target xsi:type="Mail"
          name="mail"
          smtpServer="smtp.example.com"
          smtpPort="587"
          enableSsl="true"
          smtpAuthentication="Basic"
          smtpUserName="${env:SMTP_USER}"
          smtpPassword="${env:SMTP_PASS}"
          from="noreply@example.com"
          to="ops@example.com"
          subject="[${machinename}] ${level:uppercase=true}: ${logger}"
          body="${longdate}|${level}|${logger}|${message}${newline}${exception:format=tostring}" />
</targets>
<rules>
  <logger name="*" minlevel="Error" writeTo="mail" />
</rules>
```

> **Tip:** Wrap with a `<target xsi:type="BufferingWrapper">` to batch multiple entries into one email and avoid alert storms.

This is the lowest-friction option — no code changes to the module are required.

#### Option 2 — NLog `WebServiceTarget` / custom `Target` in Orchard Core

Implement a custom `NLog.Targets.Target` subclass that enqueues matched entries into an `IBackgroundTask`, then uses Orchard Core's `ISmtpService` (from `OrchardCore.Email`) to send templated emails. This approach lets you control templates, rate-limiting, and per-tenant routing entirely in C#.

Reference: [`NLog.Targets.Target` base class](https://github.com/NLog/NLog/blob/master/src/NLog/Targets/Target.cs)

#### Option 3 — Seq + alerting rules (external, SaaS or self-hosted)

[Seq](https://datalust.co/seq) is a structured log server with a first-class NLog sink ([`NLog.Targets.Seq`](https://github.com/datalust/nlog-targets-seq)). It provides a web UI for alert rules, email/Slack/Teams notifications, and dashboards — without any custom code.

#### Option 4 — Serilog `Email` sink (alternative logging pipeline)

If the project were to switch to Serilog, [`Serilog.Sinks.Email`](https://github.com/serilog/serilog-sinks-email) provides a similar capability with `minimumLevel` filtering and SMTP configuration.

> **Recommendation for a future module version:** expose a lightweight settings page (using Orchard Core's `ISiteService`) where administrators can configure SMTP credentials, a minimum log level threshold, and a list of recipient addresses. Under the hood this would register a custom `NLog.Targets.Target` that delegates to Orchard Core's `ISmtpService`, keeping everything inside the Orchard Core ecosystem.

## License

MIT
