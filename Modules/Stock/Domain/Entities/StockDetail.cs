using System;
using System.Collections.Generic;
namespace aqua_api.Modules.Stock.Domain.Entities
{
    public class StockDetail : BaseEntity
    {

        // Stock ile ilişkilendirme
        public long StockId { get; set; }
        public Stock Stock { get; set; } = null!;

        // HTML açıklama (kullanıcı tarafından girilir)
     public string HtmlDescription { get; set; } = null!;

    // Teknik özellikler (JSON veya Text formatında)
    public string? TechnicalSpecsJson { get; set; }
    }
}