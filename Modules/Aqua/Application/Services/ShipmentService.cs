using AutoMapper;
using aqua_api.Shared.Infrastructure.Time;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public ShipmentService(
            IUnitOfWork unitOfWork,
            IShipmentRepository shipmentRepository,
            IBalanceLedgerManager balanceLedgerManager,
            IMapper mapper,
            ILocalizationService localizationService,
            IErpService erpService)
        {
            _unitOfWork = unitOfWork;
            _shipmentRepository = shipmentRepository;
            _balanceLedgerManager = balanceLedgerManager;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<ShipmentDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Shipments
                    .Query()
                    .Include(x => x.Project)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<ShipmentDto>.ErrorResult(
                        _localizationService.GetLocalizedString("ShipmentService.NotFound"),
                        _localizationService.GetLocalizedString("ShipmentService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = await MapShipmentDtoAsync(entity);
                return ApiResponse<ShipmentDto>.SuccessResult(dto, _localizationService.GetLocalizedString("ShipmentService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<ShipmentDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<ShipmentDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Shipments
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.Project)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(Shipment.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = new List<ShipmentDto>(entities.Count);
                foreach (var entity in entities)
                {
                    items.Add(await MapShipmentDtoAsync(entity));
                }

                var pagedResponse = new PagedResponse<ShipmentDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<ShipmentDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("ShipmentService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<ShipmentDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<ShipmentDto>> CreateAsync(CreateShipmentDto dto)
        {
            try
            {
                if (dto.TargetWarehouseId.HasValue)
                {
                    dto.TargetWarehouseId = await ValidateAndResolveWarehouseIdAsync(dto.TargetWarehouseId.Value);
                }

                var entity = _mapper.Map<Shipment>(dto);
                await _unitOfWork.Shipments.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = await MapShipmentDtoAsync(entity);
                return ApiResponse<ShipmentDto>.SuccessResult(result, _localizationService.GetLocalizedString("ShipmentService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<ShipmentDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<ShipmentDto>> UpdateAsync(long id, UpdateShipmentDto dto)
        {
            try
            {
                var repo = _unitOfWork.Shipments;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<ShipmentDto>.ErrorResult(
                        _localizationService.GetLocalizedString("ShipmentService.NotFound"),
                        _localizationService.GetLocalizedString("ShipmentService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                if (dto.TargetWarehouseId.HasValue)
                {
                    dto.TargetWarehouseId = await ValidateAndResolveWarehouseIdAsync(dto.TargetWarehouseId.Value);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = await MapShipmentDtoAsync(entity);
                return ApiResponse<ShipmentDto>.SuccessResult(result, _localizationService.GetLocalizedString("ShipmentService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<ShipmentDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.Shipments;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("ShipmentService.NotFound"),
                        _localizationService.GetLocalizedString("ShipmentService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("ShipmentService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> Post(long shipmentId, long userId)
        {
            try
            {
                await _unitOfWork.BeginTransaction();

                var shipment = await _shipmentRepository.GetForPost(shipmentId)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("ShipmentService.ShipmentNotFound"));

                EnsureDraftStatus(shipment.Status, nameof(Shipment));
                var lines = shipment.Lines.Where(x => !x.IsDeleted).ToList();
                if (!lines.Any())
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ShipmentService.MustContainLines"));

                var sourceProjectCageIds = new HashSet<long>();
                foreach (var line in lines)
                {
                    sourceProjectCageIds.Add(line.FromProjectCageId);

                    await _balanceLedgerManager.ApplyDelta(
                        shipment.ProjectId,
                        line.FishBatchId,
                        line.FromProjectCageId,
                        -line.FishCount,
                        -line.BiomassGram,
                        BatchMovementType.Shipment,
                        shipment.ShipmentDate,
                        "Shipment out to cold storage",
                        "RII_Shipment",
                        shipment.Id,
                        line.FromProjectCageId,
                        null,
                        null,
                        null,
                        line.AverageGram,
                        null,
                        userId);

                    if (shipment.TargetWarehouseId.HasValue)
                    {
                        await _balanceLedgerManager.ApplyWarehouseDelta(
                            shipment.ProjectId,
                            line.FishBatchId,
                            shipment.TargetWarehouseId.Value,
                            line.FishCount,
                            line.BiomassGram,
                            BatchMovementType.Shipment,
                            shipment.ShipmentDate,
                            "Shipment in to warehouse",
                            "RII_Shipment",
                            shipment.Id,
                            null,
                            shipment.TargetWarehouseId,
                            null,
                            null,
                            null,
                            line.AverageGram,
                            userId);
                    }
                }

                // Persist balance deltas first so release/close checks query the latest DB state.
                await _unitOfWork.SaveChanges();

                await ReleaseEmptySourceCages(sourceProjectCageIds, shipment.ShipmentDate, userId);

                shipment.Status = DocumentStatus.Posted;
                shipment.UpdatedBy = userId;
                shipment.UpdatedDate = DateTimeProvider.UtcNow;

                await TryCloseProjectIfFullyShipped(shipment.ProjectId, shipment.ShipmentDate, userId);

                await _unitOfWork.SaveChanges();
                await _unitOfWork.Commit();

                return ApiResponse<bool>.SuccessResult(
                    true,
                    _localizationService.GetLocalizedString("ShipmentService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentService.BusinessRuleError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("ShipmentService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task ReleaseEmptySourceCages(IEnumerable<long> sourceProjectCageIds, DateTime shipmentDate, long userId)
        {
            var sourceIds = sourceProjectCageIds.Distinct().ToList();
            if (sourceIds.Count == 0)
            {
                return;
            }

            var sourceCages = await _unitOfWork.Db.ProjectCages
                .Where(x => sourceIds.Contains(x.Id) && !x.IsDeleted && x.ReleasedDate == null)
                .ToListAsync();

            foreach (var sourceCage in sourceCages)
            {
                var hasLiveBalance = await _unitOfWork.Db.BatchCageBalances
                    .AnyAsync(x => x.ProjectCageId == sourceCage.Id && !x.IsDeleted && x.LiveCount > 0);
                if (hasLiveBalance)
                {
                    continue;
                }

                var releaseDate = shipmentDate.Date < sourceCage.AssignedDate.Date
                    ? sourceCage.AssignedDate.Date
                    : shipmentDate.Date;
                sourceCage.ReleasedDate = releaseDate;
                sourceCage.UpdatedBy = userId;
                sourceCage.UpdatedDate = DateTimeProvider.UtcNow;
            }
        }

        private async Task TryCloseProjectIfFullyShipped(long projectId, DateTime shipmentDate, long userId)
        {
            var hasLiveBalanceInProject = await _unitOfWork.Db.BatchCageBalances
                .Where(x => !x.IsDeleted && x.LiveCount > 0)
                .Join(
                    _unitOfWork.Db.ProjectCages.Where(pc => !pc.IsDeleted),
                    balance => balance.ProjectCageId,
                    projectCage => projectCage.Id,
                    (balance, projectCage) => new { balance, projectCage })
                .AnyAsync(x => x.projectCage.ProjectId == projectId);

            if (hasLiveBalanceInProject)
            {
                return;
            }

            var project = await _unitOfWork.Db.Projects
                .FirstOrDefaultAsync(x => x.Id == projectId && !x.IsDeleted);
            if (project == null)
            {
                return;
            }

            project.EndDate = shipmentDate.Date;
            project.Status = DocumentStatus.Cancelled;
            project.UpdatedBy = userId;
            project.UpdatedDate = DateTimeProvider.UtcNow;

            var activeProjectCages = await _unitOfWork.Db.ProjectCages
                .Where(x => x.ProjectId == projectId && !x.IsDeleted && x.ReleasedDate == null)
                .ToListAsync();

            foreach (var cage in activeProjectCages)
            {
                var releaseDate = shipmentDate.Date < cage.AssignedDate.Date
                    ? cage.AssignedDate.Date
                    : shipmentDate.Date;
                cage.ReleasedDate = releaseDate;
                cage.UpdatedBy = userId;
                cage.UpdatedDate = DateTimeProvider.UtcNow;
            }
        }

        private void EnsureDraftStatus(DocumentStatus status, string documentName)
        {
            if (status != DocumentStatus.Draft)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("General.DocumentMustBeDraftBeforePosting", documentName));
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

        private async Task<ShipmentDto> MapShipmentDtoAsync(Shipment entity)
        {
            var dto = _mapper.Map<ShipmentDto>(entity);

            if (!dto.TargetWarehouseId.HasValue)
            {
                return dto;
            }

            var warehouse = await _unitOfWork.Repository<WarehouseEntity>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.Id == dto.TargetWarehouseId.Value);

            dto.TargetWarehouseCode = warehouse?.ErpWarehouseCode;
            dto.TargetWarehouseName = warehouse?.WarehouseName;
            return dto;
        }
    }
}
