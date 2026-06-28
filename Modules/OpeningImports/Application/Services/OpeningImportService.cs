using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using aqua_api.Shared.Common.Helpers;

namespace aqua_api.Modules.OpeningImports.Application.Services
{
public class OpeningImportService : IOpeningImportService
{
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly ILocalizationService _localizationService;

        private string L(string key, params object[] args) =>
            args.Length == 0
                ? _localizationService.GetLocalizedString(key)
                : _localizationService.GetLocalizedString(key, args);

        public OpeningImportService(
            IUnitOfWork unitOfWork,
            IBalanceLedgerManager balanceLedgerManager,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _balanceLedgerManager = balanceLedgerManager;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<OpeningImportPreviewResponseDto>> PreviewAsync(OpeningImportPreviewRequestDto dto)
        {
            try
            {
                if (dto.Sheets == null || dto.Sheets.Count == 0)
                {
                    return ApiResponse<OpeningImportPreviewResponseDto>.ErrorResult(
                        _localizationService.GetLocalizedString("OpeningImportService.EmptyWorkbook"),
                        _localizationService.GetLocalizedString("OpeningImportService.EmptyWorkbook"),
                        StatusCodes.Status400BadRequest);
                }

                var stagedRows = await ValidateRowsAsync(dto);

                var job = new OpeningImportJob
                {
                    FileName = string.IsNullOrWhiteSpace(dto.FileName) ? "opening-import.xlsx" : dto.FileName.Trim(),
                    SourceSystem = dto.SourceSystem?.Trim(),
                    Status = stagedRows.Any(x => x.Entity.Status == OpeningImportRowStatus.Error)
                        ? OpeningImportJobStatus.Failed
                        : OpeningImportJobStatus.Previewed,
                    MappingsJson = JsonSerializer.Serialize(dto.Sheets.Select(x => new
                    {
                        x.SheetName,
                        x.Mappings
                    }), JsonOptions),
                    PreviewedAt = DateTimeProvider.Now,
                    SummaryJson = JsonSerializer.Serialize(BuildSummary(stagedRows), JsonOptions),
                    Rows = stagedRows.Select(x => x.Entity).ToList()
                };

                await _unitOfWork.Repository<OpeningImportJob>().AddAsync(job);
                await _unitOfWork.SaveChangesAsync();

                foreach (var row in stagedRows)
                {
                    row.Entity.OpeningImportJobId = job.Id;
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<OpeningImportPreviewResponseDto>.SuccessResult(
                    BuildPreviewResponse(job, stagedRows.Select(x => x.Entity).ToList()),
                    _localizationService.GetLocalizedString("OpeningImportService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<OpeningImportPreviewResponseDto>.ErrorResult(
                    _localizationService.GetLocalizedString("OpeningImportService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<OpeningImportPreviewResponseDto>> GetByIdAsync(long id)
        {
            try
            {
                var job = await _unitOfWork.Db.Set<OpeningImportJob>()
                    .AsNoTracking()
                    .Include(x => x.Rows)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (job == null)
                {
                    return ApiResponse<OpeningImportPreviewResponseDto>.ErrorResult(
                        _localizationService.GetLocalizedString("OpeningImportService.JobNotFound"),
                        _localizationService.GetLocalizedString("OpeningImportService.JobNotFound"),
                        StatusCodes.Status404NotFound);
                }

                return ApiResponse<OpeningImportPreviewResponseDto>.SuccessResult(
                    BuildPreviewResponse(job, job.Rows.OrderBy(x => x.SheetName).ThenBy(x => x.RowNumber).ToList()),
                    _localizationService.GetLocalizedString("OpeningImportService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<OpeningImportPreviewResponseDto>.ErrorResult(
                    _localizationService.GetLocalizedString("OpeningImportService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<OpeningImportCommitResultDto>> CommitAsync(long id)
        {
            try
            {
                var job = await _unitOfWork.Db.Set<OpeningImportJob>()
                    .Include(x => x.Rows)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (job == null)
                {
                    return ApiResponse<OpeningImportCommitResultDto>.ErrorResult(
                        _localizationService.GetLocalizedString("OpeningImportService.JobNotFound"),
                        _localizationService.GetLocalizedString("OpeningImportService.JobNotFound"),
                        StatusCodes.Status404NotFound);
                }

                if (job.Status == OpeningImportJobStatus.Applied)
                {
                    return ApiResponse<OpeningImportCommitResultDto>.ErrorResult(
                        _localizationService.GetLocalizedString("OpeningImportService.AlreadyApplied"),
                        _localizationService.GetLocalizedString("OpeningImportService.AlreadyApplied"),
                        StatusCodes.Status400BadRequest);
                }

                if (HasOpeningGoodsReceiptHeaderConflicts(job.Rows))
                {
                    return ApiResponse<OpeningImportCommitResultDto>.ErrorResult(
                        L("OpeningImportService.GoodsReceiptHeaderConflict"),
                        L("OpeningImportService.GoodsReceiptHeaderConflict"),
                        StatusCodes.Status400BadRequest);
                }

                if (job.Rows.Any(x => x.Status == OpeningImportRowStatus.Error))
                {
                    return ApiResponse<OpeningImportCommitResultDto>.ErrorResult(
                        _localizationService.GetLocalizedString("OpeningImportService.CommitHasErrors"),
                        _localizationService.GetLocalizedString("OpeningImportService.CommitHasErrors"),
                        StatusCodes.Status400BadRequest);
                }

                await _unitOfWork.BeginTransactionAsync();

                var result = new OpeningImportCommitResultDto { JobId = id };
                var committedRows = job.Rows.ToList();
                var projectsByCode = await EnsureProjectsAsync(committedRows, result);
                var cagesByCode = await EnsureCagesAsync(committedRows, projectsByCode, result);
                await ApplyOpeningBalancesAsync(committedRows, projectsByCode, cagesByCode, result);
                await _unitOfWork.SaveChangesAsync();
                await CreateSummaryDocumentsAsync(committedRows, projectsByCode, cagesByCode, result);
                await _unitOfWork.SaveChangesAsync();
                await ApplyOpeningMortalityLedgersAsync(committedRows, projectsByCode, cagesByCode);

                job.Status = OpeningImportJobStatus.Applied;
                job.AppliedAt = DateTimeProvider.Now;
                job.SummaryJson = JsonSerializer.Serialize(result, JsonOptions);
                await _unitOfWork.Repository<OpeningImportJob>().UpdateAsync(job);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<OpeningImportCommitResultDto>.SuccessResult(
                    result,
                    _localizationService.GetLocalizedString("OpeningImportService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return ApiResponse<OpeningImportCommitResultDto>.ErrorResult(
                    _localizationService.GetLocalizedString("OpeningImportService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<OpeningImportCleanupSoftDeletedResultDto>> CleanupSoftDeletedReferencesAsync(long id)
        {
            try
            {
                var job = await _unitOfWork.Db.Set<OpeningImportJob>()
                    .Include(x => x.Rows)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (job == null)
                {
                    return ApiResponse<OpeningImportCleanupSoftDeletedResultDto>.ErrorResult(
                        _localizationService.GetLocalizedString("OpeningImportService.JobNotFound"),
                        _localizationService.GetLocalizedString("OpeningImportService.JobNotFound"),
                        StatusCodes.Status404NotFound);
                }

                if (job.Status == OpeningImportJobStatus.Applied)
                {
                    return ApiResponse<OpeningImportCleanupSoftDeletedResultDto>.ErrorResult(
                        _localizationService.GetLocalizedString("OpeningImportService.AlreadyApplied"),
                        _localizationService.GetLocalizedString("OpeningImportService.AlreadyApplied"),
                        StatusCodes.Status400BadRequest);
                }

                var normalizedRows = job.Rows.Select(ParseRow).ToList();
                var projectCodes = normalizedRows
                    .Select(x => x.TryGetValue("projectCode", out var projectCode) ? projectCode : null)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var cageCodes = normalizedRows
                    .Select(x => x.TryGetValue("cageCode", out var cageCode) ? cageCode : null)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var projects = await _unitOfWork.Projects.Query(tracking: true, ignoreQueryFilters: true)
                    .Where(x => x.IsDeleted && projectCodes.Contains(x.ProjectCode))
                    .ToListAsync();
                var cages = await _unitOfWork.Cages.Query(tracking: true, ignoreQueryFilters: true)
                    .Where(x => x.IsDeleted && cageCodes.Contains(x.CageCode))
                    .ToListAsync();
                var projectIds = projects.Select(x => x.Id).ToList();
                var cageIds = cages.Select(x => x.Id).ToList();

                var projectCages = await _unitOfWork.Db.ProjectCages
                    .IgnoreQueryFilters()
                    .Where(x =>
                        (projectIds.Count > 0 && projectIds.Contains(x.ProjectId)) ||
                        (cageIds.Count > 0 && cageIds.Contains(x.CageId)))
                    .ToListAsync();

                var cageWarehouseMappings = await _unitOfWork.Db.CageWarehouseMappings
                    .IgnoreQueryFilters()
                    .Where(x => cageIds.Count > 0 && cageIds.Contains(x.CageId))
                    .ToListAsync();

                var result = new OpeningImportCleanupSoftDeletedResultDto
                {
                    JobId = id,
                    DeletedProjectCodes = projects.Select(x => x.ProjectCode).ToList(),
                    DeletedCageCodes = cages.Select(x => x.CageCode).ToList()
                };

                if (projects.Count == 0 && cages.Count == 0 && projectCages.Count == 0 && cageWarehouseMappings.Count == 0)
                {
                    return ApiResponse<OpeningImportCleanupSoftDeletedResultDto>.SuccessResult(
                        result,
                        L("OpeningImportService.CleanupNoDeletedTestRecordFound"));
                }

                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.Db.RemoveRange(cageWarehouseMappings);
                _unitOfWork.Db.RemoveRange(projectCages);
                _unitOfWork.Db.RemoveRange(cages);
                _unitOfWork.Db.RemoveRange(projects);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                result.DeletedProjects = projects.Count;
                result.DeletedCages = cages.Count;
                result.DeletedProjectCages = projectCages.Count;
                result.DeletedCageWarehouseMappings = cageWarehouseMappings.Count;

                return ApiResponse<OpeningImportCleanupSoftDeletedResultDto>.SuccessResult(
                    result,
                    L("OpeningImportService.CleanupDeletedRecordsCleared"));
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return ApiResponse<OpeningImportCleanupSoftDeletedResultDto>.ErrorResult(
                    L("OpeningImportService.CleanupCouldNotClearAutomatically"),
                    L("OpeningImportService.CleanupHasRelations", ex.GetBaseException().Message),
                    StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return ApiResponse<OpeningImportCleanupSoftDeletedResultDto>.ErrorResult(
                    _localizationService.GetLocalizedString("OpeningImportService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<OpeningImportResetExistingDataResultDto>> ResetExistingDataAsync(long id)
        {
            try
            {
                var job = await _unitOfWork.Db.Set<OpeningImportJob>()
                    .Include(x => x.Rows)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (job == null)
                {
                    return ApiResponse<OpeningImportResetExistingDataResultDto>.ErrorResult(
                        _localizationService.GetLocalizedString("OpeningImportService.JobNotFound"),
                        _localizationService.GetLocalizedString("OpeningImportService.JobNotFound"),
                        StatusCodes.Status404NotFound);
                }

                var normalizedRows = job.Rows.Select(ParseRow).ToList();
                var projectCodes = normalizedRows
                    .Select(x => x.TryGetValue("projectCode", out var projectCode) ? projectCode : null)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var cageCodes = normalizedRows
                    .Select(x => x.TryGetValue("cageCode", out var cageCode) ? cageCode : null)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var projects = await _unitOfWork.Db.Projects
                    .IgnoreQueryFilters()
                    .Where(x => projectCodes.Contains(x.ProjectCode))
                    .ToListAsync();
                var cages = await _unitOfWork.Db.Cages
                    .IgnoreQueryFilters()
                    .Where(x => cageCodes.Contains(x.CageCode))
                    .ToListAsync();

                var result = new OpeningImportResetExistingDataResultDto
                {
                    JobId = id,
                    DeletedProjectCodes = projects.Select(x => x.ProjectCode).ToList(),
                    DeletedCageCodes = cages.Select(x => x.CageCode).ToList()
                };

                if (projects.Count == 0 && cages.Count == 0)
                {
                    return ApiResponse<OpeningImportResetExistingDataResultDto>.SuccessResult(
                        result,
                        L("OpeningImportService.ResetNoRecordsToClear"));
                }

                await _unitOfWork.BeginTransactionAsync();

                var projectIds = projects.Select(x => x.Id).ToList();
                var cageIds = cages.Select(x => x.Id).ToList();

                var projectCageIds = await _unitOfWork.Db.ProjectCages
                    .IgnoreQueryFilters()
                    .Where(x =>
                        (projectIds.Count > 0 && projectIds.Contains(x.ProjectId)) ||
                        (cageIds.Count > 0 && cageIds.Contains(x.CageId)))
                    .Select(x => x.Id)
                    .ToListAsync();

                var fishBatchIds = await _unitOfWork.Db.FishBatches
                    .IgnoreQueryFilters()
                    .Where(x => projectIds.Contains(x.ProjectId))
                    .Select(x => x.Id)
                    .ToListAsync();

                var goodsReceiptIds = await _unitOfWork.Db.GoodsReceipts
                    .IgnoreQueryFilters()
                    .Where(x => x.ProjectId.HasValue && projectIds.Contains(x.ProjectId.Value))
                    .Select(x => x.Id)
                    .ToListAsync();
                var feedingIds = await _unitOfWork.Db.Feedings.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId)).Select(x => x.Id).ToListAsync();
                var mortalityIds = await _unitOfWork.Db.Mortalities.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId)).Select(x => x.Id).ToListAsync();
                var shipmentIds = await _unitOfWork.Db.Shipments.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId)).Select(x => x.Id).ToListAsync();
                var transferIds = await _unitOfWork.Db.Transfers.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId)).Select(x => x.Id).ToListAsync();
                var warehouseTransferIds = await _unitOfWork.Db.WarehouseTransfers.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId)).Select(x => x.Id).ToListAsync();
                var cageWarehouseTransferIds = await _unitOfWork.Db.CageWarehouseTransfers.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId)).Select(x => x.Id).ToListAsync();
                var warehouseCageTransferIds = await _unitOfWork.Db.WarehouseCageTransfers.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId)).Select(x => x.Id).ToListAsync();
                var weighingIds = await _unitOfWork.Db.Weighings.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId)).Select(x => x.Id).ToListAsync();
                var stockConvertIds = await _unitOfWork.Db.StockConverts.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId)).Select(x => x.Id).ToListAsync();
                var netOperationIds = await _unitOfWork.Db.NetOperations.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId)).Select(x => x.Id).ToListAsync();

                var fishLabSampleIds = await _unitOfWork.Db.FishLabSamples
                    .IgnoreQueryFilters()
                    .Where(x =>
                        projectIds.Contains(x.ProjectId) ||
                        (x.ProjectCageId.HasValue && projectCageIds.Contains(x.ProjectCageId.Value)) ||
                        (x.FishBatchId.HasValue && fishBatchIds.Contains(x.FishBatchId.Value)))
                    .Select(x => x.Id)
                    .ToListAsync();
                var complianceAuditIds = await _unitOfWork.Db.ComplianceAudits
                    .IgnoreQueryFilters()
                    .Where(x =>
                        projectIds.Contains(x.ProjectId) ||
                        (x.ProjectCageId.HasValue && projectCageIds.Contains(x.ProjectCageId.Value)) ||
                        (x.FishBatchId.HasValue && fishBatchIds.Contains(x.FishBatchId.Value)))
                    .Select(x => x.Id)
                    .ToListAsync();

                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.FishLabResults.IgnoreQueryFilters().Where(x => fishLabSampleIds.Contains(x.FishLabSampleId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.ComplianceCorrectiveActions.IgnoreQueryFilters().Where(x => complianceAuditIds.Contains(x.ComplianceAuditId)));

                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.GoodsReceiptFishDistributions.IgnoreQueryFilters().Where(x => projectCageIds.Contains(x.ProjectCageId) || fishBatchIds.Contains(x.FishBatchId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.FeedingDistributions.IgnoreQueryFilters().Where(x => projectCageIds.Contains(x.ProjectCageId) || fishBatchIds.Contains(x.FishBatchId)));

                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.MortalityLines.IgnoreQueryFilters().Where(x => mortalityIds.Contains(x.MortalityId) || projectCageIds.Contains(x.ProjectCageId) || fishBatchIds.Contains(x.FishBatchId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.FeedingLines.IgnoreQueryFilters().Where(x => feedingIds.Contains(x.FeedingId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.GoodsReceiptLines.IgnoreQueryFilters().Where(x => goodsReceiptIds.Contains(x.GoodsReceiptId) || (x.FishBatchId.HasValue && fishBatchIds.Contains(x.FishBatchId.Value))));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.ShipmentLines.IgnoreQueryFilters().Where(x => shipmentIds.Contains(x.ShipmentId) || projectCageIds.Contains(x.FromProjectCageId) || fishBatchIds.Contains(x.FishBatchId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.TransferLines.IgnoreQueryFilters().Where(x => transferIds.Contains(x.TransferId) || projectCageIds.Contains(x.FromProjectCageId) || projectCageIds.Contains(x.ToProjectCageId) || fishBatchIds.Contains(x.FishBatchId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.WarehouseTransferLines.IgnoreQueryFilters().Where(x => warehouseTransferIds.Contains(x.WarehouseTransferId) || fishBatchIds.Contains(x.FishBatchId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.CageWarehouseTransferLines.IgnoreQueryFilters().Where(x => cageWarehouseTransferIds.Contains(x.CageWarehouseTransferId) || projectCageIds.Contains(x.FromProjectCageId) || fishBatchIds.Contains(x.FishBatchId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.WarehouseCageTransferLines.IgnoreQueryFilters().Where(x => warehouseCageTransferIds.Contains(x.WarehouseCageTransferId) || projectCageIds.Contains(x.ToProjectCageId) || fishBatchIds.Contains(x.FishBatchId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.WeighingLines.IgnoreQueryFilters().Where(x => weighingIds.Contains(x.WeighingId) || projectCageIds.Contains(x.ProjectCageId) || fishBatchIds.Contains(x.FishBatchId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.StockConvertLines.IgnoreQueryFilters().Where(x => stockConvertIds.Contains(x.StockConvertId) || projectCageIds.Contains(x.FromProjectCageId) || projectCageIds.Contains(x.ToProjectCageId) || fishBatchIds.Contains(x.FromFishBatchId) || fishBatchIds.Contains(x.ToFishBatchId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.NetOperationLines.IgnoreQueryFilters().Where(x => netOperationIds.Contains(x.NetOperationId) || projectCageIds.Contains(x.ProjectCageId) || (x.FishBatchId.HasValue && fishBatchIds.Contains(x.FishBatchId.Value))));

                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.BatchMovements.IgnoreQueryFilters().Where(x =>
                    fishBatchIds.Contains(x.FishBatchId) ||
                    (x.ProjectCageId.HasValue && projectCageIds.Contains(x.ProjectCageId.Value)) ||
                    (x.FromProjectCageId.HasValue && projectCageIds.Contains(x.FromProjectCageId.Value)) ||
                    (x.ToProjectCageId.HasValue && projectCageIds.Contains(x.ToProjectCageId.Value))));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.BatchCageBalances.IgnoreQueryFilters().Where(x => projectCageIds.Contains(x.ProjectCageId) || fishBatchIds.Contains(x.FishBatchId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.BatchWarehouseBalances.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId) || fishBatchIds.Contains(x.FishBatchId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.ProjectCageDailyKpiSnapshots.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId) || projectCageIds.Contains(x.ProjectCageId) || fishBatchIds.Contains(x.FishBatchId)));

                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.FishHealthEvents.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId) || (x.ProjectCageId.HasValue && projectCageIds.Contains(x.ProjectCageId.Value)) || (x.FishBatchId.HasValue && fishBatchIds.Contains(x.FishBatchId.Value))));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.FishTreatments.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId) || (x.ProjectCageId.HasValue && projectCageIds.Contains(x.ProjectCageId.Value)) || (x.FishBatchId.HasValue && fishBatchIds.Contains(x.FishBatchId.Value))));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.FishLabSamples.IgnoreQueryFilters().Where(x => fishLabSampleIds.Contains(x.Id)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.WelfareAssessments.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId) || (x.ProjectCageId.HasValue && projectCageIds.Contains(x.ProjectCageId.Value)) || (x.FishBatchId.HasValue && fishBatchIds.Contains(x.FishBatchId.Value))));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.ComplianceAudits.IgnoreQueryFilters().Where(x => complianceAuditIds.Contains(x.Id)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.DailyWeathers.IgnoreQueryFilters().Where(x => projectIds.Contains(x.ProjectId)));

                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.ProjectMergeCages.IgnoreQueryFilters().Where(x => projectIds.Contains(x.SourceProjectId) || projectCageIds.Contains(x.ProjectCageId) || cageIds.Contains(x.CageId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.ProjectMergeSources.IgnoreQueryFilters().Where(x => projectIds.Contains(x.SourceProjectId)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.ProjectMerges.IgnoreQueryFilters().Where(x => projectIds.Contains(x.TargetProjectId)));

                result.DeletedGoodsReceipts = await RemoveRangeAsync(_unitOfWork.Db.GoodsReceipts.IgnoreQueryFilters().Where(x => goodsReceiptIds.Contains(x.Id)));
                result.DeletedFeedings = await RemoveRangeAsync(_unitOfWork.Db.Feedings.IgnoreQueryFilters().Where(x => feedingIds.Contains(x.Id)));
                result.DeletedMortalities = await RemoveRangeAsync(_unitOfWork.Db.Mortalities.IgnoreQueryFilters().Where(x => mortalityIds.Contains(x.Id)));
                result.DeletedShipments = await RemoveRangeAsync(_unitOfWork.Db.Shipments.IgnoreQueryFilters().Where(x => shipmentIds.Contains(x.Id)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.Transfers.IgnoreQueryFilters().Where(x => transferIds.Contains(x.Id)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.WarehouseTransfers.IgnoreQueryFilters().Where(x => warehouseTransferIds.Contains(x.Id)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.CageWarehouseTransfers.IgnoreQueryFilters().Where(x => cageWarehouseTransferIds.Contains(x.Id)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.WarehouseCageTransfers.IgnoreQueryFilters().Where(x => warehouseCageTransferIds.Contains(x.Id)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.Weighings.IgnoreQueryFilters().Where(x => weighingIds.Contains(x.Id)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.StockConverts.IgnoreQueryFilters().Where(x => stockConvertIds.Contains(x.Id)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.NetOperations.IgnoreQueryFilters().Where(x => netOperationIds.Contains(x.Id)));

                var batchesToDetach = await _unitOfWork.Db.FishBatches
                    .IgnoreQueryFilters()
                    .Where(x => fishBatchIds.Contains(x.Id))
                    .ToListAsync();
                foreach (var batch in batchesToDetach)
                {
                    batch.SourceGoodsReceiptLineId = null;
                }

                result.DeletedFishBatches = await RemoveRangeAsync(_unitOfWork.Db.FishBatches.IgnoreQueryFilters().Where(x => fishBatchIds.Contains(x.Id)));
                result.DeletedProjectCages = await RemoveRangeAsync(_unitOfWork.Db.ProjectCages.IgnoreQueryFilters().Where(x => projectCageIds.Contains(x.Id)));
                result.DeletedOperationalRecords += await RemoveRangeAsync(_unitOfWork.Db.CageWarehouseMappings.IgnoreQueryFilters().Where(x => cageIds.Contains(x.CageId)));
                result.DeletedCages = await RemoveRangeAsync(_unitOfWork.Db.Cages.IgnoreQueryFilters().Where(x => cageIds.Contains(x.Id)));
                result.DeletedProjects = await RemoveRangeAsync(_unitOfWork.Db.Projects.IgnoreQueryFilters().Where(x => projectIds.Contains(x.Id)));

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<OpeningImportResetExistingDataResultDto>.SuccessResult(
                    result,
                    L("OpeningImportService.ResetPermanentClearCompleted"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return ApiResponse<OpeningImportResetExistingDataResultDto>.ErrorResult(
                    L("OpeningImportService.ResetClearFailed"),
                    ex.GetBaseException().Message,
                    StatusCodes.Status409Conflict);
            }
        }

        private async Task<int> RemoveRangeAsync<TEntity>(IQueryable<TEntity> query)
            where TEntity : class
        {
            var entities = await query.ToListAsync();
            if (entities.Count == 0)
            {
                return 0;
            }

            _unitOfWork.Db.Set<TEntity>().RemoveRange(entities);
            return entities.Count;
        }

        private async Task<List<StagedRow>> ValidateRowsAsync(OpeningImportPreviewRequestDto dto)
        {
            var stagedRows = new List<StagedRow>();

            foreach (var sheet in dto.Sheets)
            {
                var effectiveRows = sheet.Rows ?? new List<Dictionary<string, string?>>();
                for (var index = 0; index < effectiveRows.Count; index++)
                {
                    var rawRow = NormalizeDictionary(effectiveRows[index]);
                    var normalized = ApplyMappings(rawRow, sheet.Mappings);
                    var messages = ValidateSheetRow(sheet.SheetName, normalized);

                    stagedRows.Add(new StagedRow
                    {
                        SheetName = sheet.SheetName,
                        RowNumber = index + 2,
                        RawData = rawRow,
                        NormalizedData = normalized,
                        Messages = messages,
                        Entity = new OpeningImportRow
                        {
                            SheetName = sheet.SheetName,
                            RowNumber = index + 2,
                            Status = ResolveRowStatus(messages),
                            RawDataJson = JsonSerializer.Serialize(rawRow, JsonOptions),
                            NormalizedDataJson = JsonSerializer.Serialize(normalized, JsonOptions),
                            MessagesJson = JsonSerializer.Serialize(messages, JsonOptions)
                        }
                    });
                }
            }

            await ApplyCrossValidationsAsync(stagedRows);

            foreach (var row in stagedRows)
            {
                row.Entity.Status = ResolveRowStatus(row.Messages);
                row.Entity.NormalizedDataJson = JsonSerializer.Serialize(row.NormalizedData, JsonOptions);
                row.Entity.MessagesJson = JsonSerializer.Serialize(row.Messages, JsonOptions);
            }

            return stagedRows
                .OrderBy(x => x.SheetName)
                .ThenBy(x => x.RowNumber)
                .ToList();
        }

        private async Task ApplyCrossValidationsAsync(List<StagedRow> rows)
        {
            var projectRows = rows.Where(x => IsSheet(x.SheetName, "Projects")).ToList();
            var cageRows = rows.Where(x => IsSheet(x.SheetName, "Cages")).ToList();
            var stockRows = rows.Where(x => IsSheet(x.SheetName, "OpeningStock")).ToList();
            var goodsReceiptRows = rows.Where(x => IsSheet(x.SheetName, "OpeningGoodsReceipts")).ToList();
            var mortalityRows = rows.Where(x => IsSheet(x.SheetName, "OpeningMortality")).ToList();
            var feedingRows = rows.Where(x => IsSheet(x.SheetName, "OpeningFeedings")).ToList();
            var shipmentRows = rows.Where(x => IsSheet(x.SheetName, "OpeningShipments")).ToList();

            PropagateOpeningGoodsReceiptHeaderValues(goodsReceiptRows);
            AppendDuplicateErrors(
                projectRows,
                "projectCode",
                value => L("OpeningImportService.Validation.DuplicateProjectCode", value));
            AppendDuplicateErrors(
                cageRows,
                row => $"{GetValue(row, "projectCode")}::{GetValue(row, "cageCode")}",
                value => L("OpeningImportService.Validation.DuplicateProjectCage", value));
            AppendOpeningGoodsReceiptHeaderErrors(
                goodsReceiptRows,
                L("OpeningImportService.GoodsReceiptHeaderConflict"));
            AppendOpeningGoodsReceiptBatchErrors(
                goodsReceiptRows,
                L("OpeningImportService.GoodsReceiptBatchConflict"));

            var referencedProjectCodes = rows
                .Select(x => GetValue(x, "projectCode"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingProjects = await _unitOfWork.Projects.Query()
                .Where(x => referencedProjectCodes.Contains(x.ProjectCode))
                .ToDictionaryAsync(x => x.ProjectCode, x => x, StringComparer.OrdinalIgnoreCase);
            var deletedProjectCodes = await _unitOfWork.Projects.Query(ignoreQueryFilters: true)
                .Where(x => x.IsDeleted && referencedProjectCodes.Contains(x.ProjectCode))
                .Select(x => x.ProjectCode)
                .ToListAsync();
            var deletedProjectCodeSet = deletedProjectCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                var projectCode = GetValue(row, "projectCode");
                if (!string.IsNullOrWhiteSpace(projectCode) && existingProjects.ContainsKey(projectCode))
                {
                    row.Messages.Add(L("OpeningImportService.Validation.ProjectAlreadyExists", projectCode));
                }
                else if (!string.IsNullOrWhiteSpace(projectCode) && deletedProjectCodeSet.Contains(projectCode))
                {
                    row.Messages.Add(L("OpeningImportService.Validation.ProjectDeletedExists", projectCode));
                }
            }

            foreach (var row in cageRows.Concat(stockRows).Concat(goodsReceiptRows).Concat(mortalityRows).Concat(feedingRows).Concat(shipmentRows))
            {
                var projectCode = GetValue(row, "projectCode");
                if (string.IsNullOrWhiteSpace(projectCode))
                {
                    continue;
                }

                var existsInFile = projectRows.Any(x => string.Equals(GetValue(x, "projectCode"), projectCode, StringComparison.OrdinalIgnoreCase));
                if (!existsInFile && !existingProjects.ContainsKey(projectCode))
                {
                    row.Messages.Add(L("OpeningImportService.Validation.ProjectNotFound", projectCode));
                }
            }

            var openingStockCodes = stockRows
                .Concat(goodsReceiptRows)
                .Concat(mortalityRows)
                .Concat(feedingRows)
                .Concat(shipmentRows)
                .Select(x => GetValue(x, "fishStockCode"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingStocks = await _unitOfWork.Stocks.Query()
                .Where(x => openingStockCodes.Contains(x.ErpStockCode))
                .ToDictionaryAsync(x => x.ErpStockCode, x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var row in stockRows)
            {
                var stockCode = GetValue(row, "fishStockCode");
                if (!string.IsNullOrWhiteSpace(stockCode) && !existingStocks.ContainsKey(stockCode))
                {
                    row.Messages.Add(L("OpeningImportService.Validation.StockNotFound", stockCode));
                }
            }

            var feedStockCodes = feedingRows
                .Select(x => GetValue(x, "feedStockCode"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingFeedStocks = await _unitOfWork.Stocks.Query()
                .Where(x => feedStockCodes.Contains(x.ErpStockCode))
                .ToDictionaryAsync(x => x.ErpStockCode, x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var row in feedingRows)
            {
                var fishStockCode = GetValue(row, "fishStockCode");
                if (!string.IsNullOrWhiteSpace(fishStockCode) && !existingStocks.ContainsKey(fishStockCode))
                {
                    row.Messages.Add(L("OpeningImportService.Validation.FishStockNotFound", fishStockCode));
                }

                var feedStockCode = GetValue(row, "feedStockCode");
                if (!string.IsNullOrWhiteSpace(feedStockCode) && !existingFeedStocks.ContainsKey(feedStockCode))
                {
                    row.Messages.Add(L("OpeningImportService.Validation.FeedStockNotFound", feedStockCode));
                }
            }

            foreach (var row in shipmentRows)
            {
                var fishStockCode = GetValue(row, "fishStockCode");
                if (!string.IsNullOrWhiteSpace(fishStockCode) && !existingStocks.ContainsKey(fishStockCode))
                {
                    row.Messages.Add(L("OpeningImportService.Validation.FishStockNotFound", fishStockCode));
                }
            }

            var warehouseCodes = cageRows
                .Concat(stockRows)
                .Concat(goodsReceiptRows)
                .Select(x => GetValue(x, "warehouseCode"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            warehouseCodes = warehouseCodes
                .Concat(shipmentRows
                    .Select(x => GetValue(x, "targetWarehouseCode"))
                    .Where(x => !string.IsNullOrWhiteSpace(x)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var warehouseShortCodes = warehouseCodes
                .Select(code => short.TryParse(code, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : (short?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            var existingWarehouses = await _unitOfWork.Db.Warehouses
                .AsNoTracking()
                .Where(x => warehouseShortCodes.Contains(x.ErpWarehouseCode) && !x.IsDeleted)
                .ToDictionaryAsync(x => x.ErpWarehouseCode, x => x);

            foreach (var row in cageRows.Concat(stockRows).Concat(goodsReceiptRows).Concat(shipmentRows))
            {
                var warehouseCode = GetValue(row, "warehouseCode") ?? GetValue(row, "targetWarehouseCode");
                if (string.IsNullOrWhiteSpace(warehouseCode))
                {
                    continue;
                }

                if (!short.TryParse(warehouseCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedWarehouse) ||
                    !existingWarehouses.ContainsKey(parsedWarehouse))
                {
                    row.Messages.Add(L("OpeningImportService.Validation.WarehouseNotFound", warehouseCode));
                }
            }

            var cageCodes = rows
                .Select(x => GetValue(x, "cageCode"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingCages = await _unitOfWork.Cages.Query()
                .Where(x => cageCodes.Contains(x.CageCode))
                .ToDictionaryAsync(x => x.CageCode, x => x, StringComparer.OrdinalIgnoreCase);
            var deletedCageCodes = await _unitOfWork.Cages.Query(ignoreQueryFilters: true)
                .Where(x => x.IsDeleted && cageCodes.Contains(x.CageCode))
                .Select(x => x.CageCode)
                .ToListAsync();
            var deletedCageCodeSet = deletedCageCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var row in cageRows)
            {
                var cageCode = GetValue(row, "cageCode");
                if (!string.IsNullOrWhiteSpace(cageCode) && existingCages.ContainsKey(cageCode))
                {
                    row.Messages.Add(L("OpeningImportService.Validation.CageAlreadyExists", cageCode));
                }
                else if (!string.IsNullOrWhiteSpace(cageCode) && deletedCageCodeSet.Contains(cageCode))
                {
                    row.Messages.Add(L("OpeningImportService.Validation.CageDeletedExists", cageCode));
                }
            }

            var activeAssignments = await _unitOfWork.Db.ProjectCages
                .AsNoTracking()
                .Include(x => x.Project)
                .Include(x => x.Cage)
                .Where(x => !x.IsDeleted && x.ReleasedDate == null && cageCodes.Contains(x.Cage!.CageCode))
                .ToListAsync();

            foreach (var row in cageRows.Concat(stockRows).Concat(goodsReceiptRows).Concat(mortalityRows).Concat(feedingRows).Concat(shipmentRows))
            {
                var cageCode = GetValue(row, "cageCode");
                var projectCode = GetValue(row, "projectCode");
                if (string.IsNullOrWhiteSpace(cageCode) || string.IsNullOrWhiteSpace(projectCode))
                {
                    continue;
                }

                var existsInFile = cageRows.Any(x =>
                    string.Equals(GetValue(x, "cageCode"), cageCode, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(GetValue(x, "projectCode"), projectCode, StringComparison.OrdinalIgnoreCase));

                if (!existsInFile && !existingCages.ContainsKey(cageCode))
                {
                    row.Messages.Add(L("OpeningImportService.Validation.CageNotFound", cageCode));
                }

                var conflictingAssignment = activeAssignments.FirstOrDefault(x =>
                    string.Equals(x.Cage?.CageCode, cageCode, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(x.Project?.ProjectCode, projectCode, StringComparison.OrdinalIgnoreCase));

                if (conflictingAssignment != null)
                {
                    var conflictingProjectCode = conflictingAssignment.Project?.ProjectCode ?? string.Empty;
                    row.Messages.Add(L("OpeningImportService.Validation.CageAssignedToDifferentProject", cageCode, conflictingProjectCode));
                }
            }
        }

        private async Task<Dictionary<string, Project>> EnsureProjectsAsync(List<OpeningImportRow> rows, OpeningImportCommitResultDto result)
        {
            var projectRows = rows
                .Where(x => IsSheet(x.SheetName, "Projects") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .Select(ParseRow)
                .ToList();

            var referencedCodes = rows
                .Where(x => (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .Select(x => ParseRow(x))
                .Select(x => x.TryGetValue("projectCode", out var projectCode) ? projectCode : null)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existing = await _unitOfWork.Projects.Query(tracking: true)
                .Where(x => referencedCodes.Contains(x.ProjectCode))
                .ToDictionaryAsync(x => x.ProjectCode, x => x, StringComparer.OrdinalIgnoreCase);

            var createdAny = false;
            foreach (var row in projectRows)
            {
                var projectCode = row["projectCode"] ?? string.Empty;
                if (existing.ContainsKey(projectCode))
                {
                    continue;
                }

                var startDate = ParseDateOrDefault(row.TryGetValue("startDate", out var startDateValue) ? startDateValue : null, DateTimeProvider.Now.Date);

                var entity = new Project
                {
                    ProjectCode = projectCode,
                    ProjectName = row.TryGetValue("projectName", out var projectName) && !string.IsNullOrWhiteSpace(projectName)
                        ? projectName
                        : projectCode,
                    StartDate = startDate,
                    Status = DocumentStatus.Draft,
                    Note = row.TryGetValue("note", out var note) ? note : null
                };

                await _unitOfWork.Projects.AddAsync(entity);
                existing[projectCode] = entity;
                result.CreatedProjects += 1;
                createdAny = true;
            }

            if (createdAny)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return existing;
        }

        private async Task<Dictionary<string, Cage>> EnsureCagesAsync(
            List<OpeningImportRow> rows,
            IReadOnlyDictionary<string, Project> projectsByCode,
            OpeningImportCommitResultDto result)
        {
            var cageRows = rows
                .Where(x => IsSheet(x.SheetName, "Cages") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .Select(ParseRow)
                .ToList();

            var stockRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningStock") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .Select(ParseRow)
                .Where(x => x.TryGetValue("cageCode", out var cageCode) && !string.IsNullOrWhiteSpace(cageCode))
                .ToList();
            var goodsReceiptRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningGoodsReceipts") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .Select(ParseRow)
                .Where(x => x.TryGetValue("cageCode", out var cageCode) && !string.IsNullOrWhiteSpace(cageCode))
                .ToList();
            var mortalityRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningMortality") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .Select(ParseRow)
                .Where(x => x.TryGetValue("cageCode", out var cageCode) && !string.IsNullOrWhiteSpace(cageCode))
                .ToList();
            var feedingRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningFeedings") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .Select(ParseRow)
                .Where(x => x.TryGetValue("cageCode", out var cageCode) && !string.IsNullOrWhiteSpace(cageCode))
                .ToList();
            var shipmentRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningShipments") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .Select(ParseRow)
                .Where(x => x.TryGetValue("cageCode", out var cageCode) && !string.IsNullOrWhiteSpace(cageCode))
                .ToList();

            var allCageCodes = cageRows
                .Concat(stockRows)
                .Concat(goodsReceiptRows)
                .Concat(mortalityRows)
                .Concat(feedingRows)
                .Concat(shipmentRows)
                .Select(x => x["cageCode"] ?? string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var cagesByCode = await _unitOfWork.Cages.Query(tracking: true)
                .Where(x => allCageCodes.Contains(x.CageCode))
                .ToDictionaryAsync(x => x.CageCode, x => x, StringComparer.OrdinalIgnoreCase);

            var createdAnyCage = false;
            foreach (var row in cageRows)
            {
                var cageCode = row["cageCode"] ?? string.Empty;
                if (cagesByCode.ContainsKey(cageCode))
                {
                    continue;
                }

                var entity = new Cage
                {
                    CageCode = cageCode,
                    CageName = row.TryGetValue("cageName", out var cageName) && !string.IsNullOrWhiteSpace(cageName)
                        ? cageName
                        : cageCode
                };

                await _unitOfWork.Cages.AddAsync(entity);
                cagesByCode[cageCode] = entity;
                result.CreatedCages += 1;
                createdAnyCage = true;
            }

            if (createdAnyCage)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            await EnsureCageWarehouseMappingsAsync(cageRows, cagesByCode, result);

            var existingAssignments = await _unitOfWork.Db.ProjectCages
                .Where(x => !x.IsDeleted && x.ReleasedDate == null && allCageCodes.Contains(x.Cage!.CageCode))
                .Select(x => new { x.ProjectId, x.CageId })
                .ToListAsync();

            var activeAssignmentKeys = existingAssignments
                .Select(x => $"{x.ProjectId}:{x.CageId}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var activeCageOwners = existingAssignments
                .GroupBy(x => x.CageId)
                .ToDictionary(x => x.Key, x => x.First().ProjectId);

            var createdAnyAssignment = false;
            foreach (var row in cageRows.Concat(stockRows).Concat(goodsReceiptRows).Concat(mortalityRows).Concat(feedingRows).Concat(shipmentRows))
            {
                var projectCode = row.TryGetValue("projectCode", out var projectCodeValue) ? projectCodeValue : null;
                var cageCode = row.TryGetValue("cageCode", out var cageCodeValue) ? cageCodeValue : null;
                if (string.IsNullOrWhiteSpace(projectCode) || string.IsNullOrWhiteSpace(cageCode))
                {
                    continue;
                }

                if (!projectsByCode.TryGetValue(projectCode, out var project) || !cagesByCode.TryGetValue(cageCode, out var cage))
                {
                    continue;
                }

                var assignmentKey = $"{project.Id}:{cage.Id}";
                if (activeAssignmentKeys.Contains(assignmentKey))
                {
                    continue;
                }

                var assignedDate = ParseDateOrDefault(row.TryGetValue("assignedDate", out var assignedDateValue) ? assignedDateValue : null, project.StartDate);
                var releasedDate = ParseNullableDate(row.TryGetValue("releasedDate", out var releasedDateValue) ? releasedDateValue : null);

                if (activeCageOwners.TryGetValue(cage.Id, out var activeProjectId) && activeProjectId != project.Id)
                {
                    continue;
                }

                await _unitOfWork.ProjectCages.AddAsync(new ProjectCage
                {
                    ProjectId = project.Id,
                    CageId = cage.Id,
                    AssignedDate = assignedDate,
                    ReleasedDate = releasedDate
                });
                result.CreatedProjectCages += 1;
                createdAnyAssignment = true;
                activeAssignmentKeys.Add(assignmentKey);
                if (!releasedDate.HasValue)
                {
                    activeCageOwners[cage.Id] = project.Id;
                }
            }

            if (createdAnyAssignment)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return cagesByCode;
        }

        private async Task EnsureCageWarehouseMappingsAsync(
            IReadOnlyCollection<Dictionary<string, string?>> cageRows,
            IReadOnlyDictionary<string, Cage> cagesByCode,
            OpeningImportCommitResultDto result)
        {
            var mappingRows = cageRows
                .Where(x => x.TryGetValue("warehouseCode", out var warehouseCode) && !string.IsNullOrWhiteSpace(warehouseCode))
                .ToList();

            if (mappingRows.Count == 0)
            {
                return;
            }

            var warehouseCodes = mappingRows
                .Select(x => x["warehouseCode"])
                .Where(x => short.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                .Select(x => short.Parse(x!, NumberStyles.Integer, CultureInfo.InvariantCulture))
                .Distinct()
                .ToList();

            var warehouses = await _unitOfWork.Db.Warehouses
                .Where(x => !x.IsDeleted && warehouseCodes.Contains(x.ErpWarehouseCode))
                .ToDictionaryAsync(x => x.ErpWarehouseCode, x => x);

            var cageIds = mappingRows
                .Select(x => x.TryGetValue("cageCode", out var cageCode) && !string.IsNullOrWhiteSpace(cageCode) && cagesByCode.TryGetValue(cageCode, out var cage)
                    ? cage.Id
                    : (long?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var activeMappings = await _unitOfWork.Repository<CageWarehouseMapping>()
                .Query()
                .Where(x => !x.IsDeleted && x.IsActive && cageIds.Contains(x.CageId))
                .Select(x => new { x.CageId, x.WarehouseId })
                .ToListAsync();

            var activeCageIds = activeMappings
                .Select(x => x.CageId)
                .ToHashSet();

            var createdAny = false;
            foreach (var row in mappingRows)
            {
                var cageCode = row.TryGetValue("cageCode", out var cageCodeValue) ? cageCodeValue : null;
                var warehouseCode = row.TryGetValue("warehouseCode", out var warehouseCodeValue) ? warehouseCodeValue : null;

                if (string.IsNullOrWhiteSpace(cageCode) ||
                    string.IsNullOrWhiteSpace(warehouseCode) ||
                    !cagesByCode.TryGetValue(cageCode, out var cage) ||
                    !short.TryParse(warehouseCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedWarehouseCode) ||
                    !warehouses.TryGetValue(parsedWarehouseCode, out var warehouse) ||
                    activeCageIds.Contains(cage.Id))
                {
                    continue;
                }

                    await _unitOfWork.Repository<CageWarehouseMapping>().AddAsync(new CageWarehouseMapping
                    {
                        CageId = cage.Id,
                        WarehouseId = warehouse.Id,
                        IsActive = true,
                        Note = L("OpeningImportService.TemplateGeneratedByOpeningImport")
                    });
                activeCageIds.Add(cage.Id);
                result.CreatedCageWarehouseMappings += 1;
                createdAny = true;
            }

            if (createdAny)
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private async Task ApplyOpeningBalancesAsync(
            List<OpeningImportRow> rows,
            IReadOnlyDictionary<string, Project> projectsByCode,
            IReadOnlyDictionary<string, Cage> cagesByCode,
            OpeningImportCommitResultDto result)
        {
            var stockRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningStock") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .ToList();
            var derivedRows = BuildDerivedOpeningRowsAsync(rows);
            stockRows = stockRows.Concat(derivedRows).ToList();

            var stockCodes = stockRows
                .Select(x => ParseRow(x))
                .Select(x => x.TryGetValue("fishStockCode", out var fishStockCode) ? fishStockCode : null)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var stocks = await _unitOfWork.Stocks.Query(tracking: true)
                .Where(x => stockCodes.Contains(x.ErpStockCode))
                .ToDictionaryAsync(x => x.ErpStockCode, x => x, StringComparer.OrdinalIgnoreCase);

            var warehouseCodes = stockRows
                .Select(x => ParseRow(x))
                .Select(x => x.TryGetValue("warehouseCode", out var warehouseCode) ? warehouseCode : null)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(x => short.Parse(x!, CultureInfo.InvariantCulture))
                .ToList();

            var warehouses = await _unitOfWork.Db.Warehouses
                .Where(x => !x.IsDeleted && warehouseCodes.Contains(x.ErpWarehouseCode))
                .ToDictionaryAsync(x => x.ErpWarehouseCode, x => x);

            foreach (var row in stockRows)
            {
                var normalized = ParseRow(row);
                var projectCode = normalized["projectCode"] ?? string.Empty;
                var batchCode = normalized["batchCode"] ?? string.Empty;
                var fishStockCode = normalized["fishStockCode"] ?? string.Empty;
                var cageCode = normalized.TryGetValue("cageCode", out var cageValue) ? cageValue : null;
                var warehouseCode = normalized.TryGetValue("warehouseCode", out var warehouseValue) ? warehouseValue : null;

                if (!projectsByCode.TryGetValue(projectCode, out var project) || !stocks.TryGetValue(fishStockCode, out var stock))
                {
                    result.SkippedRows += 1;
                    continue;
                }

                var batch = await _unitOfWork.Db.FishBatches
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.BatchCode == batchCode);

                var averageGram = ParseAverageGramOrDefault(normalized.TryGetValue("averageGram", out var averageGramValue) ? averageGramValue : null, 0m);
                var openingDate = ParseDateOrDefault(normalized.TryGetValue("asOfDate", out var asOfDateValue) ? asOfDateValue : null, project.StartDate);

                if (batch == null)
                {
                    batch = new FishBatch
                    {
                        ProjectId = project.Id,
                        BatchCode = batchCode,
                        FishStockId = stock.Id,
                        CurrentAverageGram = averageGram,
                        StartDate = openingDate
                    };

                    await _unitOfWork.FishBatches.AddAsync(batch);
                    await _unitOfWork.SaveChangesAsync();
                    result.CreatedFishBatches += 1;
                }

                var fishCount = ParseIntOrDefault(normalized.TryGetValue("fishCount", out var fishCountValue) ? fishCountValue : null, 0);
                var biomassGram = Math.Round(fishCount * averageGram, 3, MidpointRounding.AwayFromZero);

                if (!string.IsNullOrWhiteSpace(cageCode) && cagesByCode.TryGetValue(cageCode, out var cage))
                {
                    var projectCage = await FindProjectCageForDateAsync(project.Id, cage.Id, openingDate);

                    if (projectCage == null)
                    {
                        result.SkippedRows += 1;
                        continue;
                    }

                    await _balanceLedgerManager.ApplyDelta(
                        project.Id,
                        batch.Id,
                        projectCage.Id,
                        fishCount,
                        biomassGram,
                        BatchMovementType.OpeningImport,
                        openingDate,
                        "Opening import",
                        "OpeningImportJob",
                        row.OpeningImportJobId,
                        null,
                        projectCage.Id,
                        stock.Id,
                        stock.Id,
                        averageGram,
                        averageGram);

                    result.AppliedCageRows += 1;
                }
                else if (!string.IsNullOrWhiteSpace(warehouseCode) &&
                         short.TryParse(warehouseCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedWarehouseCode) &&
                         warehouses.TryGetValue(parsedWarehouseCode, out var warehouse))
                {
                    await _balanceLedgerManager.ApplyWarehouseDelta(
                        project.Id,
                        batch.Id,
                        warehouse.Id,
                        fishCount,
                        biomassGram,
                        BatchMovementType.OpeningImport,
                        openingDate,
                        "Opening import",
                        "OpeningImportJob",
                        row.OpeningImportJobId,
                        null,
                        warehouse.Id,
                        stock.Id,
                        stock.Id,
                        averageGram,
                        averageGram);

                    result.AppliedWarehouseRows += 1;
                }
                else
                {
                    result.SkippedRows += 1;
                    continue;
                }

                row.Status = OpeningImportRowStatus.Applied;
                row.UpdatedDate = DateTimeProvider.Now;
            }
        }

        private async Task ApplyOpeningMortalityLedgersAsync(
            List<OpeningImportRow> rows,
            IReadOnlyDictionary<string, Project> projectsByCode,
            IReadOnlyDictionary<string, Cage> cagesByCode)
        {
            var mortalityRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningMortality") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning || x.Status == OpeningImportRowStatus.Applied))
                .ToList();

            if (mortalityRows.Count == 0)
            {
                return;
            }

            var stockCodes = mortalityRows
                .Select(ParseRow)
                .Select(x => x.TryGetValue("fishStockCode", out var fishStockCode) ? fishStockCode : null)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var stocks = await _unitOfWork.Stocks.Query(tracking: true)
                .Where(x => stockCodes.Contains(x.ErpStockCode))
                .ToDictionaryAsync(x => x.ErpStockCode, x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var row in mortalityRows)
            {
                var normalized = ParseRow(row);
                var projectCode = normalized["projectCode"] ?? string.Empty;
                var fishStockCode = normalized["fishStockCode"] ?? string.Empty;
                var cageCode = normalized.TryGetValue("cageCode", out var cageValue) ? cageValue : null;

                if (!projectsByCode.TryGetValue(projectCode, out var project) ||
                    !stocks.TryGetValue(fishStockCode, out var stock) ||
                    string.IsNullOrWhiteSpace(cageCode) ||
                    !cagesByCode.TryGetValue(cageCode, out var cage))
                {
                    continue;
                }

                var mortalityDate = ParseDateOrDefault(normalized.TryGetValue("mortalityDate", out var mortalityDateValue) ? mortalityDateValue : null, project.StartDate);
                var projectCage = await FindProjectCageForDateAsync(project.Id, cage.Id, mortalityDate);
                if (projectCage == null)
                {
                    continue;
                }

                var deadCount = ParseIntOrDefault(normalized.TryGetValue("deadCount", out var deadCountValue) ? deadCountValue : null, 0);
                if (deadCount <= 0)
                {
                    continue;
                }

                var batchCode = normalized.TryGetValue("batchCode", out var batchCodeValue) ? batchCodeValue : null;
                var batch = await _unitOfWork.Db.FishBatches
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.BatchCode == batchCode && x.FishStockId == stock.Id);
                if (batch == null)
                {
                    throw new InvalidOperationException(L("OpeningImportService.MortalityBatchNotFound"));
                }

                var mortality = await _unitOfWork.Db.Mortalities
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.MortalityDate.Date == mortalityDate.Date);
                if (mortality == null)
                {
                    throw new InvalidOperationException(L("OpeningImportService.MortalityHeaderNotFound"));
                }

                var ledgerExists = await _unitOfWork.Db.BatchMovements.AnyAsync(x =>
                    !x.IsDeleted &&
                    x.MovementType == BatchMovementType.Mortality &&
                    x.ReferenceTable == "RII_MORTALITY" &&
                    x.ReferenceId == mortality.Id &&
                    x.FishBatchId == batch.Id &&
                    x.ProjectCageId == projectCage.Id);

                if (ledgerExists)
                {
                    continue;
                }

                var explicitMortalityBiomassKg = ParseNullableDecimal(
                    normalized.TryGetValue("mortalityBiomassKg", out var mortalityBiomassKgValue)
                        ? mortalityBiomassKgValue
                        : null);
                var hasExplicitMortalityBiomass = explicitMortalityBiomassKg.HasValue && explicitMortalityBiomassKg.Value >= 0m;
                var balance = hasExplicitMortalityBiomass
                    ? null
                    : await _unitOfWork.Db.BatchCageBalances
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => !x.IsDeleted && x.FishBatchId == batch.Id && x.ProjectCageId == projectCage.Id);
                var averageGram = hasExplicitMortalityBiomass
                    ? deadCount > 0 && explicitMortalityBiomassKg!.Value > 0m
                        ? Math.Round(explicitMortalityBiomassKg.Value * 1000m / deadCount, 3, MidpointRounding.AwayFromZero)
                        : 0m
                    : ResolveAverageGram(balance);
                if (!hasExplicitMortalityBiomass && averageGram <= 0)
                {
                    throw new InvalidOperationException(L("OpeningImportService.MortalityAverageGramNotFound"));
                }
                var mortalityBiomassGram = hasExplicitMortalityBiomass
                    ? Math.Round(explicitMortalityBiomassKg!.Value * 1000m, 3, MidpointRounding.AwayFromZero)
                    : Math.Round(deadCount * averageGram, 3, MidpointRounding.AwayFromZero);

                await _balanceLedgerManager.ApplyDelta(
                    project.Id,
                    batch.Id,
                    projectCage.Id,
                    -deadCount,
                    -mortalityBiomassGram,
                    BatchMovementType.Mortality,
                    mortalityDate,
                    "Opening import mortality",
                    "RII_MORTALITY",
                    mortality.Id,
                    projectCage.Id,
                    null,
                    stock.Id,
                    stock.Id,
                    averageGram,
                    averageGram);
            }
        }

        private List<OpeningImportRow> BuildDerivedOpeningRowsAsync(List<OpeningImportRow> rows)
        {
            var explicitRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningStock") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .Select(ParseRow)
                .Select(CreateBalanceKey)
                .Where(x => x != null)
                .Select(x => x!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var receiptRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningGoodsReceipts") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .Select(ParseRow)
                .ToList();
            var mortalityRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningMortality") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .Select(ParseRow)
                .ToList();

            var aggregates = new Dictionary<string, (Dictionary<string, string?> Row, int FishCount)>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in receiptRows)
            {
                var key = CreateBalanceKey(row);
                if (key == null || explicitRows.Contains(key))
                {
                    continue;
                }

                var fishCount = ParseIntOrDefault(row.TryGetValue("fishCount", out var fishCountValue) ? fishCountValue : null, 0);
                if (fishCount <= 0)
                {
                    continue;
                }

                if (!aggregates.TryGetValue(key, out var aggregate))
                {
                    var snapshot = new Dictionary<string, string?>(row, StringComparer.OrdinalIgnoreCase);
                    snapshot["asOfDate"] = row.TryGetValue("receiptDate", out var receiptDate) ? receiptDate : null;
                    aggregates[key] = (snapshot, fishCount);
                }
                else
                {
                    aggregates[key] = (aggregate.Row, aggregate.FishCount + fishCount);
                }
            }

            foreach (var row in mortalityRows)
            {
                var key = CreateBalanceKey(row);
                if (key == null || explicitRows.Contains(key))
                {
                    continue;
                }

                var deadCount = ParseIntOrDefault(row.TryGetValue("deadCount", out var deadCountValue) ? deadCountValue : null, 0);
                if (deadCount <= 0)
                {
                    continue;
                }

                if (!aggregates.TryGetValue(key, out var aggregate))
                {
                    continue;
                }

                aggregates[key] = (aggregate.Row, Math.Max(0, aggregate.FishCount - deadCount));
            }

            return aggregates
                .Where(x => x.Value.FishCount > 0)
                .Select(x =>
                {
                var row = new OpeningImportRow
                {
                        SheetName = "OpeningStock",
                        RowNumber = 0,
                        Status = OpeningImportRowStatus.Valid,
                    };

                    var normalized = new Dictionary<string, string?>(x.Value.Row, StringComparer.OrdinalIgnoreCase)
                    {
                        ["fishCount"] = x.Value.FishCount.ToString(CultureInfo.InvariantCulture),
                        ["asOfDate"] = x.Value.Row.TryGetValue("asOfDate", out var asOfDate) && !string.IsNullOrWhiteSpace(asOfDate)
                            ? asOfDate
                            : DateTimeProvider.Now.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    };

                    row.RawDataJson = JsonSerializer.Serialize(normalized, JsonOptions);
                    row.NormalizedDataJson = JsonSerializer.Serialize(normalized, JsonOptions);
                row.MessagesJson = JsonSerializer.Serialize(new List<string>
                {
                    L("OpeningImportService.DerivedOpeningStockNotice")
                }, JsonOptions);

                    return row;
                })
                .ToList();
        }

        private async Task CreateSummaryDocumentsAsync(
            List<OpeningImportRow> rows,
            IReadOnlyDictionary<string, Project> projectsByCode,
            IReadOnlyDictionary<string, Cage> cagesByCode,
            OpeningImportCommitResultDto result)
        {
            await CreateOpeningGoodsReceiptsAsync(rows, projectsByCode, cagesByCode, result);
            await CreateOpeningMortalitiesAsync(rows, projectsByCode, cagesByCode, result);
            await CreateOpeningFeedingsAsync(rows, projectsByCode, cagesByCode, result);
            await CreateOpeningShipmentsAsync(rows, projectsByCode, cagesByCode, result);
        }

        private async Task CreateOpeningGoodsReceiptsAsync(
            List<OpeningImportRow> rows,
            IReadOnlyDictionary<string, Project> projectsByCode,
            IReadOnlyDictionary<string, Cage> cagesByCode,
            OpeningImportCommitResultDto result)
        {
            var receiptRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningGoodsReceipts") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .ToList();

            if (receiptRows.Count == 0)
            {
                return;
            }

            var stockCodes = receiptRows
                .Select(ParseRow)
                .Select(x => x.TryGetValue("fishStockCode", out var fishStockCode) ? fishStockCode : null)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var stocks = await _unitOfWork.Stocks.Query(tracking: true)
                .Where(x => stockCodes.Contains(x.ErpStockCode))
                .ToDictionaryAsync(x => x.ErpStockCode, x => x, StringComparer.OrdinalIgnoreCase);

            var warehouseCodes = receiptRows
                .Select(ParseRow)
                .Select(x => x.TryGetValue("warehouseCode", out var warehouseCode) ? warehouseCode : null)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(x => short.Parse(x!, CultureInfo.InvariantCulture))
                .ToList();

            var warehouses = await _unitOfWork.Db.Warehouses
                .Where(x => !x.IsDeleted && warehouseCodes.Contains(x.ErpWarehouseCode))
                .ToDictionaryAsync(x => x.ErpWarehouseCode, x => x);

            var batchesByKey = await LoadExistingBatchesByKeyAsync(receiptRows, projectsByCode);
            var headerByKey = new Dictionary<string, GoodsReceipt>(StringComparer.OrdinalIgnoreCase);
            var lineByKey = new Dictionary<string, GoodsReceiptLine>(StringComparer.OrdinalIgnoreCase);
            var distributionByKey = new Dictionary<string, GoodsReceiptFishDistribution>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in receiptRows)
            {
                var normalized = ParseRow(row);
                var projectCode = normalized.TryGetValue("projectCode", out var projectCodeValue) ? projectCodeValue : null;
                var fishStockCode = normalized.TryGetValue("fishStockCode", out var fishStockCodeValue) ? fishStockCodeValue : null;
                if (string.IsNullOrWhiteSpace(projectCode) || string.IsNullOrWhiteSpace(fishStockCode))
                {
                    result.SkippedRows += 1;
                    continue;
                }

                if (!projectsByCode.TryGetValue(projectCode, out var project) || !stocks.TryGetValue(fishStockCode, out var stock))
                {
                    result.SkippedRows += 1;
                    continue;
                }

                var receiptDate = ParseDateOrDefault(normalized.TryGetValue("receiptDate", out var receiptDateValue) ? receiptDateValue : null, project.StartDate);
                var receiptNo = normalized.TryGetValue("receiptNo", out var receiptNoValue) && !string.IsNullOrWhiteSpace(receiptNoValue)
                    ? receiptNoValue!.Trim()
                    : $"OPEN-REC-{project.ProjectCode}-{receiptDate:yyyyMMdd}";
                var headerKey = project.Id.ToString(CultureInfo.InvariantCulture);

                if (!headerByKey.TryGetValue(headerKey, out var header))
                {
                    header = await _unitOfWork.Db.GoodsReceipts
                        .Include(x => x.Lines)
                        .ThenInclude(x => x.FishDistributions)
                        .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectId == project.Id);

                    if (header == null)
                    {
                        header = new GoodsReceipt
                        {
                            ProjectId = project.Id,
                            ReceiptNo = receiptNo,
                            ReceiptDate = receiptDate,
                            Status = DocumentStatus.Posted,
                            WarehouseId = TryResolveWarehouseId(normalized, warehouses),
                            Note = $"Opening import summary - {row.OpeningImportJobId}"
                        };

                        await _unitOfWork.GoodsReceipts.AddAsync(header);
                        result.CreatedGoodsReceipts += 1;
                    }

                    headerByKey[headerKey] = header;
                }

                var batchCode = normalized.TryGetValue("batchCode", out var batchCodeValue) ? batchCodeValue : null;
                var effectiveBatchCode = ResolveBatchCode(project.ProjectCode, batchCode);
                var lineKey = $"{project.Id}:{effectiveBatchCode}";
                var fishCount = ParseIntOrDefault(normalized.TryGetValue("fishCount", out var fishCountValue) ? fishCountValue : null, 0);
                var fishAverageGram = ParseAverageGramOrDefault(normalized.TryGetValue("averageGram", out var averageGramValue) ? averageGramValue : null, 0m);

                if (!lineByKey.TryGetValue(lineKey, out var line))
                {
                    var batch = await EnsureFishBatchAsync(project, stock, batchCode, normalized, batchesByKey, result);
                    line = new GoodsReceiptLine
                    {
                        GoodsReceipt = header,
                        ItemType = GoodsReceiptItemType.Fish,
                        StockId = stock.Id,
                        FishCount = 0,
                        FishAverageGram = fishAverageGram,
                        FishTotalGram = 0,
                        FishBatchId = batch.Id
                    };

                    await _unitOfWork.GoodsReceiptLines.AddAsync(line);
                    lineByKey[lineKey] = line;
                    result.CreatedGoodsReceiptLines += 1;
                }

                line.FishCount = (line.FishCount ?? 0) + fishCount;
                line.FishTotalGram = Math.Round(
                    (line.FishTotalGram ?? 0) + fishCount * fishAverageGram,
                    3,
                    MidpointRounding.AwayFromZero);

                if (!string.IsNullOrWhiteSpace(normalized.TryGetValue("cageCode", out var cageCodeValue) ? cageCodeValue : null) &&
                    cagesByCode.TryGetValue(cageCodeValue!, out var cage))
                {
                    var projectCage = await FindProjectCageForDateAsync(project.Id, cage.Id, receiptDate);

                    if (projectCage != null)
                    {
                        var distributionKey = $"{lineKey}:{projectCage.Id}";
                        if (!distributionByKey.TryGetValue(distributionKey, out var distribution))
                        {
                            distribution = new GoodsReceiptFishDistribution
                            {
                                ProjectCageId = projectCage.Id,
                                FishBatchId = line.FishBatchId!.Value,
                                FishCount = 0
                            };
                            line.FishDistributions.Add(distribution);
                            distributionByKey[distributionKey] = distribution;
                        }

                        distribution.FishCount += fishCount;
                    }
                }
            }
        }

        private async Task CreateOpeningMortalitiesAsync(
            List<OpeningImportRow> rows,
            IReadOnlyDictionary<string, Project> projectsByCode,
            IReadOnlyDictionary<string, Cage> cagesByCode,
            OpeningImportCommitResultDto result)
        {
            var mortalityRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningMortality") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .ToList();

            if (mortalityRows.Count == 0)
            {
                return;
            }

            var stockCodes = mortalityRows
                .Select(ParseRow)
                .Select(x => x.TryGetValue("fishStockCode", out var fishStockCode) ? fishStockCode : null)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var stocks = await _unitOfWork.Stocks.Query(tracking: true)
                .Where(x => stockCodes.Contains(x.ErpStockCode))
                .ToDictionaryAsync(x => x.ErpStockCode, x => x, StringComparer.OrdinalIgnoreCase);
            var batchesByKey = await LoadExistingBatchesByKeyAsync(mortalityRows, projectsByCode);
            var headerByKey = new Dictionary<string, Mortality>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in mortalityRows)
            {
                var normalized = ParseRow(row);
                var projectCode = normalized.TryGetValue("projectCode", out var projectCodeValue) ? projectCodeValue : null;
                var cageCode = normalized.TryGetValue("cageCode", out var cageCodeValue) ? cageCodeValue : null;
                var fishStockCode = normalized.TryGetValue("fishStockCode", out var fishStockCodeValue) ? fishStockCodeValue : null;
                if (string.IsNullOrWhiteSpace(projectCode) || string.IsNullOrWhiteSpace(cageCode) || string.IsNullOrWhiteSpace(fishStockCode))
                {
                    result.SkippedRows += 1;
                    continue;
                }

                if (!projectsByCode.TryGetValue(projectCode, out var project) ||
                    !cagesByCode.TryGetValue(cageCode, out var cage) ||
                    !stocks.TryGetValue(fishStockCode, out var stock))
                {
                    result.SkippedRows += 1;
                    continue;
                }

                var mortalityDate = ParseDateOrDefault(normalized.TryGetValue("mortalityDate", out var mortalityDateValue) ? mortalityDateValue : null, project.StartDate);
                var projectCage = await FindProjectCageForDateAsync(project.Id, cage.Id, mortalityDate);
                if (projectCage == null)
                {
                    result.SkippedRows += 1;
                    continue;
                }

                var headerKey = $"{project.Id}:{mortalityDate:yyyyMMdd}";
                if (!headerByKey.TryGetValue(headerKey, out var header))
                {
                    header = await _unitOfWork.Db.Mortalities
                        .Include(x => x.Lines)
                        .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.MortalityDate.Date == mortalityDate.Date);

                    if (header == null)
                    {
                        header = new Mortality
                        {
                            ProjectId = project.Id,
                            MortalityDate = mortalityDate,
                            Status = DocumentStatus.Posted,
                            Note = $"Opening import summary - {row.OpeningImportJobId}"
                        };
                        await _unitOfWork.Mortalities.AddAsync(header);
                        result.CreatedMortalityHeaders += 1;
                    }

                    headerByKey[headerKey] = header;
                }

                var batchCode = normalized.TryGetValue("batchCode", out var batchCodeValue) ? batchCodeValue : null;
                var batch = await EnsureFishBatchAsync(project, stock, batchCode, normalized, batchesByKey, result);

                await _unitOfWork.MortalityLines.AddAsync(new MortalityLine
                {
                    Mortality = header,
                    FishBatchId = batch.Id,
                    ProjectCageId = projectCage.Id,
                    DeadCount = ParseIntOrDefault(normalized.TryGetValue("deadCount", out var deadCountValue) ? deadCountValue : null, 0)
                });

                result.CreatedMortalityLines += 1;
            }
        }

        private async Task CreateOpeningFeedingsAsync(
            List<OpeningImportRow> rows,
            IReadOnlyDictionary<string, Project> projectsByCode,
            IReadOnlyDictionary<string, Cage> cagesByCode,
            OpeningImportCommitResultDto result)
        {
            var feedingRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningFeedings") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .ToList();

            if (feedingRows.Count == 0)
            {
                return;
            }

            var stockCodes = feedingRows
                .Select(ParseRow)
                .SelectMany(x => new[]
                {
                    x.TryGetValue("fishStockCode", out var fishStockCode) ? fishStockCode : null,
                    x.TryGetValue("feedStockCode", out var feedStockCode) ? feedStockCode : null,
                })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var stocks = await _unitOfWork.Stocks.Query(tracking: true)
                .Where(x => stockCodes.Contains(x.ErpStockCode))
                .ToDictionaryAsync(x => x.ErpStockCode, x => x, StringComparer.OrdinalIgnoreCase);

            var batchesByKey = await LoadExistingBatchesByKeyAsync(feedingRows, projectsByCode);
            var headerByKey = new Dictionary<string, Feeding>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in feedingRows)
            {
                var normalized = ParseRow(row);
                var projectCode = normalized.TryGetValue("projectCode", out var projectCodeValue) ? projectCodeValue : null;
                var cageCode = normalized.TryGetValue("cageCode", out var cageCodeValue) ? cageCodeValue : null;
                var fishStockCode = normalized.TryGetValue("fishStockCode", out var fishStockCodeValue) ? fishStockCodeValue : null;
                var feedStockCode = normalized.TryGetValue("feedStockCode", out var feedStockCodeValue) ? feedStockCodeValue : null;

                if (string.IsNullOrWhiteSpace(projectCode) ||
                    string.IsNullOrWhiteSpace(cageCode) ||
                    string.IsNullOrWhiteSpace(fishStockCode) ||
                    string.IsNullOrWhiteSpace(feedStockCode))
                {
                    result.SkippedRows += 1;
                    continue;
                }

                if (!projectsByCode.TryGetValue(projectCode, out var project) ||
                    !cagesByCode.TryGetValue(cageCode, out var cage) ||
                    !stocks.TryGetValue(fishStockCode, out var fishStock) ||
                    !stocks.TryGetValue(feedStockCode, out var feedStock))
                {
                    result.SkippedRows += 1;
                    continue;
                }

                var feedingDate = ParseDateOrDefault(normalized.TryGetValue("feedingDate", out var feedingDateValue) ? feedingDateValue : null, project.StartDate);
                var projectCage = await FindProjectCageForDateAsync(project.Id, cage.Id, feedingDate);
                if (projectCage == null)
                {
                    result.SkippedRows += 1;
                    continue;
                }

                var feedingSlot = ParseFeedingSlot(normalized.TryGetValue("feedingSlot", out var feedingSlotValue) ? feedingSlotValue : null);
                var headerKey = $"{project.Id}:{feedingDate:yyyyMMdd}:{(byte)feedingSlot}";

                if (!headerByKey.TryGetValue(headerKey, out var header))
                {
                    header = await _unitOfWork.Db.Feedings
                        .Include(x => x.Lines)
                        .ThenInclude(x => x.Distributions)
                        .FirstOrDefaultAsync(x =>
                            !x.IsDeleted &&
                            x.ProjectId == project.Id &&
                            x.FeedingDate.Date == feedingDate.Date &&
                            x.FeedingSlot == feedingSlot);

                    if (header == null)
                    {
                        header = new Feeding
                        {
                            ProjectId = project.Id,
                            FeedingNo = $"OPEN-FEED-{project.ProjectCode}-{feedingDate:yyyyMMdd}-{(byte)feedingSlot}",
                            FeedingDate = feedingDate,
                            FeedingSlot = feedingSlot,
                            SourceType = FeedingSourceType.Manual,
                            Status = DocumentStatus.Posted,
                            Note = $"Opening import summary - {row.OpeningImportJobId}"
                        };

                        await _unitOfWork.Feedings.AddAsync(header);
                        result.CreatedFeedingHeaders += 1;
                    }

                    headerByKey[headerKey] = header;
                }

                var batchCode = normalized.TryGetValue("batchCode", out var batchCodeValue) ? batchCodeValue : null;
                var batch = await EnsureFishBatchAsync(project, fishStock, batchCode, normalized, batchesByKey, result);
                var feedGram = ParseDecimalOrDefault(normalized.TryGetValue("feedGram", out var feedGramValue) ? feedGramValue : null, 0m);

                var line = new FeedingLine
                {
                    Feeding = header,
                    StockId = feedStock.Id,
                    QtyUnit = 1,
                    GramPerUnit = feedGram,
                    TotalGram = feedGram,
                };

                line.Distributions.Add(new FeedingDistribution
                {
                    FishBatchId = batch.Id,
                    ProjectCageId = projectCage.Id,
                    FeedGram = feedGram,
                });

                await _unitOfWork.FeedingLines.AddAsync(line);
                result.CreatedFeedingLines += 1;
                result.CreatedFeedingDistributions += 1;
            }
        }

        private async Task CreateOpeningShipmentsAsync(
            List<OpeningImportRow> rows,
            IReadOnlyDictionary<string, Project> projectsByCode,
            IReadOnlyDictionary<string, Cage> cagesByCode,
            OpeningImportCommitResultDto result)
        {
            var shipmentRows = rows
                .Where(x => IsSheet(x.SheetName, "OpeningShipments") && (x.Status == OpeningImportRowStatus.Valid || x.Status == OpeningImportRowStatus.Warning))
                .ToList();

            if (shipmentRows.Count == 0)
            {
                return;
            }

            var stockCodes = shipmentRows
                .Select(ParseRow)
                .Select(x => x.TryGetValue("fishStockCode", out var fishStockCode) ? fishStockCode : null)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var stocks = await _unitOfWork.Stocks.Query(tracking: true)
                .Where(x => stockCodes.Contains(x.ErpStockCode))
                .ToDictionaryAsync(x => x.ErpStockCode, x => x, StringComparer.OrdinalIgnoreCase);

            var warehouseCodes = shipmentRows
                .Select(ParseRow)
                .Select(x => x.TryGetValue("targetWarehouseCode", out var warehouseCode) ? warehouseCode : null)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(x => short.Parse(x!, CultureInfo.InvariantCulture))
                .ToList();

            var warehouses = await _unitOfWork.Db.Warehouses
                .Where(x => !x.IsDeleted && warehouseCodes.Contains(x.ErpWarehouseCode))
                .ToDictionaryAsync(x => x.ErpWarehouseCode, x => x);

            var batchesByKey = await LoadExistingBatchesByKeyAsync(shipmentRows, projectsByCode);
            var headerByKey = new Dictionary<string, Shipment>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in shipmentRows)
            {
                var normalized = ParseRow(row);
                var projectCode = normalized.TryGetValue("projectCode", out var projectCodeValue) ? projectCodeValue : null;
                var cageCode = normalized.TryGetValue("cageCode", out var cageCodeValue) ? cageCodeValue : null;
                var fishStockCode = normalized.TryGetValue("fishStockCode", out var fishStockCodeValue) ? fishStockCodeValue : null;

                if (string.IsNullOrWhiteSpace(projectCode) || string.IsNullOrWhiteSpace(cageCode) || string.IsNullOrWhiteSpace(fishStockCode))
                {
                    result.SkippedRows += 1;
                    continue;
                }

                if (!projectsByCode.TryGetValue(projectCode, out var project) ||
                    !cagesByCode.TryGetValue(cageCode, out var cage) ||
                    !stocks.TryGetValue(fishStockCode, out var stock))
                {
                    result.SkippedRows += 1;
                    continue;
                }

                var shipmentDate = ParseDateOrDefault(normalized.TryGetValue("shipmentDate", out var shipmentDateValue) ? shipmentDateValue : null, project.StartDate);
                var projectCage = await FindProjectCageForDateAsync(project.Id, cage.Id, shipmentDate);
                if (projectCage == null)
                {
                    result.SkippedRows += 1;
                    continue;
                }

                var targetWarehouseId = TryResolveWarehouseIdByField(normalized, "targetWarehouseCode", warehouses);
                var headerKey = $"{project.Id}:{shipmentDate:yyyyMMdd}:{targetWarehouseId?.ToString() ?? "none"}";

                if (!headerByKey.TryGetValue(headerKey, out var header))
                {
                    header = await _unitOfWork.Db.Shipments
                        .Include(x => x.Lines)
                        .FirstOrDefaultAsync(x =>
                            !x.IsDeleted &&
                            x.ProjectId == project.Id &&
                            x.ShipmentDate.Date == shipmentDate.Date &&
                            x.TargetWarehouseId == targetWarehouseId);

                    if (header == null)
                    {
                        header = new Shipment
                        {
                            ProjectId = project.Id,
                            ShipmentNo = $"OPEN-SHP-{project.ProjectCode}-{shipmentDate:yyyyMMdd}-{(targetWarehouseId?.ToString() ?? "NA")}",
                            ShipmentDate = shipmentDate,
                            TargetWarehouseId = targetWarehouseId,
                            Status = DocumentStatus.Posted,
                            Note = $"Opening import summary - {row.OpeningImportJobId}"
                        };

                        await _unitOfWork.Shipments.AddAsync(header);
                        result.CreatedShipmentHeaders += 1;
                    }

                    headerByKey[headerKey] = header;
                }

                var batchCode = normalized.TryGetValue("batchCode", out var batchCodeValue) ? batchCodeValue : null;
                var batch = await EnsureFishBatchAsync(project, stock, batchCode, normalized, batchesByKey, result);
                var fishCount = ParseIntOrDefault(normalized.TryGetValue("fishCount", out var fishCountValue) ? fishCountValue : null, 0);
                var averageGram = ParseAverageGramOrDefault(normalized.TryGetValue("averageGram", out var averageGramValue) ? averageGramValue : null, 0m);
                var biomassGram = Math.Round(fishCount * averageGram, 3, MidpointRounding.AwayFromZero);
                var pricing = AquaLinePricingMath.NormalizeShipmentLine(
                    biomassGram,
                    normalized.TryGetValue("currencyCode", out var currencyCodeValue) ? currencyCodeValue : null,
                    ParseNullableDecimal(normalized.TryGetValue("exchangeRate", out var exchangeRateValue) ? exchangeRateValue : null),
                    ParseNullableDecimal(normalized.TryGetValue("unitPrice", out var unitPriceValue) ? unitPriceValue : null)
                );

                await _unitOfWork.ShipmentLines.AddAsync(new ShipmentLine
                {
                    Shipment = header,
                    FishBatchId = batch.Id,
                    FromProjectCageId = projectCage.Id,
                    FishCount = fishCount,
                    AverageGram = averageGram,
                    BiomassGram = biomassGram,
                    CurrencyCode = pricing.CurrencyCode,
                    ExchangeRate = pricing.ExchangeRate,
                    UnitPrice = pricing.UnitPrice,
                    LocalUnitPrice = pricing.LocalUnitPrice,
                    LineAmount = pricing.LineAmount,
                    LocalLineAmount = pricing.LocalLineAmount,
                });

                result.CreatedShipmentLines += 1;
            }
        }

        private Task<ProjectCage?> FindProjectCageForDateAsync(long projectId, long cageId, DateTime effectiveDate)
        {
            var date = effectiveDate.Date;
            var nextDate = date.AddDays(1);

            return _unitOfWork.Db.ProjectCages
                .Where(x =>
                    !x.IsDeleted &&
                    x.ProjectId == projectId &&
                    x.CageId == cageId &&
                    x.AssignedDate < nextDate &&
                    (x.ReleasedDate == null || x.ReleasedDate >= date))
                .OrderByDescending(x => x.AssignedDate)
                .FirstOrDefaultAsync();
        }

        private async Task<Dictionary<string, FishBatch>> LoadExistingBatchesByKeyAsync(
            List<OpeningImportRow> rows,
            IReadOnlyDictionary<string, Project> projectsByCode)
        {
            var projectIds = rows
                .Select(ParseRow)
                .Select(x => x.TryGetValue("projectCode", out var projectCode) && !string.IsNullOrWhiteSpace(projectCode) && projectsByCode.TryGetValue(projectCode!, out var project)
                    ? project.Id
                    : (long?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            return await _unitOfWork.Db.FishBatches
                .Where(x => !x.IsDeleted && projectIds.Contains(x.ProjectId))
                .ToDictionaryAsync(x => $"{x.ProjectId}:{x.BatchCode}", x => x, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<FishBatch> EnsureFishBatchAsync(
            Project project,
            aqua_api.Modules.Stock.Domain.Entities.Stock stock,
            string? batchCode,
            Dictionary<string, string?> normalized,
            Dictionary<string, FishBatch> batchesByKey,
            OpeningImportCommitResultDto result)
        {
            var effectiveBatchCode = ResolveBatchCode(project.ProjectCode, batchCode);
            var key = $"{project.Id}:{effectiveBatchCode}";
            if (batchesByKey.TryGetValue(key, out var existingBatch))
            {
                return existingBatch;
            }

            var averageGram = ParseAverageGramOrDefault(normalized.TryGetValue("averageGram", out var averageGramValue) ? averageGramValue : null, 0m);
            var startDate = ParseDateOrDefault(
                normalized.TryGetValue("asOfDate", out var asOfDateValue) ? asOfDateValue :
                normalized.TryGetValue("receiptDate", out var receiptDateValue) ? receiptDateValue :
                normalized.TryGetValue("mortalityDate", out var mortalityDateValue) ? mortalityDateValue : null,
                project.StartDate);

            var batch = new FishBatch
            {
                ProjectId = project.Id,
                BatchCode = effectiveBatchCode,
                FishStockId = stock.Id,
                CurrentAverageGram = averageGram,
                StartDate = startDate
            };

            await _unitOfWork.FishBatches.AddAsync(batch);
            await _unitOfWork.SaveChangesAsync();
            batchesByKey[key] = batch;
            result.CreatedFishBatches += 1;
            return batch;
        }

        private static long? TryResolveWarehouseId(
            Dictionary<string, string?> normalized,
            IReadOnlyDictionary<short, aqua_api.Modules.Warehouse.Domain.Entities.Warehouse> warehouses)
            => TryResolveWarehouseIdByField(normalized, "warehouseCode", warehouses);

        private static long? TryResolveWarehouseIdByField(
            Dictionary<string, string?> normalized,
            string fieldKey,
            IReadOnlyDictionary<short, aqua_api.Modules.Warehouse.Domain.Entities.Warehouse> warehouses)
        {
            if (!normalized.TryGetValue(fieldKey, out var warehouseCode) ||
                string.IsNullOrWhiteSpace(warehouseCode) ||
                !short.TryParse(warehouseCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedWarehouseCode) ||
                !warehouses.TryGetValue(parsedWarehouseCode, out var warehouse))
            {
                return null;
            }

            return warehouse.Id;
        }

        private static string? CreateBalanceKey(Dictionary<string, string?> row)
        {
            var projectCode = row.TryGetValue("projectCode", out var projectCodeValue) ? projectCodeValue : null;
            var batchCode = row.TryGetValue("batchCode", out var batchCodeValue) ? batchCodeValue : null;
            var fishStockCode = row.TryGetValue("fishStockCode", out var fishStockCodeValue) ? fishStockCodeValue : null;
            var cageCode = row.TryGetValue("cageCode", out var cageCodeValue) ? cageCodeValue : null;
            var warehouseCode = row.TryGetValue("warehouseCode", out var warehouseCodeValue) ? warehouseCodeValue : null;
            if (string.IsNullOrWhiteSpace(projectCode) || string.IsNullOrWhiteSpace(fishStockCode))
            {
                return null;
            }

            var effectiveBatchCode = ResolveBatchCode(projectCode!, batchCode);

            var locationPart = !string.IsNullOrWhiteSpace(cageCode)
                ? $"cage:{cageCode}"
                : !string.IsNullOrWhiteSpace(warehouseCode)
                    ? $"warehouse:{warehouseCode}"
                    : "unknown";

            return $"{projectCode}::{effectiveBatchCode}::{fishStockCode}::{locationPart}";
        }

        private static Dictionary<string, string?> NormalizeDictionary(Dictionary<string, string?> source)
        {
            return source.ToDictionary(
                x => (x.Key ?? string.Empty).Trim(),
                x => string.IsNullOrWhiteSpace(x.Value) ? null : x.Value?.Trim(),
                StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string?> ApplyMappings(
            Dictionary<string, string?> rawRow,
            List<OpeningImportFieldMappingDto>? mappings)
        {
            var normalized = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            if (mappings == null)
            {
                return normalized;
            }

            foreach (var mapping in mappings)
            {
                if (string.IsNullOrWhiteSpace(mapping.SourceColumn) || string.IsNullOrWhiteSpace(mapping.TargetField))
                {
                    continue;
                }

                rawRow.TryGetValue(mapping.SourceColumn.Trim(), out var rawValue);
                normalized[mapping.TargetField.Trim()] = string.IsNullOrWhiteSpace(rawValue) ? null : rawValue?.Trim();
            }

            return normalized;
        }

        private List<string> ValidateSheetRow(string sheetName, Dictionary<string, string?> normalized)
        {
            var messages = new List<string>();

            if (IsSheet(sheetName, "Projects"))
            {
                Require(normalized, "projectCode", messages, "OpeningImportService.Validation.ProjectCodeRequired");
                Require(normalized, "projectName", messages, "OpeningImportService.Validation.ProjectNameRequired");
                ValidateDate(normalized, "startDate", messages, "OpeningImportService.Validation.StartDateInvalid");
                return messages;
            }

            if (IsSheet(sheetName, "Cages"))
            {
                Require(normalized, "projectCode", messages, "OpeningImportService.Validation.ProjectCodeRequired");
                Require(normalized, "cageCode", messages, "OpeningImportService.Validation.CageCodeRequired");
                Require(normalized, "cageName", messages, "OpeningImportService.Validation.CageNameRequired");
                return messages;
            }

            if (IsSheet(sheetName, "OpeningStock"))
            {
                Require(normalized, "projectCode", messages, "OpeningImportService.Validation.ProjectCodeRequired");
                Require(normalized, "fishStockCode", messages, "OpeningImportService.Validation.FishStockCodeRequired");
                ValidatePositiveInt(normalized, "fishCount", messages, "OpeningImportService.Validation.FishCountMustBePositive");
                ValidatePositiveDecimal(normalized, "averageGram", messages, "OpeningImportService.Validation.AverageGramMustBePositive");
                ValidateDate(normalized, "asOfDate", messages, "OpeningImportService.Validation.OpeningDateInvalid");

                var hasCage = !string.IsNullOrWhiteSpace(normalized.TryGetValue("cageCode", out var cageCode) ? cageCode : null);
                var hasWarehouse = !string.IsNullOrWhiteSpace(normalized.TryGetValue("warehouseCode", out var warehouseCode) ? warehouseCode : null);
                if (!hasCage && !hasWarehouse)
                {
                    messages.Add(L("OpeningImportService.Validation.OpeningStockLocationRequired"));
                }

                return messages;
            }

            if (IsSheet(sheetName, "OpeningGoodsReceipts"))
            {
                Require(normalized, "projectCode", messages, "OpeningImportService.Validation.ProjectCodeRequired");
                Require(normalized, "cageCode", messages, "OpeningImportService.Validation.CageCodeRequired");
                Require(normalized, "fishStockCode", messages, "OpeningImportService.Validation.FishStockCodeRequired");
                ValidatePositiveInt(normalized, "fishCount", messages, "OpeningImportService.Validation.FishCountMustBePositive");
                ValidatePositiveDecimal(normalized, "averageGram", messages, "OpeningImportService.Validation.AverageGramMustBePositive");
                ValidateDate(normalized, "receiptDate", messages, "OpeningImportService.Validation.ReceiptDateInvalid");
                return messages;
            }

            if (IsSheet(sheetName, "OpeningMortality"))
            {
                Require(normalized, "projectCode", messages, "OpeningImportService.Validation.ProjectCodeRequired");
                Require(normalized, "cageCode", messages, "OpeningImportService.Validation.CageCodeRequired");
                Require(normalized, "fishStockCode", messages, "OpeningImportService.Validation.FishStockCodeRequired");
                ValidatePositiveInt(normalized, "deadCount", messages, "OpeningImportService.Validation.MortalityCountMustBePositive");
                ValidateNonNegativeDecimal(normalized, "mortalityBiomassKg", messages, "OpeningImportService.Validation.MortalityBiomassKgMustBeNonNegative");
                ValidateDate(normalized, "mortalityDate", messages, "OpeningImportService.Validation.MortalityDateInvalid");
                return messages;
            }

            if (IsSheet(sheetName, "OpeningFeedings"))
            {
                Require(normalized, "projectCode", messages, "OpeningImportService.Validation.ProjectCodeRequired");
                Require(normalized, "cageCode", messages, "OpeningImportService.Validation.CageCodeRequired");
                Require(normalized, "batchCode", messages, "OpeningImportService.Validation.BatchCodeRequired");
                Require(normalized, "fishStockCode", messages, "OpeningImportService.Validation.FishStockCodeRequired");
                Require(normalized, "feedStockCode", messages, "OpeningImportService.Validation.FeedStockCodeRequired");
                ValidatePositiveDecimal(normalized, "feedGram", messages, "OpeningImportService.Validation.FeedGramMustBePositive");
                ValidateDate(normalized, "feedingDate", messages, "OpeningImportService.Validation.FeedingDateInvalid");
                return messages;
            }

            if (IsSheet(sheetName, "OpeningShipments"))
            {
                Require(normalized, "projectCode", messages, "OpeningImportService.Validation.ProjectCodeRequired");
                Require(normalized, "cageCode", messages, "OpeningImportService.Validation.CageCodeRequired");
                Require(normalized, "batchCode", messages, "OpeningImportService.Validation.BatchCodeRequired");
                Require(normalized, "fishStockCode", messages, "OpeningImportService.Validation.FishStockCodeRequired");
                ValidatePositiveInt(normalized, "fishCount", messages, "OpeningImportService.Validation.ShipmentCountMustBePositive");
                ValidatePositiveDecimal(normalized, "averageGram", messages, "OpeningImportService.Validation.AverageGramMustBePositive");
                ValidateDate(normalized, "shipmentDate", messages, "OpeningImportService.Validation.ShipmentDateInvalid");
                ValidateNonNegativeDecimal(normalized, "unitPrice", messages, "OpeningImportService.Validation.UnitPriceMustBeNonNegative");
                ValidatePositiveDecimalWhenPresent(normalized, "exchangeRate", messages, "OpeningImportService.Validation.ExchangeRateMustBePositive");
                return messages;
            }

            messages.Add(L("OpeningImportService.Validation.UnsupportedSheet", sheetName));
            return messages;
        }

        private void Require(Dictionary<string, string?> values, string key, List<string> messages, string messageKey)
        {
            if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                messages.Add(L(messageKey));
            }
        }

        private void ValidatePositiveInt(Dictionary<string, string?> values, string key, List<string> messages, string messageKey)
        {
            if (!values.TryGetValue(key, out var value) || !TryParseOpeningInt(value, out var parsed) || parsed <= 0)
            {
                messages.Add(L(messageKey));
            }
        }

        private void ValidatePositiveDecimal(Dictionary<string, string?> values, string key, List<string> messages, string messageKey)
        {
            if (!values.TryGetValue(key, out var value) || !TryParseOpeningDecimal(value, out var parsed) || parsed <= 0)
            {
                messages.Add(L(messageKey));
            }
        }

        private void ValidatePositiveDecimalWhenPresent(Dictionary<string, string?> values, string key, List<string> messages, string messageKey)
        {
            if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!TryParseOpeningDecimal(value, out var parsed) || parsed <= 0)
            {
                messages.Add(L(messageKey));
            }
        }

        private void ValidateNonNegativeDecimal(Dictionary<string, string?> values, string key, List<string> messages, string messageKey)
        {
            if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!TryParseOpeningDecimal(value, out var parsed) || parsed < 0)
            {
                messages.Add(L(messageKey));
            }
        }

        private void ValidateDate(Dictionary<string, string?> values, string key, List<string> messages, string messageKey)
        {
            if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!TryParseOpeningDate(value, out _))
            {
                messages.Add(L(messageKey));
            }
        }

        private static void AppendDuplicateErrors(IEnumerable<StagedRow> rows, string field, Func<string, string> messageFactory)
        {
            AppendDuplicateErrors(rows, row => GetValue(row, field), messageFactory);
        }

        private static void AppendDuplicateErrors(IEnumerable<StagedRow> rows, Func<StagedRow, string?> selector, Func<string, string> messageFactory)
        {
            var duplicates = rows
                .GroupBy(selector, StringComparer.OrdinalIgnoreCase)
                .Where(x => !string.IsNullOrWhiteSpace(x.Key) && x.Count() > 1)
                .ToList();

            foreach (var duplicate in duplicates)
            {
                foreach (var row in duplicate)
                {
                    row.Messages.Add(messageFactory(duplicate.Key!));
                }
            }
        }

        private static void AppendOpeningGoodsReceiptHeaderErrors(IEnumerable<StagedRow> rows, string message)
        {
            foreach (var group in rows
                         .Where(x => !string.IsNullOrWhiteSpace(GetValue(x, "projectCode")))
                         .GroupBy(x => GetValue(x, "projectCode")!, StringComparer.OrdinalIgnoreCase))
            {
                var receiptNumbers = group
                    .Select(x => GetValue(x, "receiptNo")?.Trim() ?? string.Empty)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var receiptDates = group
                    .Select(x => GetValue(x, "receiptDate")?.Trim() ?? string.Empty)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var warehouseCodes = group
                    .Select(x => GetValue(x, "warehouseCode")?.Trim() ?? string.Empty)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (receiptNumbers.Count <= 1 && receiptDates.Count <= 1 && warehouseCodes.Count <= 1)
                {
                    continue;
                }

                foreach (var row in group)
                {
                    row.Messages.Add(message);
                }
            }
        }

        private static void PropagateOpeningGoodsReceiptHeaderValues(IEnumerable<StagedRow> rows)
        {
            foreach (var group in rows
                         .Where(x => !string.IsNullOrWhiteSpace(GetValue(x, "projectCode")))
                         .GroupBy(x => GetValue(x, "projectCode")!, StringComparer.OrdinalIgnoreCase))
            {
                foreach (var field in new[] { "receiptNo", "receiptDate", "warehouseCode" })
                {
                    var headerValue = group
                        .Select(x => GetValue(x, field))
                        .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                    if (string.IsNullOrWhiteSpace(headerValue))
                    {
                        continue;
                    }

                    foreach (var row in group.Where(x => string.IsNullOrWhiteSpace(GetValue(x, field))))
                    {
                        row.NormalizedData[field] = headerValue;
                    }
                }
            }
        }

        private static void AppendOpeningGoodsReceiptBatchErrors(IEnumerable<StagedRow> rows, string message)
        {
            foreach (var group in rows
                         .Where(x => !string.IsNullOrWhiteSpace(GetValue(x, "projectCode")))
                         .GroupBy(
                             x => $"{GetValue(x, "projectCode")}::{ResolveBatchCode(GetValue(x, "projectCode")!, GetValue(x, "batchCode"))}",
                             StringComparer.OrdinalIgnoreCase))
            {
                var stockCodes = group
                    .Select(x => GetValue(x, "fishStockCode")?.Trim() ?? string.Empty)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var averageGrams = group
                    .Select(x => TryParseOpeningDecimal(GetValue(x, "averageGram"), out var parsed)
                        ? parsed.ToString(CultureInfo.InvariantCulture)
                        : GetValue(x, "averageGram")?.Trim() ?? string.Empty)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (stockCodes.Count <= 1 && averageGrams.Count <= 1)
                {
                    continue;
                }

                foreach (var row in group)
                {
                    row.Messages.Add(message);
                }
            }
        }

        private static bool HasOpeningGoodsReceiptHeaderConflicts(IEnumerable<OpeningImportRow> rows)
        {
            return rows
                .Where(x => IsSheet(x.SheetName, "OpeningGoodsReceipts"))
                .Select(ParseRow)
                .Where(x => x.TryGetValue("projectCode", out var projectCode) && !string.IsNullOrWhiteSpace(projectCode))
                .GroupBy(x => x["projectCode"]!, StringComparer.OrdinalIgnoreCase)
                .Any(group =>
                    group.Select(x => x.TryGetValue("receiptNo", out var value) ? value?.Trim() ?? string.Empty : string.Empty).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1 ||
                    group.Select(x => x.TryGetValue("receiptDate", out var value) ? value?.Trim() ?? string.Empty : string.Empty).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1 ||
                    group.Select(x => x.TryGetValue("warehouseCode", out var value) ? value?.Trim() ?? string.Empty : string.Empty).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1);
        }

        private static string? GetValue(StagedRow row, string key)
        {
            return row.NormalizedData.TryGetValue(key, out var value) ? value : null;
        }

        private static Dictionary<string, string?> ParseRow(OpeningImportRow row)
        {
            return JsonSerializer.Deserialize<Dictionary<string, string?>>(row.NormalizedDataJson ?? "{}", JsonOptions)
                ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }

        private static OpeningImportRowStatus ResolveRowStatus(List<string> messages)
        {
            return messages.Count == 0 ? OpeningImportRowStatus.Valid : OpeningImportRowStatus.Error;
        }

        private static bool IsSheet(string? actual, string expected)
        {
            return string.Equals(actual?.Trim(), expected, StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveBatchCode(string projectCode, string? batchCode)
        {
            return string.IsNullOrWhiteSpace(batchCode) ? $"OPEN-{projectCode}" : batchCode.Trim();
        }

        private static FeedingSlot ParseFeedingSlot(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return FeedingSlot.Morning;
            }

            if (byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric) &&
                Enum.IsDefined(typeof(FeedingSlot), (int)numeric))
            {
                return (FeedingSlot)numeric;
            }

            var normalized = value.Trim().ToUpperInvariant();
            return normalized switch
            {
                "EVENING" or "2" or "2. TUR" or "ROUND2" or "ROUND 2" => FeedingSlot.Evening,
                _ => FeedingSlot.Morning,
            };
        }

        private static OpeningImportSummaryDto BuildSummary(List<StagedRow> rows)
        {
            return new OpeningImportSummaryDto
            {
                TotalRows = rows.Count,
                ValidRows = rows.Count(x => x.Entity.Status == OpeningImportRowStatus.Valid),
                WarningRows = rows.Count(x => x.Entity.Status == OpeningImportRowStatus.Warning),
                ErrorRows = rows.Count(x => x.Entity.Status == OpeningImportRowStatus.Error)
            };
        }

        private static OpeningImportPreviewResponseDto BuildPreviewResponse(OpeningImportJob job, List<OpeningImportRow> rows)
        {
            var summary = JsonSerializer.Deserialize<OpeningImportSummaryDto>(job.SummaryJson ?? "{}", JsonOptions)
                ?? new OpeningImportSummaryDto();

            return new OpeningImportPreviewResponseDto
            {
                JobId = job.Id,
                Status = job.Status.ToString(),
                Summary = summary,
                Rows = rows
                    .OrderBy(x => x.SheetName)
                    .ThenBy(x => x.RowNumber)
                    .Select(x => new OpeningImportRowResultDto
                    {
                        RowId = x.Id,
                        SheetName = x.SheetName,
                        RowNumber = x.RowNumber,
                        Status = x.Status.ToString(),
                        Messages = JsonSerializer.Deserialize<List<string>>(x.MessagesJson ?? "[]", JsonOptions) ?? new List<string>(),
                        RawData = JsonSerializer.Deserialize<Dictionary<string, string?>>(x.RawDataJson, JsonOptions) ?? new Dictionary<string, string?>(),
                        NormalizedData = JsonSerializer.Deserialize<Dictionary<string, string?>>(x.NormalizedDataJson ?? "{}", JsonOptions) ?? new Dictionary<string, string?>()
                    })
                    .ToList()
            };
        }

        private static DateTime ParseDateOrDefault(string? rawValue, DateTime fallback)
        {
            return TryParseOpeningDate(rawValue, out var parsed)
                ? parsed
                : fallback;
        }

        private static DateTime? ParseNullableDate(string? rawValue)
        {
            return TryParseOpeningDate(rawValue, out var parsed)
                ? parsed
                : null;
        }

        private static bool TryParseOpeningDate(string? rawValue, out DateTime parsed)
        {
            parsed = default;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            var value = rawValue.Trim();
            if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var serialDate) &&
                serialDate >= 1 &&
                serialDate <= 2958465)
            {
                parsed = DateTime.FromOADate(serialDate).Date;
                return true;
            }

            var exactFormats = new[]
            {
                "yyyy-MM-dd",
                "yyyy/M/d",
                "yyyy.MM.dd",
                "dd.MM.yyyy",
                "d.M.yyyy",
                "dd/MM/yyyy",
                "d/M/yyyy",
                "dd-MM-yyyy",
                "d-M-yyyy",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd HH:mm:ss"
            };

            if (DateTime.TryParseExact(value, exactFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsed))
            {
                parsed = parsed.Date;
                return true;
            }

            if (DateTime.TryParse(value, new CultureInfo("tr-TR"), DateTimeStyles.AssumeLocal, out parsed) ||
                DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsed))
            {
                parsed = parsed.Date;
                return true;
            }

            return false;
        }

        private static int ParseIntOrDefault(string? rawValue, int fallback)
        {
            return TryParseOpeningInt(rawValue, out var parsed)
                ? parsed
                : fallback;
        }

        private static decimal ParseDecimalOrDefault(string? rawValue, decimal fallback)
        {
            return TryParseOpeningDecimal(rawValue, out var parsed)
                ? parsed
                : fallback;
        }

        private static decimal ParseAverageGramOrDefault(string? rawValue, decimal fallback)
        {
            if (!TryParseOpeningDecimal(rawValue, out var parsed))
            {
                return fallback;
            }

            if (parsed <= 10000m || string.IsNullOrWhiteSpace(rawValue))
            {
                return parsed;
            }

            var value = rawValue.Trim()
                .Replace(" ", string.Empty)
                .Replace("\u00A0", string.Empty)
                .Replace(',', '.');

            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalParsed) &&
                   decimalParsed > 0 &&
                   decimalParsed <= 10000m
                ? decimalParsed
                : parsed;
        }

        private static decimal? ParseNullableDecimal(string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            return TryParseOpeningDecimal(rawValue, out var parsed)
                ? parsed
                : null;
        }

        private static bool TryParseOpeningInt(string? rawValue, out int parsed)
        {
            parsed = 0;
            if (!TryParseOpeningDecimal(rawValue, out var decimalValue))
            {
                return false;
            }

            var rounded = decimal.Round(decimalValue, 0, MidpointRounding.AwayFromZero);
            if (rounded < int.MinValue || rounded > int.MaxValue)
            {
                return false;
            }

            parsed = (int)rounded;
            return true;
        }

        private static bool TryParseOpeningDecimal(string? rawValue, out decimal parsed)
        {
            parsed = 0m;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            var value = rawValue.Trim()
                .Replace(" ", string.Empty)
                .Replace("\u00A0", string.Empty);

            if (ShouldNormalizeBeforeCultureParse(value))
            {
                value = NormalizeOpeningNumber(value);
            }

            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed))
            {
                return true;
            }

            if (decimal.TryParse(value, NumberStyles.Number, new CultureInfo("tr-TR"), out parsed))
            {
                return true;
            }

            var normalized = NormalizeOpeningNumber(value);
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed);
        }

        private static bool ShouldNormalizeBeforeCultureParse(string value)
        {
            var lastComma = value.LastIndexOf(',');
            var lastDot = value.LastIndexOf('.');
            return (lastComma >= 0 && lastDot >= 0) ||
                   (lastComma >= 0 && ShouldTreatSingleSeparatorAsGrouping(value, lastComma)) ||
                   (lastDot >= 0 && ShouldTreatSingleSeparatorAsGrouping(value, lastDot));
        }

        private static string NormalizeOpeningNumber(string value)
        {
            var lastComma = value.LastIndexOf(',');
            var lastDot = value.LastIndexOf('.');

            if (lastComma >= 0 && lastDot >= 0)
            {
                var decimalSeparator = lastComma > lastDot ? ',' : '.';
                var groupingSeparator = decimalSeparator == ',' ? "." : ",";
                return value
                    .Replace(groupingSeparator, string.Empty)
                    .Replace(decimalSeparator, '.');
            }

            if (lastComma >= 0)
            {
                return ShouldTreatSingleSeparatorAsGrouping(value, lastComma)
                    ? value.Replace(",", string.Empty)
                    : value.Replace(',', '.');
            }

            if (lastDot >= 0)
            {
                return ShouldTreatSingleSeparatorAsGrouping(value, lastDot)
                    ? value.Replace(".", string.Empty)
                    : value;
            }

            return value;
        }

        private static bool ShouldTreatSingleSeparatorAsGrouping(string value, int separatorIndex)
        {
            var digitsAfter = value.Length - separatorIndex - 1;
            if (digitsAfter != 3)
            {
                return false;
            }

            return value[(separatorIndex + 1)..].All(char.IsDigit);
        }

        private static decimal ResolveAverageGram(BatchCageBalance? balance)
        {
            if (balance == null)
            {
                return 0m;
            }

            if (balance.AverageGram > 0)
            {
                return balance.AverageGram;
            }

            return balance.LiveCount > 0 && balance.BiomassGram > 0
                ? Math.Round(balance.BiomassGram / balance.LiveCount, 3, MidpointRounding.AwayFromZero)
                : 0m;
        }

        private sealed class StagedRow
        {
            public string SheetName { get; set; } = string.Empty;
            public int RowNumber { get; set; }
            public Dictionary<string, string?> RawData { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, string?> NormalizedData { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public List<string> Messages { get; set; } = new();
            public OpeningImportRow Entity { get; set; } = new();
        }
    }
}
