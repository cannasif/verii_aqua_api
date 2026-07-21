namespace aqua_api.Modules.FishGrowths.Application.Dtos;

public class FishGrowthDto
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public string? ProjectCode { get; set; }
    public string? ProjectName { get; set; }
    public long ProjectCageId { get; set; }
    public string? CageCode { get; set; }
    public string? CageName { get; set; }
    public long FishBatchId { get; set; }
    public string? BatchCode { get; set; }
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
}

public class CreateFishGrowthDto
{
    public long ProjectId { get; set; }
    public long ProjectCageId { get; set; }
    public long FishBatchId { get; set; }
    public DateTime GrowthDate { get; set; }
    public decimal GrowthGram { get; set; }
    public string? Description { get; set; }
}
