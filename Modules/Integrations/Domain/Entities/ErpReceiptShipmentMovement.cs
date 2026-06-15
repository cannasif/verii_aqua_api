using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Integrations.Domain.Entities
{
    public class ErpReceiptShipmentMovement : BaseEntity
    {
        public string SourceSystem { get; set; } = "Netsis";
        public string SourceMovementKey { get; set; } = string.Empty;

        public DateTime MovementDate { get; set; }
        public string? DocumentNo { get; set; }
        public short? ErpWarehouseCode { get; set; }
        public string? ErpProjectCode { get; set; }
        public string ErpStockCode { get; set; } = string.Empty;
        public string? ErpStockName { get; set; }
        public decimal Quantity { get; set; }
        public string MovementKind { get; set; } = string.Empty;
        public string InOutCode { get; set; } = string.Empty;
        public string? StockGroupCode { get; set; }
        public string OperationType { get; set; } = string.Empty;

        public long? ProjectId { get; set; }
        public long? CageId { get; set; }
        public long? ProjectCageId { get; set; }
        public long? StockId { get; set; }
        public long? FishBatchId { get; set; }

        public long? GoodsReceiptId { get; set; }
        public long? GoodsReceiptLineId { get; set; }
        public long? ShipmentId { get; set; }
        public long? ShipmentLineId { get; set; }
        public long? BatchMovementId { get; set; }

        public bool IsMatched { get; set; }
        public bool IsProcessed { get; set; }
        public int ProcessingAttemptCount { get; set; }

        public DateTime LastSyncedAt { get; set; }
        public DateTime? MatchedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public string? MatchError { get; set; }
        public string? ProcessError { get; set; }

        public Project? Project { get; set; }
        public Cage? Cage { get; set; }
        public ProjectCage? ProjectCage { get; set; }
        public StockEntity? Stock { get; set; }
        public FishBatch? FishBatch { get; set; }
        public GoodsReceipt? GoodsReceipt { get; set; }
        public GoodsReceiptLine? GoodsReceiptLine { get; set; }
        public Shipment? Shipment { get; set; }
        public ShipmentLine? ShipmentLine { get; set; }
        public BatchMovement? BatchMovement { get; set; }
    }
}
