
namespace aqua_api.Modules.Identity.Domain.Entities
{
    public class UserPermissionGroup : BaseEntity
    {
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        public long PermissionGroupId { get; set; }
        public PermissionGroup PermissionGroup { get; set; } = null!;
    }
}
