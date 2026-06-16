namespace aqua_api.Modules.Integrations.Application.Dtos
{
    public class ErpReceiptShipmentMovementDto
    {
        public long Id { get; set; }
        public string SourceSystem { get; set; } = string.Empty;
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
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public long? CageId { get; set; }
        public string? CageCode { get; set; }
        public string? CageName { get; set; }
        public long? ProjectCageId { get; set; }
        public long? StockId { get; set; }
        public string? StockCode { get; set; }
        public string? StockName { get; set; }
        public long? FishBatchId { get; set; }
        public string? BatchCode { get; set; }
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
    }
}
