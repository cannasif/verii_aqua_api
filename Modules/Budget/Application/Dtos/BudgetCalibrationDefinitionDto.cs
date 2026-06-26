namespace aqua_api.Modules.Budget.Application.Dtos
{
    public class BudgetCalibrationDefinitionDto
    {
        public long Id { get; set; }
        public string CalibrationCode { get; set; } = string.Empty;
        public string CalibrationInfo { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CreateBudgetCalibrationDefinitionDto
    {
        public string CalibrationCode { get; set; } = string.Empty;
        public string CalibrationInfo { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateBudgetCalibrationDefinitionDto : CreateBudgetCalibrationDefinitionDto
    {
    }
}
