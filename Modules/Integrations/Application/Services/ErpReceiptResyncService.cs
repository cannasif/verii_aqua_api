using Microsoft.EntityFrameworkCore;
using aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces;

namespace aqua_api.Modules.Integrations.Application.Services
{
    public class ErpReceiptResyncService : IErpReceiptResyncService
    {
        private readonly AquaDbContext _db;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INetsisReadService _netsisReadService;
        private readonly IErpReceiptShipmentMovementSyncJob _syncJob;
        private readonly ILocalizationService _localizationService;

        public ErpReceiptResyncService(
            AquaDbContext db,
            IUnitOfWork unitOfWork,
            INetsisReadService netsisReadService,
            IErpReceiptShipmentMovementSyncJob syncJob,
            ILocalizationService localizationService)
        {
            _db = db;
            _unitOfWork = unitOfWork;
            _netsisReadService = netsisReadService;
            _syncJob = syncJob;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<ErpReceiptResyncPreviewDto>> PreviewAsync(string documentNo, string inOutCode, string operationType)
        {
            var normalizedDocumentNo = documentNo?.Trim();
            var normalizedInOutCode = inOutCode?.Trim().ToUpperInvariant();
            var normalizedOperationType = operationType?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedDocumentNo) ||
                string.IsNullOrWhiteSpace(normalizedOperationType) ||
                normalizedInOutCode != "G")
            {
                return ApiResponse<ErpReceiptResyncPreviewDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpReceiptResync.InvalidRequest"),
                    _localizationService.GetLocalizedString("ErpReceiptResync.InvalidRequest"),
                    StatusCodes.Status400BadRequest);
            }

            var sourceRows = await _db.ErpReceiptShipmentMovements
                .AsNoTracking()
                .Where(x => x.DocumentNo == normalizedDocumentNo &&
                    x.InOutCode == normalizedInOutCode &&
                    x.OperationType == normalizedOperationType &&
                    x.IsProcessed)
                .ToListAsync();

