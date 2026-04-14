using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace aqua_api.Modules.Stock.Application.Dtos
{
    public class StockGetDto : BaseEntityDto
    {
        public string ErpStockCode { get; set; } = string.Empty;
        public string StockName { get; set; } = string.Empty;
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
        public StockDetailGetDto? StockDetail { get; set; }
        public List<StockImageDto>? StockImages { get; set; }
        public List<StockRelationDto>? ParentRelations { get; set; }
    }

    public class StockCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string ErpStockCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string StockName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Unit { get; set; }

        [MaxLength(50)]
        public string? UreticiKodu { get; set; }

        [MaxLength(50)]
        public string? GrupKodu { get; set; }

        [MaxLength(250)]
        public string? GrupAdi { get; set; }

        [MaxLength(50)]
        public string? Kod1 { get; set; }

        [MaxLength(250)]
        public string? Kod1Adi { get; set; }

        [MaxLength(50)]
        public string? Kod2 { get; set; }

        [MaxLength(250)]
        public string? Kod2Adi { get; set; }

        [MaxLength(50)]
        public string? Kod3 { get; set; }

        [MaxLength(250)]
        public string? Kod3Adi { get; set; }

        [MaxLength(50)]
        public string? Kod4 { get; set; }

        [MaxLength(250)]
        public string? Kod4Adi { get; set; }

        [MaxLength(50)]
        public string? Kod5 { get; set; }

        [MaxLength(250)]
        public string? Kod5Adi { get; set; }

        public int BranchCode { get; set; }
    }

    public class StockUpdateDto
    {
        [Required]
        [MaxLength(50)]
        public string ErpStockCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string StockName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Unit { get; set; }

        [MaxLength(50)]
        public string? UreticiKodu { get; set; }

        [MaxLength(50)]
        public string? GrupKodu { get; set; }

        [MaxLength(250)]
        public string? GrupAdi { get; set; }

        [MaxLength(50)]
        public string? Kod1 { get; set; }

        [MaxLength(250)]
        public string? Kod1Adi { get; set; }

        [MaxLength(50)]
        public string? Kod2 { get; set; }

        [MaxLength(250)]
        public string? Kod2Adi { get; set; }

        [MaxLength(50)]
        public string? Kod3 { get; set; }

        [MaxLength(250)]
        public string? Kod3Adi { get; set; }

        [MaxLength(50)]
        public string? Kod4 { get; set; }

        [MaxLength(250)]
        public string? Kod4Adi { get; set; }

        [MaxLength(50)]
        public string? Kod5 { get; set; }

        [MaxLength(250)]
        public string? Kod5Adi { get; set; }

        public int BranchCode { get; set; }
    }

    public class StockGetWithMainImageDto : StockGetDto
    {
        public StockImageDto? MainImage { get; set; }
    }
}
