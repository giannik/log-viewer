using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.LogViewer.Navigation;
using OrchardCore.LogViewer.Permissions;
using OrchardCore.LogViewer.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.LogViewer;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ILogViewerService, LogViewerService>();
        services.AddScoped<IPermissionProvider, LogViewerPermissions>();
        services.AddScoped<INavigationProvider, AdminMenu>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "LogViewer",
            areaName: "OrchardCore.LogViewer",
            pattern: "Admin/LogViewer/{action}/{fileName?}",
            defaults: new { controller = "Admin", action = "Index" }
        );
    }
}
