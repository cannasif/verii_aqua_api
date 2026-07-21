namespace aqua_api.Modules.FishGrowths.Domain.Entities;

public class FishGrowth : BaseEntity
{
    public long ProjectId { get; set; }
    public long ProjectCageId { get; set; }
    public long FishBatchId { get; set; }
    public DateTime GrowthDate { get; set; }
    public int GrowthYear { get; set; }
    public byte GrowthMonth { get; set; }
    public int FishCount { get; set; }
    public decimal PreviousAverageGram { get; set; }
    public decimal GrowthGram { get; set; }
    public decimal NewAverageGram { get; set; }
    public decimal PreviousBiomassGram { get; set; }
    public decimal NewBiomassGram { get; set; }
    public string? Description { get; set; }

    public Project? Project { get; set; }
    public ProjectCage? ProjectCage { get; set; }
    public FishBatch? FishBatch { get; set; }
}
