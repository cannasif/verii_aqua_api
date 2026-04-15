using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.Warehouse.Application.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public WarehouseService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<PagedResponse<WarehouseDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Repository<WarehouseEntity>()
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(WarehouseEntity.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                return ApiResponse<PagedResponse<WarehouseDto>>.SuccessResult(
                    new PagedResponse<WarehouseDto>
                    {
                        Items = entities.Select(x => _mapper.Map<WarehouseDto>(x)).ToList(),
                        TotalCount = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize
                    },
                    _localizationService.GetLocalizedString("WarehouseService.WarehousesRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<WarehouseDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseService.InternalServerError"),
                    _localizationService.GetLocalizedString("WarehouseService.GetAllWarehousesExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WarehouseDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Repository<WarehouseEntity>()
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<WarehouseDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseService.WarehouseNotFound"),
                        _localizationService.GetLocalizedString("WarehouseService.WarehouseNotFound"),
                        StatusCodes.Status404NotFound);
                    }

                return ApiResponse<WarehouseDto>.SuccessResult(
                    _mapper.Map<WarehouseDto>(entity),
                    _localizationService.GetLocalizedString("WarehouseService.WarehouseRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseService.InternalServerError"),
                    _localizationService.GetLocalizedString("WarehouseService.GetWarehouseByIdExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
