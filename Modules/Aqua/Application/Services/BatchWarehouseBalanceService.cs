using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using aqua_api.Modules.Stock.Domain.Entities;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class BatchWarehouseBalanceService : IBatchWarehouseBalanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public BatchWarehouseBalanceService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<BatchWarehouseBalanceDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.BatchWarehouseBalances
                    .Query()
                    .Include(x => x.Project)
                    .Include(x => x.FishBatch)
                        .ThenInclude(x => x!.FishStock)
                    .Include(x => x.Warehouse)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<BatchWarehouseBalanceDto>.ErrorResult(
                        _localizationService.GetLocalizedString("BatchWarehouseBalanceService.NotFound"),
                        _localizationService.GetLocalizedString("BatchWarehouseBalanceService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = MapBalance(entity);
                return ApiResponse<BatchWarehouseBalanceDto>.SuccessResult(
                    dto,
                    _localizationService.GetLocalizedString("BatchWarehouseBalanceService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchWarehouseBalanceDto>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchWarehouseBalanceService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<BatchWarehouseBalanceDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.BatchWarehouseBalances
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(BatchWarehouseBalance.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .Include(x => x.Project)
                    .Include(x => x.FishBatch)
                        .ThenInclude(x => x!.FishStock)
                    .Include(x => x.Warehouse)
                    .ToListAsync();

                var items = entities.Select(MapBalance).ToList();

                var pagedResponse = new PagedResponse<BatchWarehouseBalanceDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<BatchWarehouseBalanceDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("BatchWarehouseBalanceService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<BatchWarehouseBalanceDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchWarehouseBalanceService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BatchWarehouseBalanceDto>> CreateAsync(CreateBatchWarehouseBalanceDto dto)
        {
            try
            {
                var entity = _mapper.Map<BatchWarehouseBalance>(dto);
                await _unitOfWork.BatchWarehouseBalances.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var savedEntity = await _unitOfWork.BatchWarehouseBalances
                    .Query()
                    .Include(x => x.Project)
                    .Include(x => x.FishBatch)
                        .ThenInclude(x => x!.FishStock)
                    .Include(x => x.Warehouse)
                    .FirstAsync(x => x.Id == entity.Id);

                var result = MapBalance(savedEntity);
                return ApiResponse<BatchWarehouseBalanceDto>.SuccessResult(
                    result,
                    _localizationService.GetLocalizedString("BatchWarehouseBalanceService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchWarehouseBalanceDto>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchWarehouseBalanceService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BatchWarehouseBalanceDto>> UpdateAsync(long id, UpdateBatchWarehouseBalanceDto dto)
        {
            try
            {
                var repo = _unitOfWork.BatchWarehouseBalances;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<BatchWarehouseBalanceDto>.ErrorResult(
                        _localizationService.GetLocalizedString("BatchWarehouseBalanceService.NotFound"),
                        _localizationService.GetLocalizedString("BatchWarehouseBalanceService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var updatedEntity = await _unitOfWork.BatchWarehouseBalances
                    .Query()
                    .Include(x => x.Project)
                    .Include(x => x.FishBatch)
                        .ThenInclude(x => x!.FishStock)
                    .Include(x => x.Warehouse)
                    .FirstAsync(x => x.Id == entity.Id);

                var result = MapBalance(updatedEntity);
                return ApiResponse<BatchWarehouseBalanceDto>.SuccessResult(
                    result,
                    _localizationService.GetLocalizedString("BatchWarehouseBalanceService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchWarehouseBalanceDto>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchWarehouseBalanceService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.BatchWarehouseBalances;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("BatchWarehouseBalanceService.NotFound"),
                        _localizationService.GetLocalizedString("BatchWarehouseBalanceService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(
                    true,
                    _localizationService.GetLocalizedString("BatchWarehouseBalanceService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchWarehouseBalanceService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static BatchWarehouseBalanceDto MapBalance(BatchWarehouseBalance entity)
        {
            var project = entity.Project;
            var fishBatch = entity.FishBatch;
            var fishStock = fishBatch?.FishStock;
            var warehouse = entity.Warehouse;

            return new BatchWarehouseBalanceDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                ProjectCode = project?.ProjectCode,
                ProjectName = project?.ProjectName,
                FishBatchId = entity.FishBatchId,
                BatchCode = fishBatch?.BatchCode,
                FishStockId = fishBatch?.FishStockId,
                FishStockCode = fishStock?.ErpStockCode,
                FishStockName = fishStock?.StockName,
                WarehouseId = entity.WarehouseId,
                WarehouseCode = warehouse?.ErpWarehouseCode,
                WarehouseName = warehouse?.WarehouseName,
                WarehouseBranchCode = warehouse?.BranchCode,
                LiveCount = entity.LiveCount,
                AverageGram = entity.AverageGram,
                BiomassGram = entity.BiomassGram,
                AsOfDate = entity.AsOfDate,
            };
        }
    }
}
