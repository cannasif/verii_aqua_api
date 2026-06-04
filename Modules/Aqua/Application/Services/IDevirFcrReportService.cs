namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IDevirFcrReportService
    {
        Task<ApiResponse<DevirFcrReportDto>> GetReportAsync(DevirFcrReportRequestDto request);
    }
}
