using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class WarehouseTransferLineService : IWarehouseTransferLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public WarehouseTransferLineService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILocalizationService localizationService,
            IErpService erpService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<WarehouseTransferLineDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Repository<WarehouseTransferLine>()
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<WarehouseTransferLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseTransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("WarehouseTransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                return ApiResponse<WarehouseTransferLineDto>.SuccessResult(
                    await MapWarehouseTransferLineDtoAsync(entity),
                    _localizationService.GetLocalizedString("WarehouseTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseTransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<WarehouseTransferLineDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Repository<WarehouseTransferLine>()
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(WarehouseTransferLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                var items = new List<WarehouseTransferLineDto>(entities.Count);
                foreach (var entity in entities)
                {
                    items.Add(await MapWarehouseTransferLineDtoAsync(entity));
                }

                return ApiResponse<PagedResponse<WarehouseTransferLineDto>>.SuccessResult(
                    new PagedResponse<WarehouseTransferLineDto>
                    {
                        Items = items,
                        TotalCount = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                    },
                    _localizationService.GetLocalizedString("WarehouseTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<WarehouseTransferLineDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WarehouseTransferLineDto>> CreateAsync(CreateWarehouseTransferLineDto dto)
        {
            try
            {
                await NormalizeAsync(dto.FromWarehouseId, dto.ToWarehouseId);

                var entity = _mapper.Map<WarehouseTransferLine>(dto);
                await _unitOfWork.Repository<WarehouseTransferLine>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<WarehouseTransferLineDto>.SuccessResult(
                    await MapWarehouseTransferLineDtoAsync(entity),
                    _localizationService.GetLocalizedString("WarehouseTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseTransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WarehouseTransferLineDto>> CreateWithAutoHeaderAsync(CreateWarehouseTransferLineWithAutoHeaderDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await NormalizeAsync(dto.FromWarehouseId, dto.ToWarehouseId);

                var header = await _unitOfWork.Repository<WarehouseTransfer>()
                    .Query()
                    .Where(x => !x.IsDeleted && x.ProjectId == dto.ProjectId && x.Status == DocumentStatus.Draft && x.TransferDate.Date == dto.TransferDate.Date)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (header == null)
                {
                    var project = await _unitOfWork.Projects.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.ProjectId && !x.IsDeleted);
                    if (project == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ApiResponse<WarehouseTransferLineDto>.ErrorResult(
                            _localizationService.GetLocalizedString("WarehouseTransferLineService.NotFound"),
                            "Project not found.",
                            StatusCodes.Status404NotFound);
                    }

                    header = new WarehouseTransfer
                    {
                        ProjectId = dto.ProjectId,
                        TransferDate = dto.TransferDate,
                        Status = DocumentStatus.Draft,
                        TransferNo = BuildDocumentNo(project.ProjectCode, project.ProjectName),
                    };

                    await _unitOfWork.Repository<WarehouseTransfer>().AddAsync(header);
                    await _unitOfWork.SaveChangesAsync();
                }

                var entity = new WarehouseTransferLine
                {
                    WarehouseTransferId = header.Id,
                    FishBatchId = dto.FishBatchId,
                    FromWarehouseId = dto.FromWarehouseId,
                    ToWarehouseId = dto.ToWarehouseId,
                    FishCount = dto.FishCount,
                    AverageGram = dto.AverageGram,
                    BiomassGram = dto.BiomassGram,
                };

                await _unitOfWork.Repository<WarehouseTransferLine>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<WarehouseTransferLineDto>.SuccessResult(
                    await MapWarehouseTransferLineDtoAsync(entity),
                    _localizationService.GetLocalizedString("WarehouseTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<WarehouseTransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WarehouseTransferLineDto>> UpdateAsync(long id, UpdateWarehouseTransferLineDto dto)
        {
            try
            {
                var repo = _unitOfWork.Repository<WarehouseTransferLine>();
                var entity = await repo.GetByIdForUpdateAsync(id);
                if (entity == null)
                {
                    return ApiResponse<WarehouseTransferLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseTransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("WarehouseTransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await NormalizeAsync(dto.FromWarehouseId, dto.ToWarehouseId);

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<WarehouseTransferLineDto>.SuccessResult(
                    await MapWarehouseTransferLineDtoAsync(entity),
                    _localizationService.GetLocalizedString("WarehouseTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseTransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var deleted = await _unitOfWork.Repository<WarehouseTransferLine>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseTransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("WarehouseTransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("WarehouseTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static string BuildDocumentNo(string? projectCode, string? projectName)
        {
            var baseValue = !string.IsNullOrWhiteSpace(projectCode) ? projectCode : projectName;
            var normalized = string.IsNullOrWhiteSpace(baseValue) ? "WHT" : baseValue.Trim();
            return $"{normalized}-W-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }

        private async Task NormalizeAsync(long fromWarehouseId, long toWarehouseId)
        {
            if (fromWarehouseId == toWarehouseId)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("WarehouseTransferLineService.SourceAndTargetWarehouseCannotBeSame"));
            }

            await EnsureWarehouseExistsAsync(fromWarehouseId);
            await EnsureWarehouseExistsAsync(toWarehouseId);
        }

        private async Task EnsureWarehouseExistsAsync(long warehouseId)
        {
            var exists = await _unitOfWork.Repository<WarehouseEntity>()
                .Query()
                .AnyAsync(x => !x.IsDeleted && x.Id == warehouseId);

            if (!exists)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("WarehouseTransferLineService.WarehouseNotFound"));
            }
        }

        private async Task<WarehouseTransferLineDto> MapWarehouseTransferLineDtoAsync(WarehouseTransferLine entity)
        {
            var dto = _mapper.Map<WarehouseTransferLineDto>(entity);
            var warehouseIds = new[] { entity.FromWarehouseId, entity.ToWarehouseId }
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (warehouseIds.Count == 0)
            {
                return dto;
            }

            var warehouses = await _unitOfWork.Repository<WarehouseEntity>()
                .Query()
                .AsNoTracking()
                .Where(x => !x.IsDeleted && warehouseIds.Contains(x.Id))
                .ToListAsync();

            var fromWarehouse = warehouses.FirstOrDefault(x => x.Id == entity.FromWarehouseId);
            var toWarehouse = warehouses.FirstOrDefault(x => x.Id == entity.ToWarehouseId);

            dto.FromWarehouseCode = fromWarehouse?.ErpWarehouseCode;
            dto.FromWarehouseName = fromWarehouse?.WarehouseName;
            dto.ToWarehouseCode = toWarehouse?.ErpWarehouseCode;
            dto.ToWarehouseName = toWarehouse?.WarehouseName;
            return dto;
        }
    }
}
