using System;
using System.Collections.Generic;
namespace aqua_api.Modules.Stock.Domain.Entities
{
    public class Stock : BaseEntity
    {
        // ERP'den gelen ana ürün kodu
        public string ErpStockCode { get; set; } = null!;

        // Ürün adı
        public string StockName { get; set; } = null!;

        // Birim (adet, kg, metrekare vb.)
        public string? Unit { get; set; }

        public string? UreticiKodu { get; set; }

        public string? GrupKodu { get; set; }

        public string? GrupAdi { get; set; }

        public string? Kod1 { get; set; }
        public string? Kod1Adi { get; set; }

        public string? Kod2 { get; set; }
        public string? Kod2Adi { get; set; }
        public string? Kod3 { get; set; }
        public string? Kod3Adi { get; set; }
        public string? Kod4 { get; set; }
        public string? Kod4Adi { get; set; }
        public string? Kod5 { get; set; }
        public string? Kod5Adi { get; set; }
        public int BranchCode { get; set; }

        // Navigation properties
        public StockDetail? StockDetail { get; set; }
        public ICollection<StockImage> StockImages { get; set; } = new List<StockImage>();
        public ICollection<StockRelation> ParentRelations { get; set; } = new List<StockRelation>();

    }
}