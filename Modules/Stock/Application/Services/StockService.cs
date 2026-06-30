using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Stock.Application.Services
{
    public class StockService : IStockService
    {
        private static readonly string[] SearchableColumns =
        [
            nameof(StockEntity.StockName),
            nameof(StockEntity.ErpStockCode),
            nameof(StockEntity.GrupKodu),
            nameof(StockEntity.GrupAdi),
            nameof(StockEntity.UreticiKodu),
            nameof(StockEntity.Kod1),
            nameof(StockEntity.Kod1Adi),
            nameof(StockEntity.Kod2),
            nameof(StockEntity.Kod2Adi),
            nameof(StockEntity.Kod3),
            nameof(StockEntity.Kod3Adi),
            nameof(StockEntity.Kod4),
            nameof(StockEntity.Kod4Adi),
            nameof(StockEntity.Kod5),
            nameof(StockEntity.Kod5Adi)
        ];

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;
        private readonly IErpService _erpService;

        public StockService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService, IErpService erpService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
            _erpService = erpService;
        }

        public async Task<ApiResponse<PagedResponse<StockGetDto>>> GetAllStocksAsync(PagedRequest request)
        {
            try
            {
                if (request == null)
                {
                    request = new PagedRequest();
                }

                if (request.Filters == null)
                {
                    request.Filters = new List<Filter>();
                }

                var query = _unitOfWork.Stocks
                    .Query()
                    .Where(s => !s.IsDeleted)
                    .Include(s => s.StockDetail)
                    .Include(s => s.StockImages.Where(i => !i.IsDeleted))
                    .Include(s => s.ParentRelations.Where(r => !r.IsDeleted))
                        .ThenInclude(r => r.RelatedStock)
                    .Include(s => s.CreatedByUser)
                    .Include(s => s.UpdatedByUser)
                    .Include(s => s.DeletedByUser)
                    .ApplySearch(request.Search, SearchableColumns)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = request.SortBy ?? nameof(StockEntity.Id);
                query = query.ApplySorting(sortBy, request.SortDirection);

                var page = await query
                    .AsNoTracking()
                    .ToPagedItemsAsync(request)
                    .ConfigureAwait(false);

                var pagedResponse = page.ToPagedResponse(x => _mapper.Map<StockGetDto>(x));

                return ApiResponse<PagedResponse<StockGetDto>>.SuccessResult(
                    pagedResponse, 
                    _localizationService.GetLocalizedString("StockService.StocksRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<StockGetDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("StockService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockService.GetAllStocksExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<StockGetWithMainImageDto>>> GetAllStocksWithImagesAsync(PagedRequest request)
        {
            try
            {
                if (request == null)
                {
                    request = new PagedRequest();
                }

                if (request.Filters == null)
                {
                    request.Filters = new List<Filter>();
                }

                var query = _unitOfWork.Stocks
                    .Query()
                    .Where(s => !s.IsDeleted)
                    .Include(s => s.StockDetail)
                    .Include(s => s.StockImages.Where(i => !i.IsDeleted))
                    .Include(s => s.ParentRelations.Where(r => !r.IsDeleted))
                        .ThenInclude(r => r.RelatedStock)
                    .Include(s => s.CreatedByUser)
                    .Include(s => s.UpdatedByUser)
                    .Include(s => s.DeletedByUser)
                    .ApplySearch(request.Search, SearchableColumns)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = request.SortBy ?? nameof(StockEntity.Id);
                query = query.ApplySorting(sortBy, request.SortDirection);

                var page = await query
                    .AsNoTracking()
                    .ToPagedItemsAsync(request)
                    .ConfigureAwait(false);

                var baseDtos = page.Items.Select(x => _mapper.Map<StockGetDto>(x)).ToList();
                
                var dtos = baseDtos.Select(stockDto =>
                {
                    var stockWithMainImage = _mapper.Map<StockGetWithMainImageDto>(stockDto);
                    // Main image'ı bul (IsPrimary = true olan)
                    var mainImage = stockDto.StockImages?.FirstOrDefault(img => img.IsPrimary);
                    stockWithMainImage.MainImage = mainImage;
                    return stockWithMainImage;
                }).ToList();

                var pagedResponse = page.ToResponse(dtos);

                return ApiResponse<PagedResponse<StockGetWithMainImageDto>>.SuccessResult(
                    pagedResponse, 
                    _localizationService.GetLocalizedString("StockService.StocksRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<StockGetWithMainImageDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("StockService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockService.GetAllStocksExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<StockGetDto>> GetStockByIdAsync(long id)
        {
            try
            {
                var stock = await _unitOfWork.Stocks
                    .Query()
                    .Include(s => s.StockDetail)
                    .Include(s => s.StockImages.Where(i => !i.IsDeleted))
                    .Include(s => s.ParentRelations.Where(r => !r.IsDeleted))
                        .ThenInclude(r => r.RelatedStock)
                    .Include(s => s.CreatedByUser)
                    .Include(s => s.UpdatedByUser)
                    .Include(s => s.DeletedByUser)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

                if (stock == null)
                {
                    return ApiResponse<StockGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockService.StockNotFound"),
                        _localizationService.GetLocalizedString("StockService.StockNotFound"),
                        StatusCodes.Status404NotFound);
                }

                var stockDto = _mapper.Map<StockGetDto>(stock);

                return ApiResponse<StockGetDto>.SuccessResult(
                    stockDto, 
                    _localizationService.GetLocalizedString("StockService.StockRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockGetDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockService.GetStockByIdExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<StockGetDto>> CreateStockAsync(StockCreateDto stockCreateDto)
        {
            try
            {
                // Business Rule: Check if ErpStockCode already exists
                var existingStock = await _unitOfWork.Stocks
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ErpStockCode == stockCreateDto.ErpStockCode && !s.IsDeleted);

                if (existingStock != null)
                {
                    return ApiResponse<StockGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockService.ErpStockCodeAlreadyExists"),
                        _localizationService.GetLocalizedString("StockService.ErpStockCodeAlreadyExists"),
                        StatusCodes.Status400BadRequest);
                }

                var stock = _mapper.Map<StockEntity>(stockCreateDto);
                await _unitOfWork.Stocks.AddAsync(stock);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties for mapping
                var stockWithNav = await _unitOfWork.Stocks
                    .Query()
                    .Include(s => s.StockDetail)
                    .Include(s => s.StockImages.Where(i => !i.IsDeleted))
                    .Include(s => s.ParentRelations.Where(r => !r.IsDeleted))
                        .ThenInclude(r => r.RelatedStock)
                    .Include(s => s.CreatedByUser)
                    .Include(s => s.UpdatedByUser)
                    .Include(s => s.DeletedByUser)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == stock.Id && !s.IsDeleted);

                if (stockWithNav == null)
                {
                    return ApiResponse<StockGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockService.StockNotFound"),
                        _localizationService.GetLocalizedString("StockService.StockNotFound"),
                        StatusCodes.Status404NotFound);
                }

                var stockDto = _mapper.Map<StockGetDto>(stockWithNav);

                return ApiResponse<StockGetDto>.SuccessResult(
                    stockDto, 
                    _localizationService.GetLocalizedString("StockService.StockCreated"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockGetDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockService.CreateStockExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<StockGetDto>> UpdateStockAsync(long id, StockUpdateDto stockUpdateDto)
        {
            try
            {
                var stock = await _unitOfWork.Stocks.GetByIdForUpdateAsync(id);
                if (stock == null)
                {
                    return ApiResponse<StockGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockService.StockNotFound"),
                        _localizationService.GetLocalizedString("StockService.StockNotFound"),
                        StatusCodes.Status404NotFound);
                }

                // Business Rule: Check if ErpStockCode already exists (excluding current stock)
                var existingStock = await _unitOfWork.Stocks
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ErpStockCode == stockUpdateDto.ErpStockCode && s.Id != id && !s.IsDeleted);

                if (existingStock != null)
                {
                    return ApiResponse<StockGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockService.ErpStockCodeAlreadyExists"),
                        _localizationService.GetLocalizedString("StockService.ErpStockCodeAlreadyExists"),
                        StatusCodes.Status400BadRequest);
                }

                _mapper.Map(stockUpdateDto, stock);
                await _unitOfWork.Stocks.UpdateAsync(stock);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties for mapping
                var stockWithNav = await _unitOfWork.Stocks
                    .Query()
                    .Include(s => s.StockDetail)
                    .Include(s => s.StockImages.Where(i => !i.IsDeleted))
                    .Include(s => s.ParentRelations.Where(r => !r.IsDeleted))
                        .ThenInclude(r => r.RelatedStock)
                    .Include(s => s.CreatedByUser)
                    .Include(s => s.UpdatedByUser)
                    .Include(s => s.DeletedByUser)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == stock.Id && !s.IsDeleted);

                if (stockWithNav == null)
                {
                    return ApiResponse<StockGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockService.StockNotFound"),
                        _localizationService.GetLocalizedString("StockService.StockNotFound"),
                        StatusCodes.Status404NotFound);
                }

                var stockDto = _mapper.Map<StockGetDto>(stockWithNav);

                return ApiResponse<StockGetDto>.SuccessResult(
                    stockDto, 
                    _localizationService.GetLocalizedString("StockService.StockUpdated"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockGetDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockService.UpdateStockExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<object>> DeleteStockAsync(long id)
        {
            try
            {
                var stock = await _unitOfWork.Stocks.GetByIdAsync(id);
                if (stock == null)
                {
                    return ApiResponse<object>.ErrorResult(
                        _localizationService.GetLocalizedString("StockService.StockNotFound"),
                        _localizationService.GetLocalizedString("StockService.StockNotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.Stocks.SoftDeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(
                    null, 
                    _localizationService.GetLocalizedString("StockService.StockDeleted"));
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResult(
                    _localizationService.GetLocalizedString("StockService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockService.DeleteStockExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }
                
        public async Task SyncStocksFromErpAsync()
        {
            var erpResponse = await _erpService.GetStoksAsync(null);

            if (erpResponse?.Data == null || erpResponse.Data.Count == 0)
                return;

            // 🔹 ERP sync için TRACKING açık olmalı (update yapacağız)
            var existingStocks = await _unitOfWork.Stocks
                .Query(tracking:true)
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.ErpStockCode);

            var newStocks = new List<StockEntity>();
            var hasAnyChange = false;

            foreach (var erpStock in erpResponse.Data)
            {
                if (string.IsNullOrWhiteSpace(erpStock.StokKodu))
                    continue;

                var code = Clean(erpStock.StokKodu);
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                var stockName = Clean(erpStock.StokAdi);
                stockName = string.IsNullOrWhiteSpace(stockName) ? code : stockName;
                var unit = Clean(erpStock.OlcuBr1);
                var ureticiKodu = Clean(erpStock.UreticiKodu);
                var grupKodu = Clean(erpStock.GrupKodu);
                var grupAdi = Clean(erpStock.GrupIsim);
                var kod1 = Clean(erpStock.Kod1);
                var kod1Adi = Clean(erpStock.Kod1Adi);
                var kod2 = Clean(erpStock.Kod2);
                var kod2Adi = Clean(erpStock.Kod2Adi);
                var kod3 = Clean(erpStock.Kod3);
                var kod3Adi = Clean(erpStock.Kod3Adi);
                var kod4 = Clean(erpStock.Kod4);
                var kod4Adi = Clean(erpStock.Kod4Adi);
                var kod5 = Clean(erpStock.Kod5);
                var kod5Adi = Clean(erpStock.Kod5Adi);

                // 🔹 INSERT
                if (!existingStocks.TryGetValue(code, out var stock))
                {
                    newStocks.Add(new StockEntity
                    {
                        ErpStockCode = code,
                        StockName = stockName,
                        Unit = unit,
                        UreticiKodu = ureticiKodu,
                        GrupKodu = grupKodu,
                        GrupAdi = grupAdi,
                        Kod1 = kod1,
                        Kod1Adi = kod1Adi,
                        Kod2 = kod2,
                        Kod2Adi = kod2Adi,
                        Kod3 = kod3,
                        Kod3Adi = kod3Adi,
                        Kod4 = kod4,
                        Kod4Adi = kod4Adi,
                        Kod5 = kod5,
                        Kod5Adi = kod5Adi,
                        BranchCode = erpStock.SubeKodu,
                        IsERPIntegrated = true,
                        ERPIntegrationNumber = code,
                        LastSyncDate = DateTime.UtcNow,
                        CountTriedBy = 0,
                        IsDeleted = false
                    });

                    hasAnyChange = true;
                    continue;
                }

                // 🔹 UPDATE (ANY FIELD CHANGED)
                var updated = false;
                if (stock.StockName != stockName) { stock.StockName = stockName; updated = true; }
                if (ApplyErpText(stock.Unit, unit, value => stock.Unit = value)) { updated = true; }
                if (ApplyErpText(stock.UreticiKodu, ureticiKodu, value => stock.UreticiKodu = value)) { updated = true; }
                if (ApplyErpText(stock.GrupKodu, grupKodu, value => stock.GrupKodu = value)) { updated = true; }
                if (ApplyErpText(stock.GrupAdi, grupAdi, value => stock.GrupAdi = value)) { updated = true; }
                if (ApplyErpText(stock.Kod1, kod1, value => stock.Kod1 = value)) { updated = true; }
                if (ApplyErpText(stock.Kod1Adi, kod1Adi, value => stock.Kod1Adi = value)) { updated = true; }
                if (ApplyErpText(stock.Kod2, kod2, value => stock.Kod2 = value)) { updated = true; }
                if (ApplyErpText(stock.Kod2Adi, kod2Adi, value => stock.Kod2Adi = value)) { updated = true; }
                if (ApplyErpText(stock.Kod3, kod3, value => stock.Kod3 = value)) { updated = true; }
                if (ApplyErpText(stock.Kod3Adi, kod3Adi, value => stock.Kod3Adi = value)) { updated = true; }
                if (ApplyErpText(stock.Kod4, kod4, value => stock.Kod4 = value)) { updated = true; }
                if (ApplyErpText(stock.Kod4Adi, kod4Adi, value => stock.Kod4Adi = value)) { updated = true; }
                if (ApplyErpText(stock.Kod5, kod5, value => stock.Kod5 = value)) { updated = true; }
                if (ApplyErpText(stock.Kod5Adi, kod5Adi, value => stock.Kod5Adi = value)) { updated = true; }
                if (stock.BranchCode != erpStock.SubeKodu) { stock.BranchCode = erpStock.SubeKodu; updated = true; }
                if (!stock.IsERPIntegrated) { stock.IsERPIntegrated = true; updated = true; }
                if (string.IsNullOrWhiteSpace(stock.ERPIntegrationNumber)) { stock.ERPIntegrationNumber = code; updated = true; }
                if (stock.CountTriedBy == null) { stock.CountTriedBy = 0; updated = true; }

                if (updated)
                {
                    stock.LastSyncDate = DateTime.UtcNow;
                    stock.UpdatedDate = DateTimeProvider.Now;
                    stock.UpdatedBy = null;
                    hasAnyChange = true;
                }
            }

            // 🔴 GERÇEKTEN HİÇ DEĞİŞİKLİK YOKSA
            if (!hasAnyChange)
                return;

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (newStocks.Count > 0)
                    await _unitOfWork.Stocks.AddAllAsync(newStocks);

                // Update için ayrıca çağrı yok
                // EF ChangeTracker zaten değişenleri takip ediyor

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private static string Clean(string? value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private static bool ApplyErpText(string? current, string incoming, Action<string> assign)
        {
            if (string.IsNullOrWhiteSpace(incoming) && !string.IsNullOrWhiteSpace(current))
            {
                return false;
            }

            var next = incoming;
            if (current == next)
            {
                return false;
            }

            assign(next);
            return true;
        }

    }
}
