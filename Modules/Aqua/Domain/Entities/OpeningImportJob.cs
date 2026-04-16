using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class OpeningImportJob : BaseEntity
    {
        public string FileName { get; set; } = string.Empty;
        public string? SourceSystem { get; set; }
        public OpeningImportJobStatus Status { get; set; } = OpeningImportJobStatus.Draft;
        public string? MappingsJson { get; set; }
        public string? SummaryJson { get; set; }
        public DateTime? PreviewedAt { get; set; }
        public DateTime? AppliedAt { get; set; }

        public ICollection<OpeningImportRow> Rows { get; set; } = new List<OpeningImportRow>();
    }
}
