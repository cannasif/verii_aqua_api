using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Stock.Application.Services
{
    public class StockService : IStockService
    {
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
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = request.SortBy ?? nameof(StockEntity.Id);
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var items = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var dtos = items.Select(x => _mapper.Map<StockGetDto>(x)).ToList();

                var pagedResponse = new PagedResponse<StockGetDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

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
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = request.SortBy ?? nameof(StockEntity.Id);
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var items = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var baseDtos = items.Select(x => _mapper.Map<StockGetDto>(x)).ToList();
                
                var dtos = baseDtos.Select(stockDto =>
                {
                    var stockWithMainImage = _mapper.Map<StockGetWithMainImageDto>(stockDto);
                    // Main image'ı bul (IsPrimary = true olan)
                    var mainImage = stockDto.StockImages?.FirstOrDefault(img => img.IsPrimary);
                    stockWithMainImage.MainImage = mainImage;
                    return stockWithMainImage;
                }).ToList();

                var pagedResponse = new PagedResponse<StockGetWithMainImageDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

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

                // 🔹 INSERT
                if (!existingStocks.TryGetValue(erpStock.StokKodu, out var stock))
                {
                    newStocks.Add(new StockEntity
                    {
                        ErpStockCode = erpStock.StokKodu,
                        StockName = erpStock.StokAdi ?? string.Empty,
                        Unit = erpStock.OlcuBr1,
                        UreticiKodu = erpStock.UreticiKodu,
                        GrupKodu = erpStock.GrupKodu,
                        GrupAdi = erpStock.GrupIsim,
                        Kod1 = erpStock.Kod1,
                        Kod1Adi = erpStock.Kod1Adi,
                        Kod2 = erpStock.Kod2,
                        Kod2Adi = erpStock.Kod2Adi,
                        Kod3 = erpStock.Kod3,
                        Kod3Adi = erpStock.Kod3Adi,
                        Kod4 = erpStock.Kod4,
                        Kod4Adi = erpStock.Kod4Adi,
                        Kod5 = erpStock.Kod5,
                        Kod5Adi = erpStock.Kod5Adi,
                        BranchCode = erpStock.SubeKodu,
                        IsDeleted = false
                    });

                    hasAnyChange = true;
                    continue;
                }

                // 🔹 UPDATE (ANY FIELD CHANGED)
                if (
                    stock.StockName != erpStock.StokAdi ||
                    stock.Unit != erpStock.OlcuBr1 ||
                    stock.UreticiKodu != erpStock.UreticiKodu ||
                    stock.GrupKodu != erpStock.GrupKodu ||
                    stock.GrupAdi != erpStock.GrupIsim ||
                    stock.Kod1 != erpStock.Kod1 ||
                    stock.Kod1Adi != erpStock.Kod1Adi ||
                    stock.Kod2 != erpStock.Kod2 ||
                    stock.Kod2Adi != erpStock.Kod2Adi ||
                    stock.Kod3 != erpStock.Kod3 ||
                    stock.Kod3Adi != erpStock.Kod3Adi ||
                    stock.Kod4 != erpStock.Kod4 ||
                    stock.Kod4Adi != erpStock.Kod4Adi ||
                    stock.Kod5 != erpStock.Kod5 ||
                    stock.Kod5Adi != erpStock.Kod5Adi ||
                    stock.BranchCode != erpStock.SubeKodu
                )
                {
                    stock.StockName = erpStock.StokAdi ?? string.Empty;
                    stock.Unit = erpStock.OlcuBr1;
                    stock.UreticiKodu = erpStock.UreticiKodu;
                    stock.GrupKodu = erpStock.GrupKodu;
                    stock.GrupAdi = erpStock.GrupIsim;
                    stock.Kod1 = erpStock.Kod1;
                    stock.Kod1Adi = erpStock.Kod1Adi;
                    stock.Kod2 = erpStock.Kod2;
                    stock.Kod2Adi = erpStock.Kod2Adi;
                    stock.Kod3 = erpStock.Kod3;
                    stock.Kod3Adi = erpStock.Kod3Adi;
                    stock.Kod4 = erpStock.Kod4;
                    stock.Kod4Adi = erpStock.Kod4Adi;
                    stock.Kod5 = erpStock.Kod5;
                    stock.Kod5Adi = erpStock.Kod5Adi;
                    stock.BranchCode = erpStock.SubeKodu;

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

    }
}
