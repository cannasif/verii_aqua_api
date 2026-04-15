using System;
using System.Collections.Generic;

namespace aqua_api.Modules.Aqua.Domain.Entities
{

    public class Project : BaseEntity
    {
        public string ProjectCode { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public string? Note { get; set; }

        public ICollection<ProjectCage> ProjectCages { get; set; } = new List<ProjectCage>();
        public ICollection<FishBatch> FishBatches { get; set; } = new List<FishBatch>();
        public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
        public ICollection<Feeding> Feedings { get; set; } = new List<Feeding>();
        public ICollection<Mortality> Mortalities { get; set; } = new List<Mortality>();
        public ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
        public ICollection<Weighing> Weighings { get; set; } = new List<Weighing>();
        public ICollection<StockConvert> StockConverts { get; set; } = new List<StockConvert>();
        public ICollection<DailyWeather> DailyWeathers { get; set; } = new List<DailyWeather>();
        public ICollection<NetOperation> NetOperations { get; set; } = new List<NetOperation>();
        public ICollection<FishHealthEvent> FishHealthEvents { get; set; } = new List<FishHealthEvent>();
        public ICollection<FishTreatment> FishTreatments { get; set; } = new List<FishTreatment>();
        public ICollection<FishLabSample> FishLabSamples { get; set; } = new List<FishLabSample>();
        public ICollection<WelfareAssessment> WelfareAssessments { get; set; } = new List<WelfareAssessment>();
        public ICollection<ComplianceAudit> ComplianceAudits { get; set; } = new List<ComplianceAudit>();
    }
}
