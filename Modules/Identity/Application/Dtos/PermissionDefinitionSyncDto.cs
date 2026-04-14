using System.ComponentModel.DataAnnotations;

namespace aqua_api.Modules.Identity.Application.Dtos
{
    public class SyncPermissionDefinitionsDto
    {
        [Required]
        public List<SyncPermissionDefinitionItemDto> Items { get; set; } = new();

        public bool ReactivateSoftDeleted { get; set; } = true;

        public bool UpdateExistingNames { get; set; } = false;

        public bool UpdateExistingDescriptions { get; set; } = false;

        public bool UpdateExistingIsActive { get; set; } = false;
    }

    public class SyncPermissionDefinitionItemDto
    {
        [Required]
        [StringLength(120)]
        public string Code { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class PermissionDefinitionSyncResultDto
    {
        public int CreatedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int ReactivatedCount { get; set; }
        public int TotalProcessed { get; set; }
    }
}
