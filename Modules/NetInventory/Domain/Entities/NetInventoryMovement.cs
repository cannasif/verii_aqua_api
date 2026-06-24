using aqua_api.Modules.NetInventory.Domain.Enums;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.NetInventory.Domain.Entities;

public class NetInventoryMovement : BaseEntity
{
    public string MovementNo { get; set; } = string.Empty;
    public NetType NetType { get; set; }
    public NetInventoryMovementType MovementType { get; set; }
    public DateTime MovementDate { get; set; }
    public long? StockId { get; set; }
    public long? ProjectId { get; set; }
    public long? SourceWarehouseId { get; set; }
    public long? TargetWarehouseId { get; set; }
    public long? SourceProjectCageId { get; set; }
    public long? TargetProjectCageId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }

    public StockEntity? Stock { get; set; }
    public Project? Project { get; set; }
    public WarehouseEntity? SourceWarehouse { get; set; }
    public WarehouseEntity? TargetWarehouse { get; set; }
    public ProjectCage? SourceProjectCage { get; set; }
    public ProjectCage? TargetProjectCage { get; set; }
}
