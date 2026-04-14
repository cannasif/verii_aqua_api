using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class FeedingDistributionDto
    {
        public long Id { get; set; }
        public long FeedingLineId { get; set; }
        public long FishBatchId { get; set; }
        public long ProjectCageId { get; set; }
        public decimal FeedGram { get; set; }
    }

    public class CreateFeedingDistributionDto
    {
        public long FeedingLineId { get; set; }
        public long FishBatchId { get; set; }
        public long ProjectCageId { get; set; }
        public decimal FeedGram { get; set; }
    }

    public class UpdateFeedingDistributionDto : CreateFeedingDistributionDto
    {
    }
}
