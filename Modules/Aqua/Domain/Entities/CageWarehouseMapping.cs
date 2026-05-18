namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class CageWarehouseMapping : BaseEntity
    {
        public long CageId { get; set; }
        public long WarehouseId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Note { get; set; }

        public Cage? Cage { get; set; }
        public aqua_api.Modules.Warehouse.Domain.Entities.Warehouse? Warehouse { get; set; }
    }
}
