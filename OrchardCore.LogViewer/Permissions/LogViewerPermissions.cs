using OrchardCore.Security.Permissions;

namespace OrchardCore.LogViewer.Permissions;

public class LogViewerPermissions : IPermissionProvider
{
    public static readonly Permission ViewLogViewer = new(
        "ViewLogViewer",
        "View Log Viewer");

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult<IEnumerable<Permission>>([ViewLogViewer]);
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        return
        [
            new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = [ViewLogViewer],
            },
        ];
    }
}
