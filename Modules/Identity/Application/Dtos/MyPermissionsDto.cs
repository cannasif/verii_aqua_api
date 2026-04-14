namespace aqua_api.Modules.Identity.Application.Dtos
{
    public class MyPermissionsDto
    {
        public long UserId { get; set; }
        public string RoleTitle { get; set; } = string.Empty;
        public bool IsSystemAdmin { get; set; }
        public List<string> PermissionGroups { get; set; } = new List<string>();
        public List<string> PermissionCodes { get; set; } = new List<string>();
    }
}
