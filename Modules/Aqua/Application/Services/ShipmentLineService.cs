using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using aqua_api.Shared.Common.Helpers;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

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
            ILocalizationService localizationService,
            IErpService erpService)
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

        public async Task<ApiResponse<ShipmentLineDto>> CreateWithAutoHeaderAsync(CreateShipmentLineWithAutoHeaderDto dto)
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
                    AverageGram = dto.AverageGram,
                    BiomassGram = dto.BiomassGram,
                    CurrencyCode = dto.CurrencyCode,
                    ExchangeRate = dto.ExchangeRate,
                    UnitPrice = dto.UnitPrice,
                    LocalUnitPrice = dto.LocalUnitPrice,
                    LineAmount = dto.LineAmount,
                    LocalLineAmount = dto.LocalLineAmount,
                };

                NormalizePricing(createDto);

                var entity = _mapper.Map<ShipmentLine>(createDto);
                await _unitOfWork.ShipmentLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var result = _mapper.Map<ShipmentLineDto>(entity);
                return ApiResponse<ShipmentLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("ShipmentLineService.OperationSuccessful"));
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
    }
}
