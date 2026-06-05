namespace aqua_api.Modules.Cages.Application.Dtos
{
    public class CageWarehouseMappingDto
    {
        public long Id { get; set; }
        public long CageId { get; set; }
        public string? CageCode { get; set; }
        public string? CageName { get; set; }
        public long WarehouseId { get; set; }
        public short? ErpWarehouseCode { get; set; }
        public string? WarehouseName { get; set; }
        public bool IsActive { get; set; }
        public string? Note { get; set; }
    }

    public class CreateCageWarehouseMappingDto
    {
        public long CageId { get; set; }
        public long WarehouseId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Note { get; set; }
    }

    public class UpdateCageWarehouseMappingDto : CreateCageWarehouseMappingDto
    {
    }
}
