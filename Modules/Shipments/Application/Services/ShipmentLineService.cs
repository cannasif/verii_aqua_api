using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using aqua_api.Shared.Common.Helpers;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.Shipments.Application.Services
{
    public class ShipmentLineService : IShipmentLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly IFishGrowthLedgerReplayService _ledgerReplayService;
        private readonly IShipmentService _shipmentService;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public ShipmentLineService(
            IUnitOfWork unitOfWork,
            IBalanceLedgerManager balanceLedgerManager,
            IFishGrowthLedgerReplayService ledgerReplayService,
            IShipmentService shipmentService,
            IMapper mapper,
            ILocalizationService localizationService,
            IErpService erpService)
        {
            _unitOfWork = unitOfWork;
            _balanceLedgerManager = balanceLedgerManager;
            _ledgerReplayService = ledgerReplayService;
            _shipmentService = shipmentService;
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
                    .Include(x => x.Shipment)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.FishBatch)
                    .Include(x => x.FromProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<ShipmentLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var warehouse = entity.Shipment?.TargetWarehouseId is long warehouseId
                    ? await _unitOfWork.Repository<WarehouseEntity>().Query().FirstOrDefaultAsync(x => x.Id == warehouseId && !x.IsDeleted)
                    : null;
                var dto = MapShipmentLine(entity, warehouse);
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
                    .ApplySearch(request)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(ShipmentLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .Include(x => x.Shipment)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.FishBatch)
                    .Include(x => x.FromProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .ToListAsync();

                var warehouseIds = entities
                    .Select(x => x.Shipment?.TargetWarehouseId)
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .Distinct()
                    .ToList();

                var warehouses = warehouseIds.Count == 0
                    ? new List<WarehouseEntity>()
                    : await _unitOfWork.Repository<WarehouseEntity>()
                        .Query()
                        .Where(x => !x.IsDeleted && warehouseIds.Contains(x.Id))
                        .ToListAsync();

                var warehouseById = warehouses.ToDictionary(x => x.Id);
                var items = entities
                    .Select(x => MapShipmentLine(
                        x,
                        x.Shipment?.TargetWarehouseId is long warehouseId && warehouseById.TryGetValue(warehouseId, out var warehouse)
                            ? warehouse
                            : null))
                    .ToList();

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
                var shipment = await _unitOfWork.Shipments
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == dto.ShipmentId && !x.IsDeleted)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("ShipmentLineService.NotFound"));

                EnsureDraft(shipment.Status);
                await ApplyExitWeightSnapshotAsync(
                    dto,
                    shipment.ProjectId,
                    shipment.ShipmentDate);
                NormalizePricing(dto);
                var entity = _mapper.Map<ShipmentLine>(dto);
                await _unitOfWork.ShipmentLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<ShipmentLineDto>(entity);
                return ApiResponse<ShipmentLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("ShipmentLineService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<ShipmentLineDto>.ErrorResult(ex.Message, ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return ApiResponse<ShipmentLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public Task<ApiResponse<ShipmentLineDto>> CreateWithAutoHeaderAsync(CreateShipmentLineWithAutoHeaderDto dto)
        {
            return CreateWithAutoHeaderInternalAsync(dto, postUserId: null);
        }

        public Task<ApiResponse<ShipmentLineDto>> CreateWithAutoHeaderAndPostAsync(
            CreateShipmentLineWithAutoHeaderDto dto,
            long userId)
        {
            return CreateWithAutoHeaderInternalAsync(dto, userId);
        }

        private async Task<ApiResponse<ShipmentLineDto>> CreateWithAutoHeaderInternalAsync(
            CreateShipmentLineWithAutoHeaderDto dto,
            long? postUserId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var shipment = await _unitOfWork.Shipments
                    .Query(tracking: true)
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProjectId == dto.ProjectId &&
                        x.Status == DocumentStatus.Draft &&
                        x.ShipmentDate.Date == dto.ShipmentDate.Date)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (shipment == null)
                {
                    var project = await _unitOfWork.Projects
                        .Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == dto.ProjectId && !x.IsDeleted);

                    if (project == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ApiResponse<ShipmentLineDto>.ErrorResult(
                            _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                            "Project not found.",
                            StatusCodes.Status404NotFound);
                    }

                    if (dto.TargetWarehouseId.HasValue)
                    {
                        dto.TargetWarehouseId = await ValidateAndResolveWarehouseIdAsync(dto.TargetWarehouseId.Value);
                    }

                    shipment = new Shipment
                    {
                        ProjectId = dto.ProjectId,
                        ShipmentDate = dto.ShipmentDate,
                        Status = DocumentStatus.Draft,
                        ShipmentNo = BuildDocumentNo(project.ProjectCode, project.ProjectName),
                        TargetWarehouseId = dto.TargetWarehouseId,
                    };

                    await _unitOfWork.Shipments.AddAsync(shipment);
                    await _unitOfWork.SaveChangesAsync();
                }
                else if (dto.TargetWarehouseId.HasValue)
                {
                    dto.TargetWarehouseId = await ValidateAndResolveWarehouseIdAsync(dto.TargetWarehouseId.Value);
                    shipment.TargetWarehouseId = dto.TargetWarehouseId;
                    await _unitOfWork.Shipments.UpdateAsync(shipment);
                    await _unitOfWork.SaveChangesAsync();
                }

                var createDto = new CreateShipmentLineDto
                {
                    ShipmentId = shipment.Id,
                    FishBatchId = dto.FishBatchId,
                    FromProjectCageId = dto.FromProjectCageId,
                    FishCount = dto.FishCount,
                    TotalKg = dto.TotalKg,
                    CurrencyCode = dto.CurrencyCode,
                    ExchangeRate = dto.ExchangeRate,
                    UnitPrice = dto.UnitPrice,
                    LocalUnitPrice = dto.LocalUnitPrice,
                    LineAmount = dto.LineAmount,
                    LocalLineAmount = dto.LocalLineAmount,
                };

                await ApplyExitWeightSnapshotAsync(createDto, dto.ProjectId, dto.ShipmentDate);
                NormalizePricing(createDto);

                var entity = _mapper.Map<ShipmentLine>(createDto);
                await _unitOfWork.ShipmentLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                if (postUserId.HasValue)
                {
                    var postResult = await _shipmentService.PostWithinCurrentTransaction(shipment.Id, postUserId.Value);
                    if (!postResult.Success)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ApiResponse<ShipmentLineDto>.ErrorResult(
                            postResult.Message,
                            postResult.ExceptionMessage,
                            postResult.StatusCode);
                    }
                }

                await _unitOfWork.CommitTransactionAsync();

                var result = _mapper.Map<ShipmentLineDto>(entity);
                return ApiResponse<ShipmentLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("ShipmentLineService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<ShipmentLineDto>.ErrorResult(ex.Message, ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
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
                var repo = _unitOfWork.ShipmentLines;
                var entity = await repo.Query(tracking: true)
                    .Include(x => x.Shipment)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<ShipmentLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                EnsureDraft(entity.Shipment?.Status ?? DocumentStatus.Cancelled);
                await ApplyExitWeightSnapshotAsync(
                    dto,
                    entity.Shipment!.ProjectId,
                    entity.Shipment.ShipmentDate);
                NormalizePricing(dto);
                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<ShipmentLineDto>(entity);
                return ApiResponse<ShipmentLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("ShipmentLineService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<ShipmentLineDto>.ErrorResult(ex.Message, ex.Message, StatusCodes.Status400BadRequest);
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
                await _unitOfWork.BeginTransactionAsync();

                var repo = _unitOfWork.ShipmentLines;
                var entity = await repo.Query(tracking: true)
                    .Include(x => x.Shipment)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var shipment = entity.Shipment
                    ?? throw new InvalidOperationException(
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"));
                EnsureCanDelete(entity, shipment);

                var isPosted = shipment.Status == DocumentStatus.Posted;
                var userId = entity.UpdatedBy
                    ?? entity.CreatedBy
                    ?? shipment.UpdatedBy
                    ?? shipment.CreatedBy
                    ?? 1L;
                if (isPosted)
                {
                    await _ledgerReplayService.PrepareAsync(
                        shipment.ProjectId,
                        entity.FishBatchId,
                        entity.FromProjectCageId,
                        userId);
                }

                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        _localizationService.GetLocalizedString("ShipmentLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                if (isPosted)
                {
                    var now = DateTimeProvider.UtcNow;
                    var movements = await _unitOfWork.Db.BatchMovements
                        .Where(x =>
                            !x.IsDeleted
                            && x.FishBatchId == entity.FishBatchId
                            && x.MovementType == BatchMovementType.Shipment
                            && x.ReferenceTable == "RII_SHIPMENT_LINE"
                            && x.ReferenceId == entity.Id)
                        .ToListAsync();
                    foreach (var movement in movements)
                    {
                        movement.IsDeleted = true;
                        movement.DeletedBy = userId;
                        movement.DeletedDate = now;
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                if (isPosted)
                {
                    await _ledgerReplayService.ReplayAsync(
                        shipment.ProjectId,
                        entity.FishBatchId,
                        entity.FromProjectCageId,
                        shipment.ShipmentDate,
                        userId);

                    if (shipment.TargetWarehouseId.HasValue)
                    {
                        await _ledgerReplayService.RebuildWarehouseAndBatchAsync(
                            shipment.ProjectId,
                            entity.FishBatchId,
                            shipment.TargetWarehouseId.Value,
                            userId);
                    }

                    await ReopenSourceIfBalanceRestoredAsync(
                        shipment.ProjectId,
                        entity.FishBatchId,
                        entity.FromProjectCageId,
                        userId);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("ShipmentLineService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<bool>.ErrorResult(ex.Message, ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private void EnsureCanDelete(ShipmentLine line, Shipment shipment)
        {
            if (shipment.IsERPIntegrated || !string.IsNullOrWhiteSpace(line.ErpSourceMovementKey))
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("ShipmentLineService.ErpIntegratedCannotBeChanged"));
            }

            if (shipment.Status is not (DocumentStatus.Draft or DocumentStatus.Posted))
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("ShipmentLineService.PostedCannotBeChanged"));
            }
        }

        private async Task ReopenSourceIfBalanceRestoredAsync(
            long projectId,
            long fishBatchId,
            long projectCageId,
            long userId)
        {
            var hasLiveBalance = await _unitOfWork.Db.BatchCageBalances
                .AnyAsync(x =>
                    !x.IsDeleted
                    && x.FishBatchId == fishBatchId
                    && x.ProjectCageId == projectCageId
                    && x.LiveCount > 0);
            if (!hasLiveBalance)
            {
                return;
            }

            var now = DateTimeProvider.UtcNow;
            var projectCage = await _unitOfWork.Db.ProjectCages
                .FirstOrDefaultAsync(x => x.Id == projectCageId && !x.IsDeleted);
            if (projectCage?.ReleasedDate != null)
            {
                projectCage.ReleasedDate = null;
                projectCage.UpdatedBy = userId;
                projectCage.UpdatedDate = now;
            }

            var project = await _unitOfWork.Db.Projects
                .FirstOrDefaultAsync(x => x.Id == projectId && !x.IsDeleted);
            if (project?.Status == DocumentStatus.Cancelled)
            {
                project.Status = DocumentStatus.Posted;
                project.EndDate = null;
                project.UpdatedBy = userId;
                project.UpdatedDate = now;
            }
        }

        private static string BuildDocumentNo(string? projectCode, string? projectName)
        {
            var baseValue = !string.IsNullOrWhiteSpace(projectCode) ? projectCode : projectName;
            var normalized = string.IsNullOrWhiteSpace(baseValue) ? "DOC" : baseValue.Trim();
            return $"{normalized}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }

        private ShipmentLineDto MapShipmentLine(ShipmentLine entity, WarehouseEntity? warehouse)
        {
            var dto = _mapper.Map<ShipmentLineDto>(entity);
            dto.ShipmentNo = entity.Shipment?.ShipmentNo;
            dto.ProjectId = entity.Shipment?.ProjectId;
            dto.ProjectCode = entity.Shipment?.Project?.ProjectCode;
            dto.ProjectName = entity.Shipment?.Project?.ProjectName;
            dto.BatchCode = entity.FishBatch?.BatchCode;
            dto.FromCageCode = entity.FromProjectCage?.Cage?.CageCode;
            dto.FromCageName = entity.FromProjectCage?.Cage?.CageName;
            dto.TargetWarehouseId = entity.Shipment?.TargetWarehouseId;
            dto.TargetWarehouseCode = warehouse?.ErpWarehouseCode.ToString();
            dto.TargetWarehouseName = warehouse?.WarehouseName;
            return dto;
        }

        private async Task<long> ValidateAndResolveWarehouseIdAsync(long warehouseId)
        {
            var warehouseExists = await _unitOfWork.Repository<WarehouseEntity>()
                .Query()
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.Id == warehouseId);

            if (!warehouseExists)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("ShipmentService.WarehouseNotFound"));
            }

            return warehouseId;
        }

        private async Task ApplyExitWeightSnapshotAsync(
            CreateShipmentLineDto dto,
            long projectId,
            DateTime shipmentDate)
        {
            if (dto.FishCount <= 0)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("ShipmentLineService.FishCountMustBePositive"));
            }

            var projectCageExists = await _unitOfWork.Db.ProjectCages
                .AsNoTracking()
                .AnyAsync(x => x.Id == dto.FromProjectCageId && x.ProjectId == projectId && !x.IsDeleted);
            var fishBatchExists = await _unitOfWork.Db.FishBatches
                .AsNoTracking()
                .AnyAsync(x => x.Id == dto.FishBatchId && x.ProjectId == projectId && !x.IsDeleted);

            if (!projectCageExists || !fishBatchExists)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("ShipmentLineService.SourceNotFound"));
            }

            var sourceMass = await _balanceLedgerManager.GetCageMassSnapshotAsync(
                dto.FishBatchId,
                dto.FromProjectCageId,
                shipmentDate,
                BatchMovementType.Shipment);
            if (sourceMass.LiveCount < dto.FishCount)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("BalanceLedgerManager.BatchCageCountCannotGoNegative"));
            }

            if (dto.TotalKg.HasValue)
            {
                if (dto.TotalKg.Value <= 0m)
                {
                    throw new InvalidOperationException(
                        _localizationService.GetLocalizedString("ShipmentLineService.TotalKgMustBePositive"));
                }

                dto.TotalKg = RoundWeight(dto.TotalKg.Value);
                dto.BiomassGram = RoundWeight(dto.TotalKg.Value * 1000m);
                dto.AverageGram = RoundWeight(dto.BiomassGram / dto.FishCount);

                if (sourceMass.BiomassGram < dto.BiomassGram)
                {
                    throw new InvalidOperationException(
                        _localizationService.GetLocalizedString("BalanceLedgerManager.BatchCageBiomassCannotGoNegative"));
                }

                return;
            }

            if (sourceMass.AverageGram <= 0)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("ShipmentLineService.ExitWeightNotFound"));
            }

            dto.AverageGram = sourceMass.AverageGram;
            dto.BiomassGram = BatchMath.CalculateBiomassGram(dto.FishCount, dto.AverageGram);
        }

        private static decimal RoundWeight(decimal value) =>
            Math.Round(value, 8, MidpointRounding.AwayFromZero);

        private void EnsureDraft(DocumentStatus status)
        {
            if (status != DocumentStatus.Draft)
            {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("ShipmentLineService.PostedCannotBeChanged"));
            }
        }
    }
}
