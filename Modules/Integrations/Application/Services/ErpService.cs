namespace aqua_api.Modules.Integrations.Application.Services
{
    public class ErpService : IErpService
    {
        private readonly INetsisReadService _netsisReadService;

        public ErpService(INetsisReadService netsisReadService)
        {
            _netsisReadService = netsisReadService;
        }

        public Task<ApiResponse<short>> GetBranchCodeFromContext()
            => _netsisReadService.GetBranchCodeFromContextAsync();

        public async Task<ApiResponse<List<DepoDto>>> GetDeposAsync(short? depoKodu)
            => await _netsisReadService.GetWarehousesAsync(depoKodu);

        public async Task<ApiResponse<List<CariDto>>> GetCarisAsync(string? cariKodu)
            => await _netsisReadService.GetCustomersAsync(cariKodu);

        public async Task<ApiResponse<List<CariDto>>> GetCarisByCodesAsync(IEnumerable<string> cariKodlari)
            => await _netsisReadService.GetCustomersByCodesAsync(cariKodlari);

        public async Task<ApiResponse<List<StokFunctionDto>>> GetStoksAsync(string? stokKodu)
            => await _netsisReadService.GetStocksAsync(stokKodu);

        public async Task<ApiResponse<List<BranchDto>>> GetBranchesAsync(int? branchNo = null)
            => await _netsisReadService.GetBranchesAsync(branchNo);

        public async Task<ApiResponse<List<KurDto>>> GetExchangeRateAsync(DateTime tarih, int fiyatTipi)
            => await _netsisReadService.GetExchangeRatesAsync(tarih, fiyatTipi);
    
        public async Task<ApiResponse<List<ErpShippingAddressDto>>> GetErpShippingAddressAsync(string customerCode)
            => await _netsisReadService.GetShippingAddressesAsync(customerCode);

        public async Task<ApiResponse<List<StokGroupDto>>> GetStokGroupAsync(string? grupKodu)
            => await _netsisReadService.GetStockGroupsAsync(grupKodu);

        public async Task<ApiResponse<List<ProjeDto>>> GetProjectCodesAsync()
            => await _netsisReadService.GetProjectsAsync();

        public async Task<ApiResponse<object>> HealthCheckAsync()
            => await _netsisReadService.HealthCheckAsync();
    }
}
