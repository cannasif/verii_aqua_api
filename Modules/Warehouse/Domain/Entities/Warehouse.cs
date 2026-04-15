namespace aqua_api.Modules.Warehouse.Domain.Entities
{
    public class Warehouse : BaseEntity
    {
        public short ErpWarehouseCode { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? CustomerCode { get; set; }
        public int BranchCode { get; set; }
        public bool IsLocked { get; set; }
        public bool AllowNegativeBalance { get; set; }
        public DateTime? LastSyncedAt { get; set; }
    }
}
