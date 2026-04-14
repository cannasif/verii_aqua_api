
namespace aqua_api.Modules.Aqua.Application.Dtos
{
    public class ShipmentDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string ShipmentNo { get; set; } = string.Empty;
        public DateTime ShipmentDate { get; set; }
        public string? TargetWarehouse { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class CreateShipmentDto
    {
        public long ProjectId { get; set; }
        public string ShipmentNo { get; set; } = string.Empty;
        public DateTime ShipmentDate { get; set; }
        public string? TargetWarehouse { get; set; }
        public DocumentStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateShipmentDto : CreateShipmentDto
    {
    }
}
