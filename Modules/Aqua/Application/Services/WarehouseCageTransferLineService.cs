using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class WarehouseCageTransferLineService : IWarehouseCageTransferLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public WarehouseCageTransferLineService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<WarehouseCageTransferLineDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Repository<WarehouseCageTransferLine>()
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<WarehouseCageTransferLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseCageTransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("WarehouseCageTransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                return ApiResponse<WarehouseCageTransferLineDto>.SuccessResult(
                    await MapDtoAsync(entity),
                    _localizationService.GetLocalizedString("WarehouseCageTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseCageTransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<WarehouseCageTransferLineDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Repository<WarehouseCageTransferLine>()
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(WarehouseCageTransferLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                var items = new List<WarehouseCageTransferLineDto>(entities.Count);
                foreach (var entity in entities)
                {
                    items.Add(await MapDtoAsync(entity));
                }

                return ApiResponse<PagedResponse<WarehouseCageTransferLineDto>>.SuccessResult(
                    new PagedResponse<WarehouseCageTransferLineDto>
                    {
                        Items = items,
                        TotalCount = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                    },
                    _localizationService.GetLocalizedString("WarehouseCageTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<WarehouseCageTransferLineDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WarehouseCageTransferLineDto>> CreateAsync(CreateWarehouseCageTransferLineDto dto)
        {
            try
            {
                await NormalizeAsync(dto.FromWarehouseId, dto.ToProjectCageId);

                var entity = _mapper.Map<WarehouseCageTransferLine>(dto);
                await _unitOfWork.Repository<WarehouseCageTransferLine>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<WarehouseCageTransferLineDto>.SuccessResult(
                    await MapDtoAsync(entity),
                    _localizationService.GetLocalizedString("WarehouseCageTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseCageTransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WarehouseCageTransferLineDto>> CreateWithAutoHeaderAsync(CreateWarehouseCageTransferLineWithAutoHeaderDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await NormalizeAsync(dto.FromWarehouseId, dto.ToProjectCageId);

                var header = await _unitOfWork.Repository<WarehouseCageTransfer>()
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
                        return ApiResponse<WarehouseCageTransferLineDto>.ErrorResult(
                            _localizationService.GetLocalizedString("WarehouseCageTransferLineService.NotFound"),
                            "Project not found.",
                            StatusCodes.Status404NotFound);
                    }

                    header = new WarehouseCageTransfer
                    {
                        ProjectId = dto.ProjectId,
                        TransferDate = dto.TransferDate,
                        Status = DocumentStatus.Draft,
                        TransferNo = BuildDocumentNo(project.ProjectCode, project.ProjectName),
                    };

                    await _unitOfWork.Repository<WarehouseCageTransfer>().AddAsync(header);
                    await _unitOfWork.SaveChangesAsync();
                }

                var entity = new WarehouseCageTransferLine
                {
                    WarehouseCageTransferId = header.Id,
                    FishBatchId = dto.FishBatchId,
                    FromWarehouseId = dto.FromWarehouseId,
                    ToProjectCageId = dto.ToProjectCageId,
                    FishCount = dto.FishCount,
                    AverageGram = dto.AverageGram,
                    BiomassGram = dto.BiomassGram,
                };

                await _unitOfWork.Repository<WarehouseCageTransferLine>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<WarehouseCageTransferLineDto>.SuccessResult(
                    await MapDtoAsync(entity),
                    _localizationService.GetLocalizedString("WarehouseCageTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<WarehouseCageTransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WarehouseCageTransferLineDto>> UpdateAsync(long id, UpdateWarehouseCageTransferLineDto dto)
        {
            try
            {
                var repo = _unitOfWork.Repository<WarehouseCageTransferLine>();
                var entity = await repo.GetByIdForUpdateAsync(id);
                if (entity == null)
                {
                    return ApiResponse<WarehouseCageTransferLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseCageTransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("WarehouseCageTransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await NormalizeAsync(dto.FromWarehouseId, dto.ToProjectCageId);

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<WarehouseCageTransferLineDto>.SuccessResult(
                    await MapDtoAsync(entity),
                    _localizationService.GetLocalizedString("WarehouseCageTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WarehouseCageTransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var deleted = await _unitOfWork.Repository<WarehouseCageTransferLine>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("WarehouseCageTransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("WarehouseCageTransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("WarehouseCageTransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WarehouseCageTransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static string BuildDocumentNo(string? projectCode, string? projectName)
        {
            var baseValue = !string.IsNullOrWhiteSpace(projectCode) ? projectCode : projectName;
            var normalized = string.IsNullOrWhiteSpace(baseValue) ? "WCT" : baseValue.Trim();
            return $"{normalized}-WC-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }

        private async Task NormalizeAsync(long fromWarehouseId, long toProjectCageId)
        {
            await EnsureWarehouseExistsAsync(fromWarehouseId);
            await EnsureProjectCageExistsAsync(toProjectCageId);
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

        private async Task EnsureProjectCageExistsAsync(long projectCageId)
        {
            var exists = await _unitOfWork.ProjectCages
                .Query()
                .AnyAsync(x => !x.IsDeleted && x.Id == projectCageId);

            if (!exists)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("TransferService.TargetProjectCageNotFound"));
            }
        }

        private async Task<WarehouseCageTransferLineDto> MapDtoAsync(WarehouseCageTransferLine entity)
        {
            var dto = _mapper.Map<WarehouseCageTransferLineDto>(entity);

            var warehouse = await _unitOfWork.Repository<WarehouseEntity>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == entity.FromWarehouseId);

            var targetProjectCage = await _unitOfWork.ProjectCages
                .Query()
                .AsNoTracking()
                .Include(x => x.Project)
                .Include(x => x.Cage)
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == entity.ToProjectCageId);

            dto.FromWarehouseCode = warehouse?.ErpWarehouseCode;
            dto.FromWarehouseName = warehouse?.WarehouseName;
            dto.ToProjectCode = targetProjectCage?.Project?.ProjectCode;
            dto.ToCageCode = targetProjectCage?.Cage?.CageCode;

            return dto;
        }
    }
}
