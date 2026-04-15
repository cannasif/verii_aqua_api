using System;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class FeedingLineDto
    {
        public long Id { get; set; }
        public long FeedingId { get; set; }
        public long StockId { get; set; }
        public decimal QtyUnit { get; set; }
        public decimal GramPerUnit { get; set; }
        public decimal TotalGram { get; set; }
        public StockEntity? Stock { get; set; }
    }

    public class CreateFeedingLineDto
    {
        public long FeedingId { get; set; }
        public long? ProjectId { get; set; }
        public DateTime? FeedingDate { get; set; }
        public FeedingSlot? FeedingSlot { get; set; }
        public FeedingSourceType? SourceType { get; set; }
        public DocumentStatus? Status { get; set; }
        public string? FeedingNo { get; set; }
        public string? Note { get; set; }
        public long StockId { get; set; }
        public decimal QtyUnit { get; set; }
        public decimal GramPerUnit { get; set; }
        public decimal TotalGram { get; set; }
        public StockEntity? Stock { get; set; }
    }

    public class UpdateFeedingLineDto : CreateFeedingLineDto
    {
    }

    public class CreateFeedingLineWithAutoHeaderDto
    {
        public long ProjectId { get; set; }
        public DateTime FeedingDate { get; set; }
        public FeedingSlot FeedingSlot { get; set; }
        public FeedingSourceType SourceType { get; set; } = FeedingSourceType.Manual;
        public string? Note { get; set; }
        public long StockId { get; set; }
        public decimal QtyUnit { get; set; }
        public decimal GramPerUnit { get; set; }
        public decimal TotalGram { get; set; }
    }
}
