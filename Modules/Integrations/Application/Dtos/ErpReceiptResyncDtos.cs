namespace aqua_api.Modules.Integrations.Application.Dtos
{
    public class ErpReceiptResyncPreviewDto
    {
        public string DocumentNo { get; set; } = string.Empty;
        public string InOutCode { get; set; } = "G";
        public string OperationType { get; set; } = string.Empty;
        public int SourceMovementCount { get; set; }
        public int GoodsReceiptLineCount { get; set; }
        public int FishBatchCount { get; set; }
        public bool CanResync { get; set; }
        public bool RequiresErpReversal { get; set; }
        public List<string> BlockingReasons { get; set; } = new();
        public List<ErpReceiptResyncImpactDto> Impacts { get; set; } = new();
    }

    public class ErpReceiptResyncImpactDto
    {
        public string OperationType { get; set; } = string.Empty;
        public long HeaderId { get; set; }
        public string? DocumentNo { get; set; }
        public DateTime OperationDate { get; set; }
        public long? ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public long? ProjectCageId { get; set; }
        public string? CageCode { get; set; }
        public long? FishBatchId { get; set; }
        public string? BatchCode { get; set; }
        public int? FishCount { get; set; }
        public decimal? BiomassKg { get; set; }
        public decimal? FeedKg { get; set; }
        public bool IsErpIntegrated { get; set; }
        public string? ErpReferenceNumber { get; set; }
    }

    public class ErpReceiptResyncRequestDto
    {
        public string DocumentNo { get; set; } = string.Empty;
        public string InOutCode { get; set; } = "G";
        public string OperationType { get; set; } = string.Empty;
        public string ConfirmationDocumentNo { get; set; } = string.Empty;
    }

    public class ErpReceiptResyncResultDto
    {
        public string DocumentNo { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public int CancelledSourceMovementCount { get; set; }
        public int ReversedLedgerMovementCount { get; set; }
        public int ReprocessedSourceMovementCount { get; set; }
    }
}
