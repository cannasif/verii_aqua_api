using System;

namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class MortalityLineDto
    {
        public long Id { get; set; }
        public long MortalityId { get; set; }
        public long FishBatchId { get; set; }
        public long ProjectCageId { get; set; }
        public int DeadCount { get; set; }
    }

    public class CreateMortalityLineDto
    {
        public long MortalityId { get; set; }
        public long FishBatchId { get; set; }
        public long ProjectCageId { get; set; }
        public int DeadCount { get; set; }
    }

    public class UpdateMortalityLineDto : CreateMortalityLineDto
    {
    }

    public class CreateMortalityLineWithAutoHeaderDto
    {
        public long ProjectId { get; set; }
        public DateTime MortalityDate { get; set; }
        public long FishBatchId { get; set; }
        public long ProjectCageId { get; set; }
        public int DeadCount { get; set; }
    }
}
