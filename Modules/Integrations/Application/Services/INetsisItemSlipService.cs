namespace aqua_api.Modules.Integrations.Application.Services;

public interface INetsisItemSlipService
{
    Task<NetsisItemSlipCreateResponseDto> CreateWarehouseTransferOutAsync(
        NetsisItemSlipCreateDto request,
        CancellationToken cancellationToken = default);

    Task<NetsisItemSlipCreateResponseDto> CreateWarehouseTransferInAsync(
        NetsisItemSlipCreateDto request,
        CancellationToken cancellationToken = default);

    Task<NetsisItemSlipCreateResponseDto> CreateDocumentAsync(
        NetsisItemSlipCreateDto request,
        NetsisItemSlipDocumentType documentType,
        CancellationToken cancellationToken = default);
}
