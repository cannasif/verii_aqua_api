namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class OpeningImportFieldMappingDto
    {
        public string SourceColumn { get; set; } = string.Empty;
        public string TargetField { get; set; } = string.Empty;
    }

    public class OpeningImportSheetPayloadDto
    {
        public string SheetName { get; set; } = string.Empty;
        public List<Dictionary<string, string?>> Rows { get; set; } = new();
        public List<OpeningImportFieldMappingDto> Mappings { get; set; } = new();
    }

    public class OpeningImportPreviewRequestDto
    {
        public string? FileName { get; set; }
        public string? SourceSystem { get; set; }
        public List<OpeningImportSheetPayloadDto> Sheets { get; set; } = new();
    }

    public class OpeningImportRowResultDto
    {
        public long RowId { get; set; }
        public string SheetName { get; set; } = string.Empty;
        public int RowNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Messages { get; set; } = new();
        public Dictionary<string, string?> RawData { get; set; } = new();
        public Dictionary<string, string?> NormalizedData { get; set; } = new();
    }

    public class OpeningImportSummaryDto
    {
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int WarningRows { get; set; }
        public int ErrorRows { get; set; }
    }

    public class OpeningImportPreviewResponseDto
    {
        public long JobId { get; set; }
        public string Status { get; set; } = string.Empty;
        public OpeningImportSummaryDto Summary { get; set; } = new();
        public List<OpeningImportRowResultDto> Rows { get; set; } = new();
    }

    public class OpeningImportCommitResultDto
    {
        public long JobId { get; set; }
        public int CreatedProjects { get; set; }
        public int CreatedCages { get; set; }
        public int CreatedProjectCages { get; set; }
        public int CreatedFishBatches { get; set; }
        public int CreatedGoodsReceipts { get; set; }
        public int CreatedFeedingHeaders { get; set; }
        public int CreatedMortalityHeaders { get; set; }
        public int CreatedShipmentHeaders { get; set; }
        public int CreatedGoodsReceiptLines { get; set; }
        public int CreatedFeedingLines { get; set; }
        public int CreatedFeedingDistributions { get; set; }
        public int CreatedMortalityLines { get; set; }
        public int CreatedShipmentLines { get; set; }
        public int AppliedCageRows { get; set; }
        public int AppliedWarehouseRows { get; set; }
        public int SkippedRows { get; set; }
    }
}
