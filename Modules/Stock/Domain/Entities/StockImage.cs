using System;
using System.Collections.Generic;
namespace aqua_api.Modules.Stock.Domain.Entities
{
    public class StockImage : BaseEntity
    {

        // Stock ile ilişkilendirme
        public long StockId { get; set; }
        public Stock Stock { get; set; } = null!;

        // Görsel dosya yolu
        public string FilePath { get; set; } = null!;

        // Görselin açıklaması (alt text)
        public string? AltText { get; set; }

        // Görselin sırası
        public int SortOrder { get; set; }

        // Bu görsel ana görsel mi?
        public bool IsPrimary { get; set; }
    }
}