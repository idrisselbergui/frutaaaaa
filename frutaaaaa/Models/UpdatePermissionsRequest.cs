public class UpdatePermissionsRequest
{
    public List<PermissionItem> Permissions { get; set; } = new List<PermissionItem>();
}

public class PermissionItem
{
    public string PageName { get; set; } = string.Empty;
    public bool Allowed { get; set; }
}
