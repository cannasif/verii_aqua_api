using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using aqua_api.Shared.Common.Helpers;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class ShipmentLineService : IShipmentLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public ShipmentLineService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        private static void NormalizePricing(CreateShipmentLineDto dto)
        {
            var pricing = AquaLinePricingMath.NormalizeShipmentLine(
                dto.BiomassGram,
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

        public async Task<ApiResponse<ShipmentLineDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.ShipmentLines
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<ShipmentLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<ShipmentLineDto>(entity);
                return ApiResponse<ShipmentLineDto>.SuccessResult(dto, _localizationService.GetLocalizedString("ShipmentLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<ShipmentLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<ShipmentLineDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.ShipmentLines
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(ShipmentLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<ShipmentLineDto>(x)).ToList();

                var pagedResponse = new PagedResponse<ShipmentLineDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<ShipmentLineDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("ShipmentLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<ShipmentLineDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<ShipmentLineDto>> CreateAsync(CreateShipmentLineDto dto)
        {
            try
            {
                NormalizePricing(dto);
                var entity = _mapper.Map<ShipmentLine>(dto);
                await _unitOfWork.ShipmentLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<ShipmentLineDto>(entity);
                return ApiResponse<ShipmentLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("ShipmentLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<ShipmentLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<ShipmentLineDto>> UpdateAsync(long id, UpdateShipmentLineDto dto)
        {
            try
            {
                NormalizePricing(dto);
                var repo = _unitOfWork.ShipmentLines;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<ShipmentLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<ShipmentLineDto>(entity);
                return ApiResponse<ShipmentLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("ShipmentLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<ShipmentLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.ShipmentLines;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("ShipmentLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
