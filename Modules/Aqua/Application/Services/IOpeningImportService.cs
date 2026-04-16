namespace aqua_api.Modules.Aqua.Application.Services
{
    public interface IOpeningImportService
    {
        Task<ApiResponse<OpeningImportPreviewResponseDto>> PreviewAsync(OpeningImportPreviewRequestDto dto);
        Task<ApiResponse<OpeningImportPreviewResponseDto>> GetByIdAsync(long id);
        Task<ApiResponse<OpeningImportCommitResultDto>> CommitAsync(long id);
    }
}
