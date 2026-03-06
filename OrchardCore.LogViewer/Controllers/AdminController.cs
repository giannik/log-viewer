using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.LogViewer.Models;
using OrchardCore.LogViewer.Permissions;
using OrchardCore.LogViewer.Services;
using OrchardCore.LogViewer.ViewModels;

namespace OrchardCore.LogViewer.Controllers;

[Area("OrchardCore.LogViewer")]
[Route("Admin/LogViewer")]
public class AdminController : Controller
{
    private readonly ILogViewerService _logViewerService;
    private readonly IAuthorizationService _authorizationService;

    public AdminController(
        ILogViewerService logViewerService,
        IAuthorizationService authorizationService)
    {
        _logViewerService = logViewerService;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Index(LogSearchQuery query)
    {
        if (!await _authorizationService.AuthorizeAsync(User, LogViewerPermissions.ViewLogViewer))
            return Forbid();

        var files = _logViewerService.GetLogFiles().ToList();
        var result = _logViewerService.GetEntries(query);

        var viewModel = new LogViewerViewModel
        {
            LogFiles = files,
            Result = result,
            Query = query,
        };

        return View(viewModel);
    }

    [HttpGet("Entries")]
    public async Task<IActionResult> Entries(LogSearchQuery query)
    {
        if (!await _authorizationService.AuthorizeAsync(User, LogViewerPermissions.ViewLogViewer))
            return Forbid();

        var result = _logViewerService.GetEntries(query);

        var viewModel = new LogViewerViewModel
        {
            LogFiles = _logViewerService.GetLogFiles().ToList(),
            Result = result,
            Query = query,
        };

        return PartialView("_LogEntries", viewModel);
    }

    [HttpGet("Download/{fileName}")]
    public async Task<IActionResult> Download(string fileName)
    {
        if (!await _authorizationService.AuthorizeAsync(User, LogViewerPermissions.ViewLogViewer))
            return Forbid();

        var stream = _logViewerService.GetFileStream(fileName);
        if (stream == null)
            return NotFound();

        return File(stream, "text/plain", fileName);
    }
}