            if (sourceRows.Count == 0)
            {
                return ApiResponse<ErpReceiptResyncPreviewDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpReceiptResync.DocumentNotFound"),
                    _localizationService.GetLocalizedString("ErpReceiptResync.DocumentNotFound"),
                    StatusCodes.Status404NotFound);
            }

            var goodsReceiptLineIds = sourceRows
                .Where(x => x.GoodsReceiptLineId.HasValue)
                .Select(x => x.GoodsReceiptLineId!.Value)
                .Distinct()
                .ToList();
            var fishBatchIds = sourceRows
                .Where(x => x.FishBatchId.HasValue)
                .Select(x => x.FishBatchId!.Value)
                .Distinct()
                .ToList();

            var preview = new ErpReceiptResyncPreviewDto
            {
                DocumentNo = normalizedDocumentNo,
                InOutCode = normalizedInOutCode,
                OperationType = normalizedOperationType,
                SourceMovementCount = sourceRows.Count,
                GoodsReceiptLineCount = goodsReceiptLineIds.Count,
                FishBatchCount = fishBatchIds.Count
            };

            if (fishBatchIds.Count > 0)
            {
                await AddFeedingImpactsAsync(preview, fishBatchIds);
                await AddMortalityImpactsAsync(preview, fishBatchIds);
                await AddTransferImpactsAsync(preview, fishBatchIds);
                await AddShipmentImpactsAsync(preview, fishBatchIds);
                await AddWeighingImpactsAsync(preview, fishBatchIds);
                await AddStockConvertImpactsAsync(preview, fishBatchIds);
            }

            preview.RequiresErpReversal = preview.Impacts.Any(x => x.IsErpIntegrated);
            if (preview.RequiresErpReversal)
            {
                preview.BlockingReasons.Add(_localizationService.GetLocalizedString("ErpReceiptResync.ErpIntegratedDependency"));
            }

            preview.CanResync = preview.BlockingReasons.Count == 0;
            return ApiResponse<ErpReceiptResyncPreviewDto>.SuccessResult(
                preview,
                _localizationService.GetLocalizedString("ErpReceiptResync.PreviewLoaded"));
        }

        public async Task<ApiResponse<ErpReceiptResyncResultDto>> ResyncAsync(ErpReceiptResyncRequestDto request, long userId)
        {
            var documentNo = request.DocumentNo?.Trim();
            var inOutCode = request.InOutCode?.Trim().ToUpperInvariant();
            var operationType = request.OperationType?.Trim();
            if (string.IsNullOrWhiteSpace(documentNo) ||
                string.IsNullOrWhiteSpace(operationType) ||
                !string.Equals(documentNo, request.ConfirmationDocumentNo?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                inOutCode != "G")
            {
                return ResyncError("ErpReceiptResync.ConfirmationMismatch", StatusCodes.Status400BadRequest);
            }

            var previewResponse = await PreviewAsync(documentNo, inOutCode, operationType);
            if (!previewResponse.Success || previewResponse.Data == null)
            {
                return ApiResponse<ErpReceiptResyncResultDto>.ErrorResult(
                    previewResponse.Message,
                    previewResponse.ExceptionMessage,
                    previewResponse.StatusCode);
            }

            if (!previewResponse.Data.CanResync)
            {
                return ResyncError("ErpReceiptResync.Blocked", StatusCodes.Status409Conflict);
            }

            var originalMovementDate = await _db.ErpReceiptShipmentMovements
                .AsNoTracking()
                .Where(x => x.DocumentNo == documentNo &&
                    x.InOutCode == inOutCode &&
                    x.OperationType == operationType &&
                    x.IsProcessed)
                .Select(x => (DateTime?)x.MovementDate)
                .MinAsync();
            var erpResponse = await _netsisReadService.GetGoodsReceiptAndShipmentMovementsAsync(originalMovementDate?.Date);
            var currentErpRows = erpResponse.Data?
                .Where(x => string.Equals(x.FisNo?.Trim(), documentNo, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.GcKodu?.Trim(), inOutCode, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.IslemTuru?.Trim(), operationType, StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<MalKabulVeSevkiyatDto>();

            if (!erpResponse.Success || currentErpRows.Count == 0)
            {
                return ResyncError("ErpReceiptResync.CurrentErpDocumentNotFound", StatusCodes.Status409Conflict);
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var sourceRows = await _db.ErpReceiptShipmentMovements
                    .IgnoreQueryFilters()
                    .Where(x => !x.IsDeleted &&
                        x.DocumentNo == documentNo &&
                        x.InOutCode == inOutCode &&
                        x.OperationType == operationType &&
                        x.IsProcessed)
                    .ToListAsync();
                if (sourceRows.Count == 0)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ErpReceiptResync.DocumentChanged"));
                }
                var fishBatchIds = sourceRows.Where(x => x.FishBatchId.HasValue).Select(x => x.FishBatchId!.Value).Distinct().ToList();
                var goodsReceiptLineIds = sourceRows.Where(x => x.GoodsReceiptLineId.HasValue).Select(x => x.GoodsReceiptLineId!.Value).Distinct().ToList();
                var goodsReceiptIds = sourceRows.Where(x => x.GoodsReceiptId.HasValue).Select(x => x.GoodsReceiptId!.Value).Distinct().ToList();

                var reversedMovementCount = await ReverseBatchMovementsAsync(fishBatchIds, sourceRows[0].Id, userId);
                await SoftDeleteDependentOperationsAsync(fishBatchIds, userId);
                await SoftDeleteReceiptGraphAsync(goodsReceiptIds, goodsReceiptLineIds, fishBatchIds, userId);

                foreach (var sourceRow in sourceRows)
                {
                    MarkDeleted(sourceRow, userId);
                    sourceRow.IsProcessed = false;
                    sourceRow.ProcessedAt = null;
                }

                await _unitOfWork.SaveChangesAsync();

                foreach (var erpRow in currentErpRows)
                {
                    await _syncJob.ProcessMovementInCurrentTransactionAsync(erpRow);
                }

                await _unitOfWork.CommitTransactionAsync();
                return ApiResponse<ErpReceiptResyncResultDto>.SuccessResult(
                    new ErpReceiptResyncResultDto
                    {
                        DocumentNo = documentNo,
                        OperationType = operationType,
                        CancelledSourceMovementCount = sourceRows.Count,
                        ReversedLedgerMovementCount = reversedMovementCount,
                        ReprocessedSourceMovementCount = currentErpRows.Count
                    },
                    _localizationService.GetLocalizedString("ErpReceiptResync.Completed"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _db.ChangeTracker.Clear();
                return ApiResponse<ErpReceiptResyncResultDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ErpReceiptResync.Failed"),
                    ex.GetBaseException().Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<int> ReverseBatchMovementsAsync(List<long> fishBatchIds, long resyncReferenceId, long userId)
        {
            if (fishBatchIds.Count == 0) return 0;

            var movements = await _db.BatchMovements
                .Where(x => fishBatchIds.Contains(x.FishBatchId))
                .OrderByDescending(x => x.MovementDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            foreach (var movement in movements)
            {
                if (movement.ProjectCageId.HasValue)
                {
                    var balance = await _db.BatchCageBalances.FirstOrDefaultAsync(x =>
                        x.FishBatchId == movement.FishBatchId && x.ProjectCageId == movement.ProjectCageId.Value);
                    if (balance != null)
                    {
                        ApplyReverseBalance(balance, movement.SignedCount, movement.SignedBiomassGram, movement.MovementDate);
                    }
                }

                if (movement.WarehouseId.HasValue)
                {
                    var balance = await _db.BatchWarehouseBalances.FirstOrDefaultAsync(x =>
                        x.FishBatchId == movement.FishBatchId && x.WarehouseId == movement.WarehouseId.Value);
                    if (balance != null)
                    {
                        ApplyReverseBalance(balance, movement.SignedCount, movement.SignedBiomassGram, movement.MovementDate);
                    }
                }

                await _db.BatchMovements.AddAsync(new BatchMovement
                {
                    FishBatchId = movement.FishBatchId,
                    ProjectCageId = movement.ProjectCageId,
                    WarehouseId = movement.WarehouseId,
                    FromProjectCageId = movement.ToProjectCageId,
                    ToProjectCageId = movement.FromProjectCageId,
                    FromWarehouseId = movement.ToWarehouseId,
                    ToWarehouseId = movement.FromWarehouseId,
                    FromStockId = movement.ToStockId,
                    ToStockId = movement.FromStockId,
                    FromAverageGram = movement.ToAverageGram,
                    ToAverageGram = movement.FromAverageGram,
                    MovementDate = DateTimeProvider.Now,
                    MovementType = movement.MovementType,
                    SignedCount = -movement.SignedCount,
                    SignedBiomassGram = -movement.SignedBiomassGram,
                    FeedGram = movement.FeedGram.HasValue ? -movement.FeedGram.Value : null,
                    ActorUserId = userId,
                    ReferenceTable = "RII_ERP_RECEIPT_RESYNC",
                    ReferenceId = resyncReferenceId,
                    Note = $"ERP receipt resync reversal | originalMovementId={movement.Id}",
                    CreatedBy = userId
                });
            }

            return movements.Count;
        }

        private async Task SoftDeleteDependentOperationsAsync(List<long> fishBatchIds, long userId)
        {
            if (fishBatchIds.Count == 0) return;

            var feedingDistributions = await _db.FeedingDistributions
                .Where(x => fishBatchIds.Contains(x.FishBatchId)).ToListAsync();
            var feedingLineIds = feedingDistributions.Select(x => x.FeedingLineId).Distinct().ToList();
            feedingDistributions.ForEach(x => MarkDeleted(x, userId));

            var mortalityLines = await _db.MortalityLines.Where(x => fishBatchIds.Contains(x.FishBatchId)).ToListAsync();
            var mortalityIds = mortalityLines.Select(x => x.MortalityId).Distinct().ToList();
            mortalityLines.ForEach(x => MarkDeleted(x, userId));

            var transferLines = await _db.TransferLines.Where(x => fishBatchIds.Contains(x.FishBatchId)).ToListAsync();
            var transferIds = transferLines.Select(x => x.TransferId).Distinct().ToList();
            transferLines.ForEach(x => MarkDeleted(x, userId));

            var cageWarehouseLines = await _db.CageWarehouseTransferLines.Where(x => fishBatchIds.Contains(x.FishBatchId)).ToListAsync();
            var cageWarehouseIds = cageWarehouseLines.Select(x => x.CageWarehouseTransferId).Distinct().ToList();
            cageWarehouseLines.ForEach(x => MarkDeleted(x, userId));

            var warehouseCageLines = await _db.WarehouseCageTransferLines.Where(x => fishBatchIds.Contains(x.FishBatchId)).ToListAsync();
            var warehouseCageIds = warehouseCageLines.Select(x => x.WarehouseCageTransferId).Distinct().ToList();
            warehouseCageLines.ForEach(x => MarkDeleted(x, userId));

            var warehouseLines = await _db.WarehouseTransferLines.Where(x => fishBatchIds.Contains(x.FishBatchId)).ToListAsync();
            var warehouseIds = warehouseLines.Select(x => x.WarehouseTransferId).Distinct().ToList();
            warehouseLines.ForEach(x => MarkDeleted(x, userId));

            var shipmentLines = await _db.ShipmentLines.Where(x => fishBatchIds.Contains(x.FishBatchId)).ToListAsync();
            var shipmentIds = shipmentLines.Select(x => x.ShipmentId).Distinct().ToList();
            shipmentLines.ForEach(x => MarkDeleted(x, userId));

            var weighingLines = await _db.WeighingLines.Where(x => fishBatchIds.Contains(x.FishBatchId)).ToListAsync();
            var weighingIds = weighingLines.Select(x => x.WeighingId).Distinct().ToList();
            weighingLines.ForEach(x => MarkDeleted(x, userId));

            var convertLines = await _db.StockConvertLines
                .Where(x => fishBatchIds.Contains(x.FromFishBatchId) || fishBatchIds.Contains(x.ToFishBatchId)).ToListAsync();
            var convertIds = convertLines.Select(x => x.StockConvertId).Distinct().ToList();
            convertLines.ForEach(x => MarkDeleted(x, userId));

            await _unitOfWork.SaveChangesAsync();

            var feedingLines = await _db.FeedingLines.Where(x => feedingLineIds.Contains(x.Id) && !x.Distributions.Any()).ToListAsync();
            var feedingIds = feedingLines.Select(x => x.FeedingId).Distinct().ToList();
            feedingLines.ForEach(x => MarkDeleted(x, userId));
            await _unitOfWork.SaveChangesAsync();

            await SoftDeleteEmptyHeadersAsync(_db.Feedings, feedingIds, x => !x.Lines.Any(), userId);
            await SoftDeleteEmptyHeadersAsync(_db.Mortalities, mortalityIds, x => !x.Lines.Any(), userId);
            await SoftDeleteEmptyHeadersAsync(_db.Transfers, transferIds, x => !x.Lines.Any(), userId);
            await SoftDeleteEmptyHeadersAsync(_db.CageWarehouseTransfers, cageWarehouseIds, x => !x.Lines.Any(), userId);
            await SoftDeleteEmptyHeadersAsync(_db.WarehouseCageTransfers, warehouseCageIds, x => !x.Lines.Any(), userId);
            await SoftDeleteEmptyHeadersAsync(_db.WarehouseTransfers, warehouseIds, x => !x.Lines.Any(), userId);
            await SoftDeleteEmptyHeadersAsync(_db.Shipments, shipmentIds, x => !x.Lines.Any(), userId);
            await SoftDeleteEmptyHeadersAsync(_db.Weighings, weighingIds, x => !x.Lines.Any(), userId);
            await SoftDeleteEmptyHeadersAsync(_db.StockConverts, convertIds, x => !x.Lines.Any(), userId);
        }

        private async Task SoftDeleteReceiptGraphAsync(List<long> receiptIds, List<long> lineIds, List<long> fishBatchIds, long userId)
        {
            var distributions = await _db.GoodsReceiptFishDistributions.Where(x => lineIds.Contains(x.GoodsReceiptLineId)).ToListAsync();
            distributions.ForEach(x => MarkDeleted(x, userId));
            var lines = await _db.GoodsReceiptLines.Where(x => lineIds.Contains(x.Id)).ToListAsync();
            lines.ForEach(x => MarkDeleted(x, userId));

            // A single ERP document may contain lines from different operation types.
            // Persist selected line deletions first, then close only truly empty headers.
            await _unitOfWork.SaveChangesAsync();
            var receipts = await _db.GoodsReceipts
                .Where(x => receiptIds.Contains(x.Id) && !x.Lines.Any())
                .ToListAsync();
            receipts.ForEach(x => MarkDeleted(x, userId));
            var cageBalances = await _db.BatchCageBalances.Where(x => fishBatchIds.Contains(x.FishBatchId)).ToListAsync();
            cageBalances.ForEach(x => MarkDeleted(x, userId));
            var warehouseBalances = await _db.BatchWarehouseBalances.Where(x => fishBatchIds.Contains(x.FishBatchId)).ToListAsync();
            warehouseBalances.ForEach(x => MarkDeleted(x, userId));
            var batches = await _db.FishBatches.Where(x => fishBatchIds.Contains(x.Id)).ToListAsync();
            batches.ForEach(x => MarkDeleted(x, userId));
        }

        private async Task SoftDeleteEmptyHeadersAsync<T>(DbSet<T> set, List<long> ids, global::System.Linq.Expressions.Expression<Func<T, bool>> noActiveLines, long userId)
            where T : BaseEntity
        {
            if (ids.Count == 0) return;
            var headers = await set.Where(x => ids.Contains(x.Id)).Where(noActiveLines).ToListAsync();
            headers.ForEach(x => MarkDeleted(x, userId));
        }

        private void ApplyReverseBalance(BatchCageBalance balance, int signedCount, decimal signedBiomassGram, DateTime asOfDate)
        {
            balance.LiveCount -= signedCount;
            balance.BiomassGram -= signedBiomassGram;
            if (balance.LiveCount < 0 || balance.BiomassGram < 0)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("ErpReceiptResync.NegativeCageBalance"));
            balance.AverageGram = balance.LiveCount > 0 ? Math.Round(balance.BiomassGram / balance.LiveCount, 3) : 0;
            balance.AsOfDate = asOfDate;
        }

        private void ApplyReverseBalance(BatchWarehouseBalance balance, int signedCount, decimal signedBiomassGram, DateTime asOfDate)
        {
            balance.LiveCount -= signedCount;
            balance.BiomassGram -= signedBiomassGram;
            if (balance.LiveCount < 0 || balance.BiomassGram < 0)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("ErpReceiptResync.NegativeWarehouseBalance"));
            balance.AverageGram = balance.LiveCount > 0 ? Math.Round(balance.BiomassGram / balance.LiveCount, 3) : 0;
            balance.AsOfDate = asOfDate;
        }

        private static void MarkDeleted(BaseEntity entity, long userId)
        {
            entity.IsDeleted = true;
            entity.DeletedBy = userId;
            entity.DeletedDate = DateTimeProvider.Now;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTimeProvider.Now;
        }

        private ApiResponse<ErpReceiptResyncResultDto> ResyncError(string key, int statusCode)
        {
            var message = _localizationService.GetLocalizedString(key);
            return ApiResponse<ErpReceiptResyncResultDto>.ErrorResult(message, message, statusCode);
        }

        private async Task AddFeedingImpactsAsync(ErpReceiptResyncPreviewDto preview, List<long> fishBatchIds)
        {
            var rows = await _db.FeedingDistributions.AsNoTracking()
                .Where(x => fishBatchIds.Contains(x.FishBatchId))
                .Select(x => new ErpReceiptResyncImpactDto
                {
                    OperationType = "Feeding",
                    HeaderId = x.FeedingLine!.FeedingId,
                    DocumentNo = x.FeedingLine.Feeding!.FeedingNo,
                    OperationDate = x.FeedingLine.Feeding.FeedingDate,
                    ProjectId = x.FeedingLine.Feeding.ProjectId,
                    ProjectCode = x.ProjectCage!.Project!.ProjectCode,
                    ProjectCageId = x.ProjectCageId,
                    CageCode = x.ProjectCage.Cage!.CageCode,
                    FishBatchId = x.FishBatchId,
                    BatchCode = x.FishBatch!.BatchCode,
                    FeedKg = x.FeedGram / 1000m,
                    IsErpIntegrated = x.FeedingLine.Feeding.IsERPIntegrated,
                    ErpReferenceNumber = x.FeedingLine.Feeding.ERPReferenceNumber
                })
                .ToListAsync();
            preview.Impacts.AddRange(rows);
        }

        private async Task AddMortalityImpactsAsync(ErpReceiptResyncPreviewDto preview, List<long> fishBatchIds)
        {
            var rows = await _db.MortalityLines.AsNoTracking()
                .Where(x => fishBatchIds.Contains(x.FishBatchId))
                .Select(x => new ErpReceiptResyncImpactDto
                {
                    OperationType = "Mortality",
                    HeaderId = x.MortalityId,
                    DocumentNo = x.Mortality!.MortalityNo,
                    OperationDate = x.Mortality.MortalityDate,
                    ProjectId = x.Mortality.ProjectId,
                    ProjectCode = x.ProjectCage!.Project!.ProjectCode,
                    ProjectCageId = x.ProjectCageId,
                    CageCode = x.ProjectCage.Cage!.CageCode,
                    FishBatchId = x.FishBatchId,
                    BatchCode = x.FishBatch!.BatchCode,
                    FishCount = x.DeadCount,
                    IsErpIntegrated = x.Mortality.IsERPIntegrated,
                    ErpReferenceNumber = x.Mortality.ERPReferenceNumber
                })
                .ToListAsync();
            preview.Impacts.AddRange(rows);
        }

        private async Task AddTransferImpactsAsync(ErpReceiptResyncPreviewDto preview, List<long> fishBatchIds)
        {
            preview.Impacts.AddRange(await _db.TransferLines.AsNoTracking()
                .Where(x => fishBatchIds.Contains(x.FishBatchId))
                .Select(x => MapImpact("CageTransfer", x.TransferId, x.Transfer!.TransferNo, x.Transfer.TransferDate, x.Transfer.ProjectId, x.Transfer.Project!.ProjectCode, x.FromProjectCageId, x.FromProjectCage!.Cage!.CageCode, x.FishBatchId, x.FishBatch!.BatchCode, x.FishCount, x.BiomassGram))
                .ToListAsync());
            preview.Impacts.AddRange(await _db.CageWarehouseTransferLines.AsNoTracking()
                .Where(x => fishBatchIds.Contains(x.FishBatchId))
                .Select(x => MapImpact("CageWarehouseTransfer", x.CageWarehouseTransferId, x.CageWarehouseTransfer!.TransferNo, x.CageWarehouseTransfer.TransferDate, x.CageWarehouseTransfer.ProjectId, x.CageWarehouseTransfer.Project!.ProjectCode, x.FromProjectCageId, x.FromProjectCage!.Cage!.CageCode, x.FishBatchId, x.FishBatch!.BatchCode, x.FishCount, x.BiomassGram))
                .ToListAsync());
            preview.Impacts.AddRange(await _db.WarehouseCageTransferLines.AsNoTracking()
                .Where(x => fishBatchIds.Contains(x.FishBatchId))
                .Select(x => MapImpact("WarehouseCageTransfer", x.WarehouseCageTransferId, x.WarehouseCageTransfer!.TransferNo, x.WarehouseCageTransfer.TransferDate, x.WarehouseCageTransfer.ProjectId, x.WarehouseCageTransfer.Project!.ProjectCode, x.ToProjectCageId, x.ToProjectCage!.Cage!.CageCode, x.FishBatchId, x.FishBatch!.BatchCode, x.FishCount, x.BiomassGram))
                .ToListAsync());
            preview.Impacts.AddRange(await _db.WarehouseTransferLines.AsNoTracking()
                .Where(x => fishBatchIds.Contains(x.FishBatchId))
                .Select(x => MapImpact("WarehouseTransfer", x.WarehouseTransferId, x.WarehouseTransfer!.TransferNo, x.WarehouseTransfer.TransferDate, x.WarehouseTransfer.ProjectId, x.WarehouseTransfer.Project!.ProjectCode, null, null, x.FishBatchId, x.FishBatch!.BatchCode, x.FishCount, x.BiomassGram))
                .ToListAsync());
        }

        private async Task AddShipmentImpactsAsync(ErpReceiptResyncPreviewDto preview, List<long> fishBatchIds)
        {
            var rows = await _db.ShipmentLines.AsNoTracking()
                .Where(x => fishBatchIds.Contains(x.FishBatchId))
                .Select(x => new ErpReceiptResyncImpactDto
                {
                    OperationType = "Shipment",
                    HeaderId = x.ShipmentId,
                    DocumentNo = x.Shipment!.ShipmentNo,
                    OperationDate = x.Shipment.ShipmentDate,
                    ProjectId = x.Shipment.ProjectId,
                    ProjectCode = x.FromProjectCage!.Project!.ProjectCode,
                    ProjectCageId = x.FromProjectCageId,
                    CageCode = x.FromProjectCage.Cage!.CageCode,
                    FishBatchId = x.FishBatchId,
                    BatchCode = x.FishBatch!.BatchCode,
                    FishCount = x.FishCount,
                    BiomassKg = x.BiomassGram / 1000m,
                    IsErpIntegrated = x.Shipment.IsERPIntegrated,
                    ErpReferenceNumber = x.Shipment.ERPReferenceNumber
                })
                .ToListAsync();
            preview.Impacts.AddRange(rows);
        }

        private async Task AddWeighingImpactsAsync(ErpReceiptResyncPreviewDto preview, List<long> fishBatchIds)
        {
            preview.Impacts.AddRange(await _db.WeighingLines.AsNoTracking()
                .Where(x => fishBatchIds.Contains(x.FishBatchId))
                .Select(x => MapImpact("Weighing", x.WeighingId, x.Weighing!.WeighingNo, x.Weighing.WeighingDate, x.Weighing.ProjectId, x.Weighing.Project!.ProjectCode, x.ProjectCageId, x.ProjectCage!.Cage!.CageCode, x.FishBatchId, x.FishBatch!.BatchCode, x.MeasuredCount, x.MeasuredBiomassGram))
                .ToListAsync());
        }

        private async Task AddStockConvertImpactsAsync(ErpReceiptResyncPreviewDto preview, List<long> fishBatchIds)
        {
            preview.Impacts.AddRange(await _db.StockConvertLines.AsNoTracking()
                .Where(x => fishBatchIds.Contains(x.FromFishBatchId) || fishBatchIds.Contains(x.ToFishBatchId))
                .Select(x => MapImpact("StockConvert", x.StockConvertId, x.StockConvert!.ConvertNo, x.StockConvert.ConvertDate, x.StockConvert.ProjectId, x.StockConvert.Project!.ProjectCode, x.FromProjectCageId, x.FromProjectCage!.Cage!.CageCode, x.FromFishBatchId, x.FromFishBatch!.BatchCode, x.FishCount, x.BiomassGram))
                .ToListAsync());
        }

        private static ErpReceiptResyncImpactDto MapImpact(
            string operationType,
            long headerId,
            string documentNo,
            DateTime operationDate,
            long projectId,
            string? projectCode,
            long? projectCageId,
            string? cageCode,
            long fishBatchId,
            string? batchCode,
            int fishCount,
            decimal biomassGram)
        {
            return new ErpReceiptResyncImpactDto
            {
                OperationType = operationType,
                HeaderId = headerId,
                DocumentNo = documentNo,
                OperationDate = operationDate,
                ProjectId = projectId,
                ProjectCode = projectCode,
                ProjectCageId = projectCageId,
                CageCode = cageCode,
                FishBatchId = fishBatchId,
                BatchCode = batchCode,
                FishCount = fishCount,
                BiomassKg = biomassGram / 1000m
            };
        }
    }
}
