using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class BatchMovementService : IBatchMovementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public BatchMovementService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<BatchMovementDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.BatchMovements
                    .Query()
                    .Include(x => x.FishBatch)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.FishBatch)
                        .ThenInclude(x => x!.FishStock)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<BatchMovementDto>.ErrorResult(
                        _localizationService.GetLocalizedString("BatchMovementService.NotFound"),
                        _localizationService.GetLocalizedString("BatchMovementService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = await MapMovementAsync(new List<BatchMovement> { entity });
                return ApiResponse<BatchMovementDto>.SuccessResult(dto, _localizationService.GetLocalizedString("BatchMovementService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchMovementDto>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchMovementService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<BatchMovementDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.BatchMovements
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(BatchMovement.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .Include(x => x.FishBatch)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.FishBatch)
                        .ThenInclude(x => x!.FishStock)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .ToListAsync();

                var items = await MapMovementsAsync(entities);

                var pagedResponse = new PagedResponse<BatchMovementDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<BatchMovementDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("BatchMovementService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<BatchMovementDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchMovementService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BatchMovementDto>> CreateAsync(CreateBatchMovementDto dto)
        {
            try
            {
                var entity = _mapper.Map<BatchMovement>(dto);
                await _unitOfWork.BatchMovements.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<BatchMovementDto>(entity);
                return ApiResponse<BatchMovementDto>.SuccessResult(result, _localizationService.GetLocalizedString("BatchMovementService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchMovementDto>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchMovementService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BatchMovementDto>> UpdateAsync(long id, UpdateBatchMovementDto dto)
        {
            try
            {
                var repo = _unitOfWork.BatchMovements;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<BatchMovementDto>.ErrorResult(
                        _localizationService.GetLocalizedString("BatchMovementService.NotFound"),
                        _localizationService.GetLocalizedString("BatchMovementService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<BatchMovementDto>(entity);
                return ApiResponse<BatchMovementDto>.SuccessResult(result, _localizationService.GetLocalizedString("BatchMovementService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchMovementDto>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchMovementService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.BatchMovements;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("BatchMovementService.NotFound"),
                        _localizationService.GetLocalizedString("BatchMovementService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("BatchMovementService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchMovementService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<BatchMovementDto> MapMovementAsync(List<BatchMovement> entities)
        {
            var items = await MapMovementsAsync(entities);
            return items[0];
        }

        private async Task<List<BatchMovementDto>> MapMovementsAsync(List<BatchMovement> entities)
        {
            var cageIds = entities
                .SelectMany(x => new long?[] { x.ProjectCageId, x.FromProjectCageId, x.ToProjectCageId })
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var stockIds = entities
                .SelectMany(x => new long?[] { x.FromStockId, x.ToStockId })
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var transferIds = entities.Where(x => x.ReferenceTable == "Transfer").Select(x => x.ReferenceId).Distinct().ToList();
            var shipmentIds = entities.Where(x => x.ReferenceTable == "Shipment").Select(x => x.ReferenceId).Distinct().ToList();
            var weighingIds = entities.Where(x => x.ReferenceTable == "Weighing").Select(x => x.ReferenceId).Distinct().ToList();
            var feedingIds = entities.Where(x => x.ReferenceTable == "Feeding").Select(x => x.ReferenceId).Distinct().ToList();
            var mortalityIds = entities.Where(x => x.ReferenceTable == "Mortality").Select(x => x.ReferenceId).Distinct().ToList();
            var stockConvertIds = entities.Where(x => x.ReferenceTable == "StockConvert").Select(x => x.ReferenceId).Distinct().ToList();
            var goodsReceiptIds = entities.Where(x => x.ReferenceTable == "GoodsReceipt").Select(x => x.ReferenceId).Distinct().ToList();

            var projectCages = cageIds.Count == 0
                ? new List<ProjectCage>()
                : await _unitOfWork.Db.ProjectCages
                    .Where(x => cageIds.Contains(x.Id))
                    .Include(x => x.Cage)
                    .ToListAsync();

            var stocks = stockIds.Count == 0
                ? new List<StockEntity>()
                : await _unitOfWork.Db.Stocks
                    .Where(x => stockIds.Contains(x.Id))
                    .ToListAsync();

            var transfers = transferIds.Count == 0
                ? new List<Transfer>()
                : await _unitOfWork.Db.Transfers.Where(x => transferIds.Contains(x.Id)).ToListAsync();
            var shipments = shipmentIds.Count == 0
                ? new List<Shipment>()
                : await _unitOfWork.Db.Shipments.Where(x => shipmentIds.Contains(x.Id)).ToListAsync();
            var weighings = weighingIds.Count == 0
                ? new List<Weighing>()
                : await _unitOfWork.Db.Weighings.Where(x => weighingIds.Contains(x.Id)).ToListAsync();
            var feedings = feedingIds.Count == 0
                ? new List<Feeding>()
                : await _unitOfWork.Db.Feedings.Where(x => feedingIds.Contains(x.Id)).ToListAsync();
            var mortalities = mortalityIds.Count == 0
                ? new List<Mortality>()
                : await _unitOfWork.Db.Mortalities.Where(x => mortalityIds.Contains(x.Id)).ToListAsync();
            var stockConverts = stockConvertIds.Count == 0
                ? new List<StockConvert>()
                : await _unitOfWork.Db.StockConverts.Where(x => stockConvertIds.Contains(x.Id)).ToListAsync();
            var goodsReceipts = goodsReceiptIds.Count == 0
                ? new List<GoodsReceipt>()
                : await _unitOfWork.Db.GoodsReceipts.Where(x => goodsReceiptIds.Contains(x.Id)).ToListAsync();

            var projectCageById = projectCages.ToDictionary(x => x.Id);
            var stockById = stocks.ToDictionary(x => x.Id);
            var transferById = transfers.ToDictionary(x => x.Id, x => x.TransferNo);
            var shipmentById = shipments.ToDictionary(x => x.Id, x => x.ShipmentNo);
            var weighingById = weighings.ToDictionary(x => x.Id, x => x.WeighingNo);
            var feedingById = feedings.ToDictionary(x => x.Id, x => x.FeedingNo);
            var mortalityById = mortalities.ToDictionary(x => x.Id, x => x.Id.ToString());
            var stockConvertById = stockConverts.ToDictionary(x => x.Id, x => x.ConvertNo);
            var goodsReceiptById = goodsReceipts.ToDictionary(x => x.Id, x => x.ReceiptNo);

            return entities.Select(entity =>
            {
                var fishBatch = entity.FishBatch;
                var project = fishBatch?.Project;
                var fishStock = fishBatch?.FishStock;
                projectCageById.TryGetValue(entity.ProjectCageId ?? 0, out var projectCage);
                projectCageById.TryGetValue(entity.FromProjectCageId ?? 0, out var fromProjectCage);
                projectCageById.TryGetValue(entity.ToProjectCageId ?? 0, out var toProjectCage);
                stockById.TryGetValue(entity.FromStockId ?? 0, out var fromStock);
                stockById.TryGetValue(entity.ToStockId ?? 0, out var toStock);

                return new BatchMovementDto
                {
                    Id = entity.Id,
                    FishBatchId = entity.FishBatchId,
                    BatchCode = fishBatch?.BatchCode,
                    ProjectId = fishBatch?.ProjectId,
                    ProjectCode = project?.ProjectCode,
                    ProjectName = project?.ProjectName,
                    FishStockId = fishBatch?.FishStockId,
                    FishStockCode = fishStock?.ErpStockCode,
                    FishStockName = fishStock?.StockName,
                    ProjectCageId = entity.ProjectCageId,
                    ProjectCageCode = projectCage?.Cage?.CageCode,
                    ProjectCageName = projectCage?.Cage?.CageName,
                    FromProjectCageId = entity.FromProjectCageId,
                    FromProjectCageCode = fromProjectCage?.Cage?.CageCode,
                    FromProjectCageName = fromProjectCage?.Cage?.CageName,
                    ToProjectCageId = entity.ToProjectCageId,
                    ToProjectCageCode = toProjectCage?.Cage?.CageCode,
                    ToProjectCageName = toProjectCage?.Cage?.CageName,
                    FromStockId = entity.FromStockId,
                    FromStockCode = fromStock?.ErpStockCode,
                    FromStockName = fromStock?.StockName,
                    ToStockId = entity.ToStockId,
                    ToStockCode = toStock?.ErpStockCode,
                    ToStockName = toStock?.StockName,
                    FromAverageGram = entity.FromAverageGram,
                    ToAverageGram = entity.ToAverageGram,
                    MovementDate = entity.MovementDate,
                    MovementType = entity.MovementType,
                    MovementTypeName = entity.MovementType.ToString(),
                    SignedCount = entity.SignedCount,
                    SignedBiomassGram = entity.SignedBiomassGram,
                    FeedGram = entity.FeedGram,
                    ActorUserId = entity.ActorUserId,
                    ReferenceTable = entity.ReferenceTable,
                    ReferenceId = entity.ReferenceId,
                    ReferenceDocumentNo = ResolveReferenceDocumentNo(
                        entity.ReferenceTable,
                        entity.ReferenceId,
                        transferById,
                        shipmentById,
                        weighingById,
                        feedingById,
                        mortalityById,
                        stockConvertById,
                        goodsReceiptById),
                    Note = entity.Note,
                };
            }).ToList();
        }

        private static string? ResolveReferenceDocumentNo(
            string referenceTable,
            long referenceId,
            Dictionary<long, string> transferById,
            Dictionary<long, string> shipmentById,
            Dictionary<long, string> weighingById,
            Dictionary<long, string> feedingById,
            Dictionary<long, string> mortalityById,
            Dictionary<long, string> stockConvertById,
            Dictionary<long, string> goodsReceiptById)
        {
            return referenceTable switch
            {
                "Transfer" => transferById.GetValueOrDefault(referenceId),
                "Shipment" => shipmentById.GetValueOrDefault(referenceId),
                "Weighing" => weighingById.GetValueOrDefault(referenceId),
                "Feeding" => feedingById.GetValueOrDefault(referenceId),
                "Mortality" => mortalityById.GetValueOrDefault(referenceId),
                "StockConvert" => stockConvertById.GetValueOrDefault(referenceId),
                "GoodsReceipt" => goodsReceiptById.GetValueOrDefault(referenceId),
                _ => null,
            };
        }
    }
}
