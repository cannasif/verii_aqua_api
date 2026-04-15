using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using aqua_api.Shared.Common.Helpers;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class GoodsReceiptLineService : IGoodsReceiptLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public GoodsReceiptLineService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        private static void NormalizePricing(CreateGoodsReceiptLineDto dto)
        {
            var pricing = AquaLinePricingMath.NormalizeGoodsReceiptLine(
                (byte)dto.ItemType,
                dto.QtyUnit,
                dto.TotalGram,
                dto.FishTotalGram,
                dto.CurrencyCode,
                dto.ExchangeRate,
                dto.UnitPrice
            );

            dto.CurrencyCode = pricing.CurrencyCode;
            dto.ExchangeRate = pricing.ExchangeRate;
            dto.UnitPrice = pricing.UnitPrice;
            dto.LocalUnitPrice = pricing.LocalUnitPrice;
            dto.LineAmount = pricing.LineAmount;
            dto.LocalLineAmount = pricing.LocalLineAmount;
        }

        public async Task<ApiResponse<GoodsReceiptLineDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.GoodsReceiptLines
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<GoodsReceiptLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("GoodsReceiptLineService.NotFound"),
                        _localizationService.GetLocalizedString("GoodsReceiptLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<GoodsReceiptLineDto>(entity);
                return ApiResponse<GoodsReceiptLineDto>.SuccessResult(dto, _localizationService.GetLocalizedString("GoodsReceiptLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<GoodsReceiptLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<GoodsReceiptLineDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.GoodsReceiptLines
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(GoodsReceiptLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<GoodsReceiptLineDto>(x)).ToList();

                var pagedResponse = new PagedResponse<GoodsReceiptLineDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<GoodsReceiptLineDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("GoodsReceiptLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<GoodsReceiptLineDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<GoodsReceiptLineDto>> CreateAsync(CreateGoodsReceiptLineDto dto)
        {
            try
            {
                NormalizePricing(dto);
                var entity = _mapper.Map<GoodsReceiptLine>(dto);
                await _unitOfWork.GoodsReceiptLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<GoodsReceiptLineDto>(entity);
                return ApiResponse<GoodsReceiptLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("GoodsReceiptLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<GoodsReceiptLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<GoodsReceiptLineDto>> UpdateAsync(long id, UpdateGoodsReceiptLineDto dto)
        {
            try
            {
                NormalizePricing(dto);
                var repo = _unitOfWork.GoodsReceiptLines;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<GoodsReceiptLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("GoodsReceiptLineService.NotFound"),
                        _localizationService.GetLocalizedString("GoodsReceiptLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<GoodsReceiptLineDto>(entity);
                return ApiResponse<GoodsReceiptLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("GoodsReceiptLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<GoodsReceiptLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.GoodsReceiptLines;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("GoodsReceiptLineService.NotFound"),
                        _localizationService.GetLocalizedString("GoodsReceiptLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("GoodsReceiptLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
