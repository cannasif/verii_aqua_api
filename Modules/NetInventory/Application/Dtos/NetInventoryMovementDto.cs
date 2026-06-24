using aqua_api.Modules.NetInventory.Domain.Enums;

namespace aqua_api.Modules.NetInventory.Application.Dtos;

public class NetInventoryMovementDto
{
    public long Id { get; set; }
    public string MovementNo { get; set; } = string.Empty;
    public NetType NetType { get; set; }
    public string NetTypeName { get; set; } = string.Empty;
    public NetInventoryMovementType MovementType { get; set; }
    public string MovementTypeName { get; set; } = string.Empty;
    public DateTime MovementDate { get; set; }
    public long? StockId { get; set; }
    public string? StockCode { get; set; }
    public string? StockName { get; set; }
    public long? ProjectId { get; set; }
    public string? ProjectCode { get; set; }
    public string? ProjectName { get; set; }
    public long? SourceWarehouseId { get; set; }
    public string? SourceWarehouseCode { get; set; }
    public string? SourceWarehouseName { get; set; }
    public long? TargetWarehouseId { get; set; }
    public string? TargetWarehouseCode { get; set; }
    public string? TargetWarehouseName { get; set; }
    public long? SourceProjectCageId { get; set; }
    public string? SourceCageCode { get; set; }
    public string? SourceCageName { get; set; }
    public long? TargetProjectCageId { get; set; }
    public string? TargetCageCode { get; set; }
    public string? TargetCageName { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}

public class CreateNetInventoryMovementDto
{
    public string? MovementNo { get; set; }
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
}

public class UpdateNetInventoryMovementDto : CreateNetInventoryMovementDto
{
}
