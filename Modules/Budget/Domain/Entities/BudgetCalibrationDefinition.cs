namespace aqua_api.Modules.Budget.Domain.Entities
{
    public class BudgetCalibrationDefinition : BaseEntity
    {
        public string CalibrationCode { get; set; } = null!;
        public string CalibrationInfo { get; set; } = null!;
        public string? Description { get; set; }
    }
}
