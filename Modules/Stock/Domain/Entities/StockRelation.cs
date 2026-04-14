using System;
using System.Collections.Generic;
namespace aqua_api.Modules.Stock.Domain.Entities
{
    public class StockRelation : BaseEntity
    {

    // Stock ile ilişkilendirme
    public long StockId { get; set; }
    public Stock Stock { get; set; } = null!;

   // Bağlı ürün (ana ürüne bağlı olarak kullanılır)
    public long RelatedStockId { get; set; }
    public Stock RelatedStock { get; set; } = null!;

    // Bağlı ürün sayısı (örneğin 10 küçük tornavida)
    public decimal Quantity { get; set; }

    // Ekstra açıklama
    public string? Description { get; set; }

    // Zorunlu mu? (bazı ürünler opsiyonel olabilir)
    public bool IsMandatory { get; set; } = true;
    }
}