namespace aqua_api.Modules.AquaReports.Application.Services
{
    public interface IDevirFcrReportService
    {
        Task<ApiResponse<DevirFcrReportDto>> GetReportAsync(DevirFcrReportRequestDto request);
    }
}
