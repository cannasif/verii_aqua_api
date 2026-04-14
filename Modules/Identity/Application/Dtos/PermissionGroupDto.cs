using System.ComponentModel.DataAnnotations;

namespace aqua_api.Modules.Identity.Application.Dtos
{
    public class PermissionGroupDto : BaseEntityDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystemAdmin { get; set; }
        public bool IsActive { get; set; }
        public List<long> PermissionDefinitionIds { get; set; } = new List<long>();
        public List<string> PermissionCodes { get; set; } = new List<string>();
    }

    public class CreatePermissionGroupDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsSystemAdmin { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public List<long> PermissionDefinitionIds { get; set; } = new List<long>();
    }

    public class UpdatePermissionGroupDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsSystemAdmin { get; set; }
        public bool? IsActive { get; set; }
    }

    public class SetPermissionGroupPermissionsDto
    {
        [Required]
        public List<long> PermissionDefinitionIds { get; set; } = new List<long>();
    }
}
