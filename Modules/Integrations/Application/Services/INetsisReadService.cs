namespace aqua_api.Modules.Integrations.Application.Services
{
    /// <summary>
    /// Centralized read facade for Netsis/ERP-backed reads.
    /// Aqua keeps existing ERP contracts, but the underlying read orchestration
    /// lives here so controller/service boundaries match the CRM structure.
    /// </summary>
    public interface INetsisReadService
    {
        Task<ApiResponse<short>> GetBranchCodeFromContextAsync();
        Task<ApiResponse<List<CariDto>>> GetCustomersAsync(string? customerCode);
        Task<ApiResponse<PagedResponse<CariDto>>> GetCustomersPagedAsync(int pageNumber, int pageSize, string? search, string? sortBy, string? sortDirection);
        Task<ApiResponse<List<CariDto>>> GetCustomersByCodesAsync(IEnumerable<string> customerCodes);
        Task<ApiResponse<List<DepoDto>>> GetWarehousesAsync(short? warehouseCode);
        Task<ApiResponse<PagedResponse<DepoDto>>> GetWarehousesPagedAsync(int pageNumber, int pageSize, string? search, string? sortBy, string? sortDirection);
        Task<ApiResponse<List<StokFunctionDto>>> GetStocksAsync(string? stockCode);
        Task<ApiResponse<PagedResponse<StokFunctionDto>>> GetStocksPagedAsync(int pageNumber, int pageSize, string? search, string? sortBy, string? sortDirection);
        Task<ApiResponse<List<BranchDto>>> GetBranchesAsync(int? branchNo = null);
        Task<ApiResponse<PagedResponse<BranchDto>>> GetBranchesPagedAsync(int pageNumber, int pageSize, string? search, string? sortBy, string? sortDirection);
        Task<ApiResponse<List<KurDto>>> GetExchangeRatesAsync(DateTime date, int pricingType);
        Task<ApiResponse<List<ErpShippingAddressDto>>> GetShippingAddressesAsync(string customerCode);
        Task<ApiResponse<List<StokGroupDto>>> GetStockGroupsAsync(string? groupCode);
        Task<ApiResponse<List<ProjeDto>>> GetProjectsAsync();
        Task<ApiResponse<List<MalKabulVeSevkiyatDto>>> GetGoodsReceiptAndShipmentMovementsAsync(DateTime? startDate = null);
        Task<ApiResponse<PagedResponse<MalKabulVeSevkiyatDto>>> GetGoodsReceiptAndShipmentMovementsPagedAsync(int pageNumber, int pageSize, string? search, DateTime? startDate, string? sortBy, string? sortDirection);
        Task<ApiResponse<List<ErpReceiptShipmentMovementDto>>> GetReceiptShipmentMovementMirrorAsync();
        Task<ApiResponse<PagedResponse<ErpReceiptShipmentMovementDto>>> GetReceiptShipmentMovementMirrorPagedAsync(int pageNumber, int pageSize, string? search, string? sortBy, string? sortDirection);
        Task<ApiResponse<object>> HealthCheckAsync();
    }
}
