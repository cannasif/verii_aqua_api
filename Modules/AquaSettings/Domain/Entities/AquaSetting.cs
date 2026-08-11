namespace aqua_api.Modules.AquaSettings.Domain.Entities
{
    public class AquaSetting : BaseEntity
    {
        public bool RequireFullTransfer { get; set; } = true;
        public bool AllowProjectMerge { get; set; } = false;

        // 0: Dolu kafese kismi transfer yasak
        // 1: Dolu kafese sadece ayni batch ise izin ver
        // 2: Dolu kafese her durumda izin ver
        public int PartialTransferOccupiedCageMode { get; set; } = 0;

        // 0: Agirlikli ortalama
        // 1: FIFO
        // 2: Son alim fiyati
        public int FeedCostFallbackStrategy { get; set; } = 0;

        // 0: Her fireyi olay tarihindeki gramaj ile hesapla
        // 1: Toplam fireyi rapor donemi sonundaki son gramaj ile hesapla
        public int MortalityBiomassCalculationMode { get; set; } = 0;
    }
}
