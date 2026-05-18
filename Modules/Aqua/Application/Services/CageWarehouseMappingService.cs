using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class CageWarehouseMappingService : ICageWarehouseMappingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public CageWarehouseMappingService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<CageWarehouseMappingDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await QueryWithIncludes()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return NotFound();
                }

                return ApiResponse<CageWarehouseMappingDto>.SuccessResult(
                    _mapper.Map<CageWarehouseMappingDto>(entity),
                    _localizationService.GetLocalizedString("CageWarehouseMappingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ServerError<CageWarehouseMappingDto>(ex);
            }
        }

        public async Task<ApiResponse<PagedResponse<CageWarehouseMappingDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = QueryWithIncludes()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(CageWarehouseMapping.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                return ApiResponse<PagedResponse<CageWarehouseMappingDto>>.SuccessResult(
                    new PagedResponse<CageWarehouseMappingDto>
                    {
                        Items = entities.Select(x => _mapper.Map<CageWarehouseMappingDto>(x)).ToList(),
                        TotalCount = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize
                    },
                    _localizationService.GetLocalizedString("CageWarehouseMappingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<CageWarehouseMappingDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseMappingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<CageWarehouseMappingDto>> CreateAsync(CreateCageWarehouseMappingDto dto)
        {
            try
            {
                var validation = await ValidateAsync(dto.CageId, dto.WarehouseId, null, dto.IsActive);
                if (!validation.Success)
                {
                    return validation;
                }

                var entity = _mapper.Map<CageWarehouseMapping>(dto);
                await _unitOfWork.Repository<CageWarehouseMapping>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return await GetByIdAsync(entity.Id);
            }
            catch (Exception ex)
            {
                return ServerError<CageWarehouseMappingDto>(ex);
            }
        }

        public async Task<ApiResponse<CageWarehouseMappingDto>> UpdateAsync(long id, UpdateCageWarehouseMappingDto dto)
        {
            try
            {
                var repo = _unitOfWork.Repository<CageWarehouseMapping>();
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return NotFound();
                }

                var validation = await ValidateAsync(dto.CageId, dto.WarehouseId, id, dto.IsActive);
                if (!validation.Success)
                {
                    return validation;
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return await GetByIdAsync(entity.Id);
            }
            catch (Exception ex)
            {
                return ServerError<CageWarehouseMappingDto>(ex);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var isDeleted = await _unitOfWork.Repository<CageWarehouseMapping>().SoftDeleteAsync(id);
                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("CageWarehouseMappingService.NotFound"),
                        _localizationService.GetLocalizedString("CageWarehouseMappingService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("CageWarehouseMappingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseMappingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private IQueryable<CageWarehouseMapping> QueryWithIncludes()
        {
            return _unitOfWork.Repository<CageWarehouseMapping>()
                .Query()
                .Include(x => x.Cage)
                .Include(x => x.Warehouse);
        }

        private async Task<ApiResponse<CageWarehouseMappingDto>> ValidateAsync(long cageId, long warehouseId, long? currentId, bool isActive)
        {
            var cageExists = await _unitOfWork.Cages
                .Query()
                .AnyAsync(x => x.Id == cageId && !x.IsDeleted);

            if (!cageExists)
            {
                return ApiResponse<CageWarehouseMappingDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseMappingService.CageNotFound"),
                    _localizationService.GetLocalizedString("CageWarehouseMappingService.CageNotFound"),
                    StatusCodes.Status400BadRequest);
            }

            var warehouseExists = await _unitOfWork.Repository<WarehouseEntity>()
                .Query()
                .AnyAsync(x => x.Id == warehouseId && !x.IsDeleted);

            if (!warehouseExists)
            {
                return ApiResponse<CageWarehouseMappingDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseMappingService.WarehouseNotFound"),
                    _localizationService.GetLocalizedString("CageWarehouseMappingService.WarehouseNotFound"),
                    StatusCodes.Status400BadRequest);
            }

            if (!isActive)
            {
                return ApiResponse<CageWarehouseMappingDto>.SuccessResult(new CageWarehouseMappingDto(), string.Empty);
            }

            var duplicateActiveCage = await _unitOfWork.Repository<CageWarehouseMapping>()
                .Query()
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.CageId == cageId &&
                    (!currentId.HasValue || x.Id != currentId.Value));

            if (duplicateActiveCage)
            {
                return ApiResponse<CageWarehouseMappingDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageWarehouseMappingService.ActiveMappingAlreadyExists"),
                    _localizationService.GetLocalizedString("CageWarehouseMappingService.ActiveMappingAlreadyExists"),
                    StatusCodes.Status409Conflict);
            }

            return ApiResponse<CageWarehouseMappingDto>.SuccessResult(new CageWarehouseMappingDto(), string.Empty);
        }

        private ApiResponse<CageWarehouseMappingDto> NotFound()
        {
            return ApiResponse<CageWarehouseMappingDto>.ErrorResult(
                _localizationService.GetLocalizedString("CageWarehouseMappingService.NotFound"),
                _localizationService.GetLocalizedString("CageWarehouseMappingService.NotFound"),
                StatusCodes.Status404NotFound);
        }

        private ApiResponse<T> ServerError<T>(Exception ex)
        {
            return ApiResponse<T>.ErrorResult(
                _localizationService.GetLocalizedString("CageWarehouseMappingService.InternalServerError"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }
}
