using Microsoft.Extensions.Localization;
using OrchardCore.LogViewer.Permissions;
using OrchardCore.Navigation;

namespace OrchardCore.LogViewer.Navigation;

public class AdminMenu : INavigationProvider
{
    private readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            return ValueTask.CompletedTask;

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Log Viewer"], S["Log Viewer"].PrefixPosition("9"), logViewer => logViewer
                    .Action("Index", "Admin", new { area = "OrchardCore.LogViewer" })
                    .Permission(LogViewerPermissions.ViewLogViewer)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
