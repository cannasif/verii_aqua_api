namespace aqua_api.Modules.Aqua.Domain.Entities
{
    public class AquaSetting : BaseEntity
    {
        public bool RequireFullTransfer { get; set; } = true;
        public bool AllowProjectMerge { get; set; } = false;

        // 0: Dolu kafese kismi transfer yasak
        // 1: Dolu kafese sadece ayni batch ise izin ver
        // 2: Dolu kafese her durumda izin ver
        public int PartialTransferOccupiedCageMode { get; set; } = 0;
    }
}
