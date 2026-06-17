using AutoMapper;
using aqua_api.Modules.Integrations.Infrastructure.Options;
using aqua_api.Shared.Infrastructure.Time;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace aqua_api.Modules.Mortalities.Application.Services
{
    public class MortalityService : IMortalityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMortalityRepository _mortalityRepository;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;
        private readonly INetsisItemSlipService _netsisItemSlipService;
        private readonly NetsisOptions _netsisOptions;

        public MortalityService(
            IUnitOfWork unitOfWork,
            IMortalityRepository mortalityRepository,
            IBalanceLedgerManager balanceLedgerManager,
            IMapper mapper,
            ILocalizationService localizationService,
            INetsisItemSlipService netsisItemSlipService,
            IOptions<NetsisOptions> netsisOptions)
        {
            _unitOfWork = unitOfWork;
            _mortalityRepository = mortalityRepository;
            _balanceLedgerManager = balanceLedgerManager;
            _mapper = mapper;
            _localizationService = localizationService;
            _netsisItemSlipService = netsisItemSlipService;
            _netsisOptions = netsisOptions.Value;
        }

        public async Task<ApiResponse<MortalityDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Mortalities
                    .Query()
                    .Include(x => x.Project)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<MortalityDto>.ErrorResult(
                        _localizationService.GetLocalizedString("MortalityService.NotFound"),
                        _localizationService.GetLocalizedString("MortalityService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<MortalityDto>(entity);
                return ApiResponse<MortalityDto>.SuccessResult(dto, _localizationService.GetLocalizedString("MortalityService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<MortalityDto>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<MortalityDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Mortalities
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.Project)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(Mortality.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<MortalityDto>(x)).ToList();

                var pagedResponse = new PagedResponse<MortalityDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<MortalityDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("MortalityService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<MortalityDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<MortalityDto>> CreateAsync(CreateMortalityDto dto)
        {
            try
            {
                var entity = _mapper.Map<Mortality>(dto);
                entity.MortalityNo = BuildDocumentNo(entity.ProjectId, entity.MortalityDate);
                await _unitOfWork.Mortalities.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<MortalityDto>(entity);
                return ApiResponse<MortalityDto>.SuccessResult(result, _localizationService.GetLocalizedString("MortalityService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<MortalityDto>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<MortalityDto>> UpdateAsync(long id, UpdateMortalityDto dto)
        {
            try
            {
                var repo = _unitOfWork.Mortalities;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<MortalityDto>.ErrorResult(
                        _localizationService.GetLocalizedString("MortalityService.NotFound"),
                        _localizationService.GetLocalizedString("MortalityService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                entity.MortalityNo = string.IsNullOrWhiteSpace(entity.MortalityNo)
                    ? BuildDocumentNo(entity.ProjectId, entity.MortalityDate)
                    : entity.MortalityNo.Trim();
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<MortalityDto>(entity);
                return ApiResponse<MortalityDto>.SuccessResult(result, _localizationService.GetLocalizedString("MortalityService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<MortalityDto>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.Mortalities;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("MortalityService.NotFound"),
                        _localizationService.GetLocalizedString("MortalityService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("MortalityService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> Post(long mortalityId, long userId)
        {
            try
            {
                var mortality = await _mortalityRepository.GetForPost(mortalityId)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.MortalityNotFound"));

                EnsureDraftStatus(mortality.Status, nameof(Mortality));

                var postLines = new List<MortalityPostLine>();
                foreach (var line in mortality.Lines.Where(x => !x.IsDeleted))
                {
                    var balance = await _unitOfWork.Db.BatchCageBalances
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.FishBatchId == line.FishBatchId && x.ProjectCageId == line.ProjectCageId && !x.IsDeleted);

                    var averageGram = ResolveAverageGram(balance);
                    if (averageGram <= 0)
                    {
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.AverageGramMissing"));
                    }

                    if (balance == null || balance.LiveCount < line.DeadCount)
                    {
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.InsufficientBalance"));
                    }

                    var biomassDelta = -Math.Round(averageGram * line.DeadCount, 3, MidpointRounding.AwayFromZero);
                    postLines.Add(new MortalityPostLine(line, averageGram, biomassDelta));
                }

                var itemSlipRequest = BuildMortalityWarehouseIssueRequest(mortality, postLines);
                var itemSlipResponse = mortality.IsERPIntegrated
                    ? null
                    : await _netsisItemSlipService.CreateWarehouseTransferOutAsync(itemSlipRequest);
                var erpReferenceNumber = itemSlipResponse == null
                    ? mortality.ERPReferenceNumber ?? mortality.MortalityNo
                    : ResolveErpReferenceNumber(itemSlipResponse, mortality.MortalityNo ?? $"MORT-{mortality.Id}");

                await _unitOfWork.BeginTransaction();

                foreach (var postLine in postLines)
                {
                    await _balanceLedgerManager.ApplyDelta(
                        mortality.ProjectId,
                        postLine.Line.FishBatchId,
                        postLine.Line.ProjectCageId,
                        -postLine.Line.DeadCount,
                        postLine.BiomassDelta,
                        BatchMovementType.Mortality,
                        mortality.MortalityDate,
                        "Mortality",
                        "RII_Mortality",
                        mortality.Id,
                        postLine.Line.ProjectCageId,
                        null,
                        null,
                        null,
                        postLine.AverageGram,
                        postLine.AverageGram,
                        userId);
                }

                var trackedMortality = await _unitOfWork.Mortalities.GetByIdForUpdateAsync(mortality.Id)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.MortalityNotFound"));

                trackedMortality.Status = DocumentStatus.Posted;
                trackedMortality.IsERPIntegrated = true;
                trackedMortality.ERPReferenceNumber = erpReferenceNumber;
                trackedMortality.ERPIntegrationDate = DateTimeProvider.UtcNow;
                trackedMortality.ERPIntegrationStatus = "Success";
                trackedMortality.ERPErrorMessage = null;
                trackedMortality.CountTriedBy = (trackedMortality.CountTriedBy ?? 0) + 1;
                trackedMortality.UpdatedBy = userId;
                trackedMortality.UpdatedDate = DateTimeProvider.UtcNow;

                await _unitOfWork.SaveChanges();
                await _unitOfWork.Commit();

                return ApiResponse<bool>.SuccessResult(
                    true,
                    _localizationService.GetLocalizedString("MortalityService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                await MarkErpPostFailedAsync(mortalityId, userId, ex.Message);
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityService.BusinessRuleError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                await MarkErpPostFailedAsync(mortalityId, userId, ex.Message);
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> PostAquaAndQueueErpAsync(long mortalityId, long userId)
        {
            try
            {
                var mortality = await _mortalityRepository.GetForPost(mortalityId)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.MortalityNotFound"));

                if (mortality.IsERPIntegrated)
                {
                    return ApiResponse<bool>.SuccessResult(
                        true,
                        _localizationService.GetLocalizedString("MortalityService.OperationSuccessful"));
                }

                if (mortality.Status == DocumentStatus.Posted)
                {
                    await MarkErpPendingAsync(mortality.Id, userId);
                    return ApiResponse<bool>.SuccessResult(
                        true,
                        _localizationService.GetLocalizedString("MortalityService.OperationSuccessful"));
                }

                EnsureDraftStatus(mortality.Status, nameof(Mortality));

                var postLines = await BuildMortalityPostLinesAsync(mortality, validateBalance: true);

                await _unitOfWork.BeginTransaction();

                foreach (var postLine in postLines)
                {
                    await _balanceLedgerManager.ApplyDelta(
                        mortality.ProjectId,
                        postLine.Line.FishBatchId,
                        postLine.Line.ProjectCageId,
                        -postLine.Line.DeadCount,
                        postLine.BiomassDelta,
                        BatchMovementType.Mortality,
                        mortality.MortalityDate,
                        "Mortality",
                        "RII_Mortality",
                        mortality.Id,
                        postLine.Line.ProjectCageId,
                        null,
                        null,
                        null,
                        postLine.AverageGram,
                        postLine.AverageGram,
                        userId);
                }

                var trackedMortality = await _unitOfWork.Mortalities.GetByIdForUpdateAsync(mortality.Id)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.MortalityNotFound"));

                trackedMortality.Status = DocumentStatus.Posted;
                trackedMortality.IsERPIntegrated = false;
                trackedMortality.ERPReferenceNumber = null;
                trackedMortality.ERPIntegrationDate = null;
                trackedMortality.ERPIntegrationStatus = "Pending";
                trackedMortality.ERPErrorMessage = null;
                trackedMortality.UpdatedBy = userId;
                trackedMortality.UpdatedDate = DateTimeProvider.UtcNow;

                await _unitOfWork.SaveChanges();
                await _unitOfWork.Commit();

                return ApiResponse<bool>.SuccessResult(
                    true,
                    _localizationService.GetLocalizedString("MortalityService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityService.BusinessRuleError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<int> ProcessPendingErpIntegrationsAsync(DateTime operationDate, long userId)
        {
            var targetDate = operationDate.Date;
            var mortalityIds = await _unitOfWork.Mortalities
                .Query()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.Status == DocumentStatus.Posted &&
                    !x.IsERPIntegrated &&
                    x.MortalityDate.Date <= targetDate &&
                    (x.ERPIntegrationStatus == null ||
                     x.ERPIntegrationStatus == "Pending" ||
                     x.ERPIntegrationStatus == "Failed"))
                .OrderBy(x => x.Id)
                .Select(x => x.Id)
                .ToListAsync();

            var successCount = 0;
            foreach (var mortalityId in mortalityIds)
            {
                var result = await PostPendingErpAsync(mortalityId, userId);
                if (result.Success)
                {
                    successCount++;
                }
            }

            return successCount;
        }

        private async Task<ApiResponse<bool>> PostPendingErpAsync(long mortalityId, long userId)
        {
            try
            {
                var mortality = await _mortalityRepository.GetForPost(mortalityId)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.MortalityNotFound"));

                if (mortality.IsERPIntegrated)
                {
                    return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("MortalityService.OperationSuccessful"));
                }

                if (mortality.Status != DocumentStatus.Posted)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("General.DocumentMustBeDraftBeforePosting", nameof(Mortality)));
                }

                var postLines = await BuildMortalityPostLinesAsync(mortality, validateBalance: false);
                var itemSlipRequest = BuildMortalityWarehouseIssueRequest(mortality, postLines);
                var itemSlipResponse = await _netsisItemSlipService.CreateWarehouseTransferOutAsync(itemSlipRequest);
                var erpReferenceNumber = ResolveErpReferenceNumber(itemSlipResponse, mortality.MortalityNo ?? $"MORT-{mortality.Id}");

                await _unitOfWork.BeginTransaction();

                var trackedMortality = await _unitOfWork.Mortalities.GetByIdForUpdateAsync(mortality.Id)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.MortalityNotFound"));

                trackedMortality.IsERPIntegrated = true;
                trackedMortality.ERPReferenceNumber = erpReferenceNumber;
                trackedMortality.ERPIntegrationDate = DateTimeProvider.UtcNow;
                trackedMortality.ERPIntegrationStatus = "Success";
                trackedMortality.ERPErrorMessage = null;
                trackedMortality.CountTriedBy = (trackedMortality.CountTriedBy ?? 0) + 1;
                trackedMortality.UpdatedBy = userId;
                trackedMortality.UpdatedDate = DateTimeProvider.UtcNow;

                await _unitOfWork.SaveChanges();
                await _unitOfWork.Commit();

                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("MortalityService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                await MarkErpPostFailedAsync(mortalityId, userId, ex.Message);
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityService.BusinessRuleError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                await MarkErpPostFailedAsync(mortalityId, userId, ex.Message);
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private NetsisItemSlipCreateDto BuildMortalityWarehouseIssueRequest(Mortality mortality, IReadOnlyCollection<MortalityPostLine> postLines)
        {
            var lines = postLines
                .Select(x => BuildMortalityWarehouseIssueLine(mortality, x))
                .GroupBy(x => new { x.StokKodu, x.DepoKodu, x.ProjeKodu })
                .Select(group => new NetsisItemSlipLineDto
                {
                    StokKodu = group.Key.StokKodu,
                    DepoKodu = group.Key.DepoKodu,
                    CikisDepoKodu = group.Key.DepoKodu,
                    ProjeKodu = group.Key.ProjeKodu,
                    Miktar = Math.Round(group.Sum(x => x.Miktar), 3, MidpointRounding.AwayFromZero),
                    NetFiyat = 0,
                    BrutFiyat = 0,
                    Aciklama = $"Aqua fire {mortality.MortalityNo}",
                })
                .ToList();

            if (lines.Count == 0)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.MortalityNotFound"));
            }

            return new NetsisItemSlipCreateDto
            {
                SipDepoKodKullan = 1,
                Seri = ResolveMortalitySeries(),
                FatUst = new NetsisItemSlipHeaderDto
                {
                    Seri = ResolveMortalitySeries(),
                    FatirsNo = ResolveRestDocumentNo(mortality.MortalityNo ?? $"MORT-{mortality.Id}"),
                    CariKod = ResolveMortalityExpenseCode(),
                    Tarih = mortality.MortalityDate.ToString("yyyy-MM-dd"),
                    FiyatTarihi = mortality.MortalityDate.ToString("yyyy-MM-dd"),
                    ProjeKodu = mortality.Project?.ProjectCode,
                    DepoKodu = lines.Select(x => x.CikisDepoKodu ?? x.DepoKodu).FirstOrDefault(x => x.HasValue),
                    Aciklama = $"Aqua fire {mortality.MortalityNo}",
                    EkAciklama1 = mortality.Project?.ProjectCode,
                    EkAciklama2 = mortality.Project?.ProjectName,
                },
                Kalems = lines
            };
        }

        private async Task<List<MortalityPostLine>> BuildMortalityPostLinesAsync(Mortality mortality, bool validateBalance)
        {
            var postLines = new List<MortalityPostLine>();
            foreach (var line in mortality.Lines.Where(x => !x.IsDeleted))
            {
                var balance = await _unitOfWork.Db.BatchCageBalances
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.FishBatchId == line.FishBatchId && x.ProjectCageId == line.ProjectCageId && !x.IsDeleted);

                var averageGram = ResolveAverageGram(balance);
                if (validateBalance && averageGram <= 0)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.AverageGramMissing"));
                }

                if (validateBalance && (balance == null || balance.LiveCount < line.DeadCount))
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.InsufficientBalance"));
                }

                var biomassDelta = validateBalance
                    ? -Math.Round(averageGram * line.DeadCount, 3, MidpointRounding.AwayFromZero)
                    : 0m;

                postLines.Add(new MortalityPostLine(line, averageGram, biomassDelta));
            }

            return postLines;
        }

        private async Task MarkErpPendingAsync(long mortalityId, long userId)
        {
            var mortality = await _unitOfWork.Mortalities.GetByIdForUpdateAsync(mortalityId);
            if (mortality == null || mortality.Status == DocumentStatus.Cancelled)
            {
                return;
            }

            mortality.IsERPIntegrated = false;
            mortality.ERPReferenceNumber = null;
            mortality.ERPIntegrationDate = null;
            mortality.ERPIntegrationStatus = "Pending";
            mortality.ERPErrorMessage = null;
            mortality.UpdatedBy = userId;
            mortality.UpdatedDate = DateTimeProvider.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        private NetsisItemSlipLineDto BuildMortalityWarehouseIssueLine(Mortality mortality, MortalityPostLine postLine)
        {
            var line = postLine.Line;
            var stockCode = line.FishBatch?.FishStock?.ErpStockCode?.Trim();
            if (string.IsNullOrWhiteSpace(stockCode))
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.StockCodeRequired"));
            }

            var warehouse = line.ProjectCage?.Cage?.WarehouseMappings
                .Where(x => x.IsActive && !x.IsDeleted && x.Warehouse != null)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Warehouse)
                .FirstOrDefault();

            if (warehouse == null || warehouse.ErpWarehouseCode <= 0)
            {
                var cageCode = line.ProjectCage?.Cage?.CageCode ?? "-";
                throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.WarehouseMappingRequired", cageCode));
            }

            return new NetsisItemSlipLineDto
            {
                StokKodu = stockCode,
                DepoKodu = warehouse.ErpWarehouseCode,
                CikisDepoKodu = warehouse.ErpWarehouseCode,
                ProjeKodu = mortality.Project?.ProjectCode,
                Miktar = line.DeadCount,
                NetFiyat = 0,
                BrutFiyat = 0,
                Aciklama = $"Aqua fire {mortality.MortalityNo}",
            };
        }

        private async Task MarkErpPostFailedAsync(long mortalityId, long userId, string message)
        {
            try
            {
                var mortality = await _unitOfWork.Mortalities.GetByIdForUpdateAsync(mortalityId);
                if (mortality == null)
                {
                    return;
                }

                mortality.ERPIntegrationStatus = "Failed";
                mortality.ERPErrorMessage = message.Length > 1000 ? message[..1000] : message;
                mortality.CountTriedBy = (mortality.CountTriedBy ?? 0) + 1;
                mortality.UpdatedBy = userId;
                mortality.UpdatedDate = DateTimeProvider.UtcNow;

                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                // Preserve the original posting error for the caller.
            }
        }

        private string? ResolveRestDocumentNo(string fallbackDocumentNo)
            => _netsisOptions.Rest.UseRestGeneratedWarehouseTransferNumbers ? null : fallbackDocumentNo;

        private string? ResolveMortalitySeries()
            => string.IsNullOrWhiteSpace(_netsisOptions.Rest.MortalityWarehouseTransferOutSeries)
                ? "FIR"
                : _netsisOptions.Rest.MortalityWarehouseTransferOutSeries.Trim();

        private string? ResolveMortalityExpenseCode()
            => FirstNonEmpty(
                _netsisOptions.Rest.MortalityWarehouseTransferOutExpenseCode,
                _netsisOptions.Rest.WarehouseTransferOutExpenseCode);

        private static string ResolveErpReferenceNumber(NetsisItemSlipCreateResponseDto response, string fallback)
            => FirstNonEmpty(
                response.Data?.ReferenceNumber,
                response.Data?.FisNo,
                response.Data?.BelgeNo,
                response.Data?.KayitNo,
                fallback) ?? fallback;

        private static string? FirstNonEmpty(params string?[] values)
            => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        private sealed record MortalityPostLine(MortalityLine Line, decimal AverageGram, decimal BiomassDelta);

        private void EnsureDraftStatus(DocumentStatus status, string documentName)
        {
            if (status != DocumentStatus.Draft)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("General.DocumentMustBeDraftBeforePosting", documentName));
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

        private static string BuildDocumentNo(long projectId, DateTime mortalityDate)
            => $"MORT-{projectId}-{mortalityDate:yyyyMMdd}";

    }
}
