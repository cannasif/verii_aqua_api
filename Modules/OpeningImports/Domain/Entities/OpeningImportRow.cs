namespace aqua_api.Modules.OpeningImports.Domain.Entities
{
    public class OpeningImportRow : BaseEntity
    {
        public long OpeningImportJobId { get; set; }
        public string SheetName { get; set; } = string.Empty;
        public int RowNumber { get; set; }
        public OpeningImportRowStatus Status { get; set; } = OpeningImportRowStatus.Pending;
        public string RawDataJson { get; set; } = string.Empty;
        public string? NormalizedDataJson { get; set; }
        public string? MessagesJson { get; set; }

        public OpeningImportJob? OpeningImportJob { get; set; }
    }
}
