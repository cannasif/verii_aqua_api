using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Stock.Application.Services
{
    public class StockDetailService : IStockDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public StockDetailService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<PagedResponse<StockDetailGetDto>>> GetAllStockDetailsAsync(PagedRequest request)
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

                var columnMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "stockName", "Stock.StockName" }
                };

                var query = _unitOfWork.StockDetails
                    .Query()
                    .Where(sd => !sd.IsDeleted)
                    .Include(sd => sd.Stock)
                    .Include(sd => sd.CreatedByUser)
                    .Include(sd => sd.UpdatedByUser)
                    .Include(sd => sd.DeletedByUser)
                    .ApplyFilters(request.Filters, request.FilterLogic, columnMapping);

                var sortBy = request.SortBy ?? nameof(StockDetail.Id);
                query = query.ApplySorting(sortBy, request.SortDirection, columnMapping);

                var totalCount = await query.CountAsync();

                var items = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var dtos = items.Select(x => _mapper.Map<StockDetailGetDto>(x)).ToList();

                var pagedResponse = new PagedResponse<StockDetailGetDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<StockDetailGetDto>>.SuccessResult(
                    pagedResponse, 
                    _localizationService.GetLocalizedString("StockDetailService.StockDetailsRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<StockDetailGetDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("StockDetailService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockDetailService.GetAllStockDetailsExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<StockDetailGetDto>> GetStockDetailByIdAsync(long id)
        {
            try
            {
                var stockDetail = await _unitOfWork.StockDetails
                    .Query()
                    .Include(sd => sd.Stock)
                    .Include(sd => sd.CreatedByUser)
                    .Include(sd => sd.UpdatedByUser)
                    .Include(sd => sd.DeletedByUser)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sd => sd.Id == id && !sd.IsDeleted);

                if (stockDetail == null)
                {
                    return ApiResponse<StockDetailGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockDetailService.StockDetailNotFound"),
                        _localizationService.GetLocalizedString("StockDetailService.StockDetailNotFound"),
                        StatusCodes.Status404NotFound);
                }

                var stockDetailDto = _mapper.Map<StockDetailGetDto>(stockDetail);

                return ApiResponse<StockDetailGetDto>.SuccessResult(
                    stockDetailDto, 
                    _localizationService.GetLocalizedString("StockDetailService.StockDetailRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockDetailGetDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockDetailService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockDetailService.GetStockDetailByIdExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<StockDetailGetDto>> GetStockDetailByStockIdAsync(long stockId)
        {
            try
            {
                var stockDetail = await _unitOfWork.StockDetails
                    .Query()
                    .Where(sd => sd.StockId == stockId && !sd.IsDeleted)
                    .Include(sd => sd.Stock)
                    .Include(sd => sd.CreatedByUser)
                    .Include(sd => sd.UpdatedByUser)
                    .Include(sd => sd.DeletedByUser)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                var stockDetailDto = _mapper.Map<StockDetailGetDto>(stockDetail);

                return ApiResponse<StockDetailGetDto>.SuccessResult(
                    stockDetailDto, 
                    _localizationService.GetLocalizedString("StockDetailService.StockDetailRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockDetailGetDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockDetailService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockDetailService.GetStockDetailByStockIdExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<StockDetailGetDto>> CreateStockDetailAsync(StockDetailCreateDto stockDetailCreateDto)
        {
            try
            {
                // Business Rule: Check if Stock exists
                var stock = await _unitOfWork.Stocks.GetByIdAsync(stockDetailCreateDto.StockId);
                if (stock == null || stock.IsDeleted)
                {
                    return ApiResponse<StockDetailGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockDetailService.StockNotFound"),
                        _localizationService.GetLocalizedString("StockDetailService.StockNotFound"),
                        StatusCodes.Status400BadRequest);
                }

                // Business Rule: Check if StockDetail already exists for this Stock
                var existingStockDetail = await _unitOfWork.StockDetails
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sd => sd.StockId == stockDetailCreateDto.StockId && !sd.IsDeleted);

                if (existingStockDetail != null)
                {
                    return ApiResponse<StockDetailGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockDetailService.StockDetailAlreadyExists"),
                        _localizationService.GetLocalizedString("StockDetailService.StockDetailAlreadyExists"),
                        StatusCodes.Status400BadRequest);
                }

                var stockDetail = _mapper.Map<StockDetail>(stockDetailCreateDto);
                await _unitOfWork.StockDetails.AddAsync(stockDetail);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties for mapping
                var stockDetailWithNav = await _unitOfWork.StockDetails
                    .Query()
                    .Include(sd => sd.Stock)
                    .Include(sd => sd.CreatedByUser)
                    .Include(sd => sd.UpdatedByUser)
                    .Include(sd => sd.DeletedByUser)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sd => sd.Id == stockDetail.Id && !sd.IsDeleted);

                if (stockDetailWithNav == null)
                {
                    return ApiResponse<StockDetailGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockDetailService.StockDetailNotFound"),
                        _localizationService.GetLocalizedString("StockDetailService.StockDetailNotFound"),
                        StatusCodes.Status404NotFound);
                }

                var stockDetailDto = _mapper.Map<StockDetailGetDto>(stockDetailWithNav);

                return ApiResponse<StockDetailGetDto>.SuccessResult(
                    stockDetailDto, 
                    _localizationService.GetLocalizedString("StockDetailService.StockDetailCreated"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockDetailGetDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockDetailService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockDetailService.CreateStockDetailExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<StockDetailGetDto>> UpdateStockDetailAsync(long id, StockDetailUpdateDto stockDetailUpdateDto)
        {
            try
            {
                var stockDetail = await _unitOfWork.StockDetails.GetByIdForUpdateAsync(id);
                if (stockDetail == null)
                {
                    return ApiResponse<StockDetailGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockDetailService.StockDetailNotFound"),
                        _localizationService.GetLocalizedString("StockDetailService.StockDetailNotFound"),
                        StatusCodes.Status404NotFound);
                }

                // Business Rule: Check if Stock exists
                var stock = await _unitOfWork.Stocks.GetByIdAsync(stockDetailUpdateDto.StockId);
                if (stock == null || stock.IsDeleted)
                {
                    return ApiResponse<StockDetailGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockDetailService.StockNotFound"),
                        _localizationService.GetLocalizedString("StockDetailService.StockNotFound"),
                        StatusCodes.Status400BadRequest);
                }

                // Business Rule: Check if StockDetail already exists for this Stock (excluding current)
                if (stockDetail.StockId != stockDetailUpdateDto.StockId)
                {
                    var existingStockDetail = await _unitOfWork.StockDetails
                        .Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(sd => sd.StockId == stockDetailUpdateDto.StockId && sd.Id != id && !sd.IsDeleted);

                    if (existingStockDetail != null)
                    {
                        return ApiResponse<StockDetailGetDto>.ErrorResult(
                            _localizationService.GetLocalizedString("StockDetailService.StockDetailAlreadyExists"),
                            _localizationService.GetLocalizedString("StockDetailService.StockDetailAlreadyExists"),
                            StatusCodes.Status400BadRequest);
                    }
                }

                _mapper.Map(stockDetailUpdateDto, stockDetail);
                await _unitOfWork.StockDetails.UpdateAsync(stockDetail);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties for mapping
                var stockDetailWithNav = await _unitOfWork.StockDetails
                    .Query()
                    .Include(sd => sd.Stock)
                    .Include(sd => sd.CreatedByUser)
                    .Include(sd => sd.UpdatedByUser)
                    .Include(sd => sd.DeletedByUser)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sd => sd.Id == stockDetail.Id && !sd.IsDeleted);

                if (stockDetailWithNav == null)
                {
                    return ApiResponse<StockDetailGetDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockDetailService.StockDetailNotFound"),
                        _localizationService.GetLocalizedString("StockDetailService.StockDetailNotFound"),
                        StatusCodes.Status404NotFound);
                }

                var stockDetailDto = _mapper.Map<StockDetailGetDto>(stockDetailWithNav);

                return ApiResponse<StockDetailGetDto>.SuccessResult(
                    stockDetailDto, 
                    _localizationService.GetLocalizedString("StockDetailService.StockDetailUpdated"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockDetailGetDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockDetailService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockDetailService.UpdateStockDetailExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<object>> DeleteStockDetailAsync(long id)
        {
            try
            {
                var stockDetail = await _unitOfWork.StockDetails.GetByIdAsync(id);
                if (stockDetail == null)
                {
                    return ApiResponse<object>.ErrorResult(
                        _localizationService.GetLocalizedString("StockDetailService.StockDetailNotFound"),
                        _localizationService.GetLocalizedString("StockDetailService.StockDetailNotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.StockDetails.SoftDeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(
                    null, 
                    _localizationService.GetLocalizedString("StockDetailService.StockDetailDeleted"));
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResult(
                    _localizationService.GetLocalizedString("StockDetailService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockDetailService.DeleteStockDetailExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
