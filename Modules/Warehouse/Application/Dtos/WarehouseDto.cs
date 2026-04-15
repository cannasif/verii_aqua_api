namespace aqua_api.Modules.Warehouse.Application.Dtos
{
    public class WarehouseDto
    {
        public long Id { get; set; }
        public short ErpWarehouseCode { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? CustomerCode { get; set; }
        public int BranchCode { get; set; }
        public bool IsLocked { get; set; }
        public bool AllowNegativeBalance { get; set; }
        public DateTime? LastSyncedAt { get; set; }
    }
}
