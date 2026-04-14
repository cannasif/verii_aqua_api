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
}
