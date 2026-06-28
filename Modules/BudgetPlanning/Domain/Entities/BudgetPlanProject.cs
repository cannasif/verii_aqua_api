using aqua_api.Modules.BudgetPlanning.Domain.Enums;

namespace aqua_api.Modules.BudgetPlanning.Domain.Entities;

public class BudgetPlanProject : BaseEntity
{
    public long BudgetPlanId { get; set; }
    public BudgetPlanSourceType SourceType { get; set; }
    public long? SourceProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
    public Project? SourceProject { get; set; }
    public ICollection<BudgetPlanFishBatch> FishBatches { get; set; } = new List<BudgetPlanFishBatch>();
}
