namespace aqua_api.Modules.Integrations.Application.Services
{
    public interface IErpReceiptResyncService
    {
        Task<ApiResponse<ErpReceiptResyncPreviewDto>> PreviewAsync(string documentNo, string inOutCode, string operationType);
        Task<ApiResponse<ErpReceiptResyncResultDto>> ResyncAsync(ErpReceiptResyncRequestDto request, long userId);
    }
}
