using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
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
                await CreateSummaryDocumentsAsync(committedRows, projectsByCode, cagesByCode, result);
                await ApplyOpeningBalancesAsync(committedRows, projectsByCode, cagesByCode, result);

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

            AppendDuplicateErrors(projectRows, "projectCode", value => $"Aynı proje kodu dosyada tekrar ediyor: {value}");
            AppendDuplicateErrors(cageRows, row => $"{GetValue(row, "projectCode")}::{GetValue(row, "cageCode")}", value => $"Aynı proje/kafes eşleşmesi dosyada tekrar ediyor: {value}");

            var referencedProjectCodes = rows
                .Select(x => GetValue(x, "projectCode"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingProjects = await _unitOfWork.Projects.Query()
                .Where(x => referencedProjectCodes.Contains(x.ProjectCode))
                .ToDictionaryAsync(x => x.ProjectCode, x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var row in cageRows.Concat(stockRows).Concat(goodsReceiptRows).Concat(mortalityRows))
            {
                var projectCode = GetValue(row, "projectCode");
                if (string.IsNullOrWhiteSpace(projectCode))
                {
                    continue;
                }

                var existsInFile = projectRows.Any(x => string.Equals(GetValue(x, "projectCode"), projectCode, StringComparison.OrdinalIgnoreCase));
                if (!existsInFile && !existingProjects.ContainsKey(projectCode))
                {
                    row.Messages.Add($"Proje bulunamadı: {projectCode}");
                }
            }

            var openingStockCodes = stockRows
                .Concat(goodsReceiptRows)
                .Concat(mortalityRows)
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
                    row.Messages.Add($"Stok bulunamadı: {stockCode}");
                }
            }

            var warehouseCodes = stockRows
                .Concat(goodsReceiptRows)
                .Select(x => GetValue(x, "warehouseCode"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
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

            foreach (var row in stockRows)
            {
                var warehouseCode = GetValue(row, "warehouseCode");
                if (string.IsNullOrWhiteSpace(warehouseCode))
                {
                    continue;
                }

                if (!short.TryParse(warehouseCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedWarehouse) ||
                    !existingWarehouses.ContainsKey(parsedWarehouse))
                {
                    row.Messages.Add($"Depo bulunamadı: {warehouseCode}");
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

            var activeAssignments = await _unitOfWork.Db.ProjectCages
                .AsNoTracking()
                .Include(x => x.Project)
                .Include(x => x.Cage)
                .Where(x => !x.IsDeleted && x.ReleasedDate == null && cageCodes.Contains(x.Cage!.CageCode))
                .ToListAsync();

            foreach (var row in cageRows.Concat(stockRows).Concat(goodsReceiptRows).Concat(mortalityRows))
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
                    row.Messages.Add($"Kafes bulunamadı: {cageCode}");
                }

                var conflictingAssignment = activeAssignments.FirstOrDefault(x =>
                    string.Equals(x.Cage?.CageCode, cageCode, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(x.Project?.ProjectCode, projectCode, StringComparison.OrdinalIgnoreCase));

                if (conflictingAssignment != null)
                {
                    row.Messages.Add($"Kafes başka projeye aktif atanmış: {cageCode} -> {conflictingAssignment.Project?.ProjectCode}");
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

            foreach (var row in projectRows)
            {
                var projectCode = row["projectCode"] ?? string.Empty;
                if (existing.ContainsKey(projectCode))
                {
                    continue;
                }

                var status = ParseProjectStatus(row.TryGetValue("status", out var statusValue) ? statusValue : null);
                var startDate = ParseDateOrDefault(row.TryGetValue("startDate", out var startDateValue) ? startDateValue : null, DateTimeProvider.Now.Date);

                var entity = new Project
                {
                    ProjectCode = projectCode,
                    ProjectName = row.TryGetValue("projectName", out var projectName) && !string.IsNullOrWhiteSpace(projectName)
                        ? projectName
                        : projectCode,
                    StartDate = startDate,
                    Status = status,
                    Note = row.TryGetValue("note", out var note) ? note : null
                };

                await _unitOfWork.Projects.AddAsync(entity);
                existing[projectCode] = entity;
                result.CreatedProjects += 1;
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

            var allCageCodes = cageRows
                .Concat(stockRows)
                .Concat(goodsReceiptRows)
                .Concat(mortalityRows)
                .Select(x => x["cageCode"] ?? string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var cagesByCode = await _unitOfWork.Cages.Query(tracking: true)
                .Where(x => allCageCodes.Contains(x.CageCode))
                .ToDictionaryAsync(x => x.CageCode, x => x, StringComparer.OrdinalIgnoreCase);

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
            }

            foreach (var row in cageRows.Concat(stockRows).Concat(goodsReceiptRows).Concat(mortalityRows))
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

                var existingAssignment = await _unitOfWork.Db.ProjectCages
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.CageId == cage.Id && x.ReleasedDate == null);

                if (existingAssignment != null)
                {
                    continue;
                }

                var conflictingAssignment = await _unitOfWork.Db.ProjectCages
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.CageId == cage.Id && x.ReleasedDate == null);

                if (conflictingAssignment != null)
                {
                    continue;
                }

                await _unitOfWork.ProjectCages.AddAsync(new ProjectCage
                {
                    ProjectId = project.Id,
                    CageId = cage.Id,
                    AssignedDate = ParseDateOrDefault(row.TryGetValue("assignedDate", out var assignedDate) ? assignedDate : null, project.StartDate)
                });
                result.CreatedProjectCages += 1;
            }

            return cagesByCode;
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

                var averageGram = ParseDecimalOrDefault(normalized.TryGetValue("averageGram", out var averageGramValue) ? averageGramValue : null, 0m);
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
                    result.CreatedFishBatches += 1;
                }

                var fishCount = ParseIntOrDefault(normalized.TryGetValue("fishCount", out var fishCountValue) ? fishCountValue : null, 0);
                var biomassGram = Math.Round(fishCount * averageGram, 3, MidpointRounding.AwayFromZero);

                if (!string.IsNullOrWhiteSpace(cageCode) && cagesByCode.TryGetValue(cageCode, out var cage))
                {
                    var projectCage = await _unitOfWork.Db.ProjectCages
                        .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.CageId == cage.Id && x.ReleasedDate == null);

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
                        "Net açılış stok, özet mal kabul ve ölüm satırlarından türetildi."
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
                var headerKey = $"{project.Id}:{receiptNo}";

                if (!headerByKey.TryGetValue(headerKey, out var header))
                {
                    header = await _unitOfWork.Db.GoodsReceipts
                        .Include(x => x.Lines)
                        .ThenInclude(x => x.FishDistributions)
                        .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.ReceiptNo == receiptNo);

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
                var batch = await EnsureFishBatchAsync(project, stock, batchCode, normalized, batchesByKey, result);

                var line = new GoodsReceiptLine
                {
                    GoodsReceipt = header,
                    ItemType = GoodsReceiptItemType.Fish,
                    StockId = stock.Id,
                    FishCount = ParseIntOrDefault(normalized.TryGetValue("fishCount", out var fishCountValue) ? fishCountValue : null, 0),
                    FishAverageGram = ParseDecimalOrDefault(normalized.TryGetValue("averageGram", out var averageGramValue) ? averageGramValue : null, 0m),
                    FishTotalGram = Math.Round(
                        ParseIntOrDefault(normalized.TryGetValue("fishCount", out var lineFishCountValue) ? lineFishCountValue : null, 0)
                        * ParseDecimalOrDefault(normalized.TryGetValue("averageGram", out var lineAverageGramValue) ? lineAverageGramValue : null, 0m),
                        3,
                        MidpointRounding.AwayFromZero),
                    FishBatchId = batch.Id
                };

                if (!string.IsNullOrWhiteSpace(normalized.TryGetValue("cageCode", out var cageCodeValue) ? cageCodeValue : null) &&
                    cagesByCode.TryGetValue(cageCodeValue!, out var cage))
                {
                    var projectCage = await _unitOfWork.Db.ProjectCages
                        .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.CageId == cage.Id && x.ReleasedDate == null);

                    if (projectCage != null)
                    {
                        line.FishDistributions.Add(new GoodsReceiptFishDistribution
                        {
                            ProjectCageId = projectCage.Id,
                            FishBatchId = batch.Id,
                            FishCount = line.FishCount ?? 0
                        });
                    }
                }

                await _unitOfWork.GoodsReceiptLines.AddAsync(line);
                result.CreatedGoodsReceiptLines += 1;
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

                var projectCage = await _unitOfWork.Db.ProjectCages
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectId == project.Id && x.CageId == cage.Id && x.ReleasedDate == null);
                if (projectCage == null)
                {
                    result.SkippedRows += 1;
                    continue;
                }

                var mortalityDate = ParseDateOrDefault(normalized.TryGetValue("mortalityDate", out var mortalityDateValue) ? mortalityDateValue : null, project.StartDate);
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
            var effectiveBatchCode = string.IsNullOrWhiteSpace(batchCode) ? $"OPEN-{project.ProjectCode}" : batchCode.Trim();
            var key = $"{project.Id}:{effectiveBatchCode}";
            if (batchesByKey.TryGetValue(key, out var existingBatch))
            {
                return existingBatch;
            }

            var averageGram = ParseDecimalOrDefault(normalized.TryGetValue("averageGram", out var averageGramValue) ? averageGramValue : null, 0m);
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
            batchesByKey[key] = batch;
            result.CreatedFishBatches += 1;
            return batch;
        }

        private static long? TryResolveWarehouseId(
            Dictionary<string, string?> normalized,
            IReadOnlyDictionary<short, aqua_api.Modules.Warehouse.Domain.Entities.Warehouse> warehouses)
        {
            if (!normalized.TryGetValue("warehouseCode", out var warehouseCode) ||
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
            if (string.IsNullOrWhiteSpace(projectCode) || string.IsNullOrWhiteSpace(batchCode) || string.IsNullOrWhiteSpace(fishStockCode))
            {
                return null;
            }

            var locationPart = !string.IsNullOrWhiteSpace(cageCode)
                ? $"cage:{cageCode}"
                : !string.IsNullOrWhiteSpace(warehouseCode)
                    ? $"warehouse:{warehouseCode}"
                    : "unknown";

            return $"{projectCode}::{batchCode}::{fishStockCode}::{locationPart}";
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

        private static List<string> ValidateSheetRow(string sheetName, Dictionary<string, string?> normalized)
        {
            var messages = new List<string>();

            if (IsSheet(sheetName, "Projects"))
            {
                Require(normalized, "projectCode", messages, "Proje kodu zorunludur.");
                Require(normalized, "projectName", messages, "Proje adı zorunludur.");
                ValidateDate(normalized, "startDate", messages, "Başlangıç tarihi geçersiz.");
                return messages;
            }

            if (IsSheet(sheetName, "Cages"))
            {
                Require(normalized, "projectCode", messages, "Proje kodu zorunludur.");
                Require(normalized, "cageCode", messages, "Kafes kodu zorunludur.");
                Require(normalized, "cageName", messages, "Kafes adı zorunludur.");
                return messages;
            }

            if (IsSheet(sheetName, "OpeningStock"))
            {
                Require(normalized, "projectCode", messages, "Proje kodu zorunludur.");
                Require(normalized, "batchCode", messages, "Batch kodu zorunludur.");
                Require(normalized, "fishStockCode", messages, "Balık stok kodu zorunludur.");
                ValidatePositiveInt(normalized, "fishCount", messages, "Balık adedi 0'dan büyük olmalıdır.");
                ValidatePositiveDecimal(normalized, "averageGram", messages, "Ortalama gram 0'dan büyük olmalıdır.");
                ValidateDate(normalized, "asOfDate", messages, "Açılış tarihi geçersiz.");

                var hasCage = !string.IsNullOrWhiteSpace(normalized.TryGetValue("cageCode", out var cageCode) ? cageCode : null);
                var hasWarehouse = !string.IsNullOrWhiteSpace(normalized.TryGetValue("warehouseCode", out var warehouseCode) ? warehouseCode : null);
                if (hasCage == hasWarehouse)
                {
                    messages.Add("Açılış stok satırında ya kafes kodu ya da depo kodu verilmelidir.");
                }

                return messages;
            }

            if (IsSheet(sheetName, "OpeningGoodsReceipts"))
            {
                Require(normalized, "projectCode", messages, "Proje kodu zorunludur.");
                Require(normalized, "cageCode", messages, "Kafes kodu zorunludur.");
                Require(normalized, "batchCode", messages, "Batch kodu zorunludur.");
                Require(normalized, "fishStockCode", messages, "Balık stok kodu zorunludur.");
                ValidatePositiveInt(normalized, "fishCount", messages, "Balık adedi 0'dan büyük olmalıdır.");
                ValidatePositiveDecimal(normalized, "averageGram", messages, "Ortalama gram 0'dan büyük olmalıdır.");
                ValidateDate(normalized, "receiptDate", messages, "Mal kabul tarihi geçersiz.");
                return messages;
            }

            if (IsSheet(sheetName, "OpeningMortality"))
            {
                Require(normalized, "projectCode", messages, "Proje kodu zorunludur.");
                Require(normalized, "cageCode", messages, "Kafes kodu zorunludur.");
                Require(normalized, "batchCode", messages, "Batch kodu zorunludur.");
                Require(normalized, "fishStockCode", messages, "Balık stok kodu zorunludur.");
                ValidatePositiveInt(normalized, "deadCount", messages, "Ölüm adedi 0'dan büyük olmalıdır.");
                ValidateDate(normalized, "mortalityDate", messages, "Ölüm tarihi geçersiz.");
                return messages;
            }

            messages.Add($"Desteklenmeyen sheet: {sheetName}");
            return messages;
        }

        private static void Require(Dictionary<string, string?> values, string key, List<string> messages, string message)
        {
            if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                messages.Add(message);
            }
        }

        private static void ValidatePositiveInt(Dictionary<string, string?> values, string key, List<string> messages, string message)
        {
            if (!values.TryGetValue(key, out var value) || !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
            {
                messages.Add(message);
            }
        }

        private static void ValidatePositiveDecimal(Dictionary<string, string?> values, string key, List<string> messages, string message)
        {
            if (!values.TryGetValue(key, out var value) || !decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
            {
                messages.Add(message);
            }
        }

        private static void ValidateDate(Dictionary<string, string?> values, string key, List<string> messages, string message)
        {
            if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out _))
            {
                messages.Add(message);
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

        private static DocumentStatus ParseProjectStatus(string? rawStatus)
        {
            if (string.IsNullOrWhiteSpace(rawStatus))
            {
                return DocumentStatus.Draft;
            }

            if (byte.TryParse(rawStatus, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedNumeric) &&
                Enum.IsDefined(typeof(DocumentStatus), parsedNumeric))
            {
                return (DocumentStatus)parsedNumeric;
            }

            return rawStatus.Trim().ToLowerInvariant() switch
            {
                "draft" or "taslak" => DocumentStatus.Draft,
                "posted" or "postlandı" or "postlandi" => DocumentStatus.Posted,
                "cancelled" or "iptal" => DocumentStatus.Cancelled,
                _ => DocumentStatus.Draft
            };
        }

        private static DateTime ParseDateOrDefault(string? rawValue, DateTime fallback)
        {
            return DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed)
                ? parsed
                : fallback;
        }

        private static int ParseIntOrDefault(string? rawValue, int fallback)
        {
            return int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : fallback;
        }

        private static decimal ParseDecimalOrDefault(string? rawValue, decimal fallback)
        {
            return decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : fallback;
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
