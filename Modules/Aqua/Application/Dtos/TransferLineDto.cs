using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class TransferLineDto
    {
        public long Id { get; set; }
        public long TransferId { get; set; }
        public long FishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public long ToProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }

    public class CreateTransferLineDto
    {
        public long TransferId { get; set; }
        public long FishBatchId { get; set; }
        public long FromProjectCageId { get; set; }
        public long ToProjectCageId { get; set; }
        public int FishCount { get; set; }
        public decimal AverageGram { get; set; }
        public decimal BiomassGram { get; set; }
    }

    public class UpdateTransferLineDto : CreateTransferLineDto
    {
    }
}
