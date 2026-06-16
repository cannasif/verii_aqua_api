using AutoMapper;
using aqua_api.Modules.Integrations.Infrastructure.Options;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WarehouseEntity = aqua_api.Modules.Warehouse.Domain.Entities.Warehouse;

namespace aqua_api.Modules.Feedings.Application.Services
{
    public class FeedingService : IFeedingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;
        private readonly INetsisItemSlipService _netsisItemSlipService;
        private readonly NetsisOptions _netsisOptions;

        public FeedingService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILocalizationService localizationService,
            INetsisItemSlipService netsisItemSlipService,
            IOptions<NetsisOptions> netsisOptions)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
            _netsisItemSlipService = netsisItemSlipService;
            _netsisOptions = netsisOptions.Value;
        }

        public async Task<ApiResponse<FeedingDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Feedings
                    .Query()
                    .Include(x => x.Project)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<FeedingDto>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<FeedingDto>(entity);
                return ApiResponse<FeedingDto>.SuccessResult(dto, _localizationService.GetLocalizedString("FeedingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedingDto>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<FeedingDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Feedings
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.Project)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(Feeding.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<FeedingDto>(x)).ToList();

                var pagedResponse = new PagedResponse<FeedingDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<FeedingDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("FeedingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<FeedingDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<FeedingDto>> CreateAsync(CreateFeedingDto dto)
        {
            try
            {
                var entity = _mapper.Map<Feeding>(dto);
                await _unitOfWork.Feedings.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<FeedingDto>(entity);
                return ApiResponse<FeedingDto>.SuccessResult(result, _localizationService.GetLocalizedString("FeedingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedingDto>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<FeedingDto>> UpdateAsync(long id, UpdateFeedingDto dto)
        {
            try
            {
                var repo = _unitOfWork.Feedings;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<FeedingDto>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<FeedingDto>(entity);
                return ApiResponse<FeedingDto>.SuccessResult(result, _localizationService.GetLocalizedString("FeedingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedingDto>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.Feedings;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("FeedingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> Post(long feedingId, long userId)
        {
            try
            {
                var feeding = await LoadFeedingForPostingAsync(feedingId)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("FeedingService.FeedingNotFound"));

                if (feeding.Status == DocumentStatus.Cancelled)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("FeedingService.CancelledCannotBePosted"));
                }

                if (feeding.IsERPIntegrated)
                {
                    feeding.Status = DocumentStatus.Posted;
                    await _unitOfWork.SaveChangesAsync();
                    return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("FeedingService.OperationSuccessful"));
                }

                var itemSlipRequest = await BuildFeedingWarehouseIssueRequestAsync(feeding);
                var itemSlipResponse = await _netsisItemSlipService.CreateWarehouseTransferOutAsync(itemSlipRequest);
                var erpReferenceNumber = ResolveErpReferenceNumber(itemSlipResponse, feeding.FeedingNo);

                await _unitOfWork.BeginTransactionAsync();

                var trackedFeeding = await _unitOfWork.Feedings.GetByIdForUpdateAsync(feeding.Id)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("FeedingService.FeedingNotFound"));

                trackedFeeding.Status = DocumentStatus.Posted;
                trackedFeeding.IsERPIntegrated = true;
                trackedFeeding.ERPReferenceNumber = erpReferenceNumber;
                trackedFeeding.ERPIntegrationDate = DateTimeProvider.UtcNow;
                trackedFeeding.ERPIntegrationStatus = "Success";
                trackedFeeding.ERPErrorMessage = null;
                trackedFeeding.CountTriedBy = (trackedFeeding.CountTriedBy ?? 0) + 1;
                trackedFeeding.UpdatedBy = userId;
                trackedFeeding.UpdatedDate = DateTimeProvider.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("FeedingService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                await MarkErpPostFailedAsync(feedingId, userId, ex.Message);
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingService.BusinessRuleError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                await MarkErpPostFailedAsync(feedingId, userId, ex.Message);
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<Feeding?> LoadFeedingForPostingAsync(long feedingId)
        {
            return await _unitOfWork.Feedings
                .Query()
                .Include(x => x.Project)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Stock)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Distributions)
                        .ThenInclude(x => x.ProjectCage)
                            .ThenInclude(x => x!.Cage)
                                .ThenInclude(x => x!.WarehouseMappings)
                                    .ThenInclude(x => x.Warehouse)
                .FirstOrDefaultAsync(x => x.Id == feedingId && !x.IsDeleted);
        }

        private async Task<NetsisItemSlipCreateDto> BuildFeedingWarehouseIssueRequestAsync(Feeding feeding)
        {
            var itemSlipLines = new List<NetsisItemSlipLineDto>();
            foreach (var line in feeding.Lines.Where(x => !x.IsDeleted))
            {
                foreach (var distribution in line.Distributions.Where(distribution => !distribution.IsDeleted))
                {
                    itemSlipLines.Add(await BuildFeedingWarehouseIssueLineAsync(feeding, line, distribution));
                }
            }

            var lines = itemSlipLines
                .GroupBy(x => new { x.StokKodu, x.DepoKodu, x.ProjeKodu })
                .Select(group => new NetsisItemSlipLineDto
                {
                    StokKodu = group.Key.StokKodu,
                    DepoKodu = group.Key.DepoKodu,
                    ProjeKodu = group.Key.ProjeKodu,
                    Miktar = Math.Round(group.Sum(x => x.Miktar), 3, MidpointRounding.AwayFromZero),
                    NetFiyat = 0,
                    BrutFiyat = 0,
                    Aciklama = $"Aqua yemleme {feeding.FeedingNo}",
                })
                .ToList();

            if (lines.Count == 0)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("FeedingService.MustContainDistributedLines"));
            }

            return new NetsisItemSlipCreateDto
            {
                SipDepoKodKullan = 1,
                Seri = ResolveFeedSeries(),
                FatUst = new NetsisItemSlipHeaderDto
                {
                    Seri = ResolveFeedSeries(),
                    FatirsNo = ResolveRestDocumentNo(feeding.FeedingNo),
                    CariKod = ResolveFeedExpenseCode(),
                    Tarih = feeding.FeedingDate.ToString("yyyy-MM-dd"),
                    FiyatTarihi = feeding.FeedingDate.ToString("yyyy-MM-dd"),
                    ProjeKodu = feeding.Project?.ProjectCode,
                    DepoKodu = lines.Select(x => x.CikisDepoKodu ?? x.DepoKodu).FirstOrDefault(x => x.HasValue),
                    Aciklama = $"Aqua yemleme {feeding.FeedingNo}",
                    EkAciklama1 = feeding.Project?.ProjectCode,
                    EkAciklama2 = feeding.Project?.ProjectName,
                },
                Kalems = lines
            };
        }

        private string? ResolveRestDocumentNo(string fallbackDocumentNo)
            => _netsisOptions.Rest.UseRestGeneratedWarehouseTransferNumbers ? null : fallbackDocumentNo;

        private string? ResolveFeedSeries()
            => string.IsNullOrWhiteSpace(_netsisOptions.Rest.FeedWarehouseTransferOutSeries)
                ? "YEM"
                : _netsisOptions.Rest.FeedWarehouseTransferOutSeries.Trim();

        private string? ResolveFeedExpenseCode()
            => FirstNonEmpty(
                _netsisOptions.Rest.FeedWarehouseTransferOutExpenseCode,
                _netsisOptions.Rest.WarehouseTransferOutExpenseCode);

        private async Task<NetsisItemSlipLineDto> BuildFeedingWarehouseIssueLineAsync(Feeding feeding, FeedingLine line, FeedingDistribution distribution)
        {
            var stockCode = line.Stock?.ErpStockCode?.Trim();
            if (string.IsNullOrWhiteSpace(stockCode))
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("FeedingService.StockCodeRequired"));
            }

            if (distribution.FeedGram <= 0)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("FeedingService.InvalidFeedQuantity"));
            }

            var warehouse = await ResolveFeedIssueWarehouseAsync(line.StockId, distribution);

            if (warehouse == null || warehouse.ErpWarehouseCode <= 0)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("FeedingService.FeedWarehouseRequired", stockCode));
            }

            return new NetsisItemSlipLineDto
            {
                StokKodu = stockCode,
                DepoKodu = warehouse.ErpWarehouseCode,
                ProjeKodu = feeding.Project?.ProjectCode,
                Miktar = Math.Round(distribution.FeedGram, 3, MidpointRounding.AwayFromZero),
                NetFiyat = 0,
                BrutFiyat = 0,
                CikisDepoKodu = warehouse.ErpWarehouseCode,
                Aciklama = $"Aqua yemleme {feeding.FeedingNo}",
            };
        }

        private async Task<WarehouseEntity?> ResolveFeedIssueWarehouseAsync(long stockId, FeedingDistribution distribution)
        {
            var receiptWarehouseId = await _unitOfWork.Db.GoodsReceiptLines
                .AsNoTracking()
                .Where(line =>
                    !line.IsDeleted &&
                    line.StockId == stockId &&
                    line.ItemType == GoodsReceiptItemType.Feed &&
                    line.GoodsReceipt != null &&
                    !line.GoodsReceipt.IsDeleted &&
                    line.GoodsReceipt.Status == DocumentStatus.Posted &&
                    line.GoodsReceipt.WarehouseId.HasValue)
                .OrderByDescending(line => line.GoodsReceipt!.ReceiptDate)
                .ThenByDescending(line => line.GoodsReceiptId)
                .Select(line => line.GoodsReceipt!.WarehouseId)
                .FirstOrDefaultAsync();

            if (receiptWarehouseId.HasValue)
            {
                return await _unitOfWork.Db.Warehouses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        !x.IsDeleted &&
                        x.Id == receiptWarehouseId.Value);
            }

            var cageWarehouse = distribution.ProjectCage?.Cage?.WarehouseMappings
                .Where(x => x.IsActive && !x.IsDeleted && x.Warehouse != null)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Warehouse)
                .FirstOrDefault();

            if (cageWarehouse != null)
            {
                return cageWarehouse;
            }

            var defaultWarehouseCode = _netsisOptions.Rest.FeedWarehouseTransferOutWarehouseCode
                ?? _netsisOptions.Rest.DefaultWarehouseCode;
            if (!defaultWarehouseCode.HasValue || defaultWarehouseCode.Value <= 0)
            {
                return null;
            }

            return await _unitOfWork.Db.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.ErpWarehouseCode == defaultWarehouseCode.Value);
        }

        private async Task MarkErpPostFailedAsync(long feedingId, long userId, string message)
        {
            try
            {
                var feeding = await _unitOfWork.Feedings.GetByIdForUpdateAsync(feedingId);
                if (feeding == null)
                {
                    return;
                }

                feeding.ERPIntegrationStatus = "Failed";
                feeding.ERPErrorMessage = message.Length > 1000 ? message[..1000] : message;
                feeding.CountTriedBy = (feeding.CountTriedBy ?? 0) + 1;
                feeding.UpdatedBy = userId;
                feeding.UpdatedDate = DateTimeProvider.UtcNow;

                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                // The original posting error is more useful to the caller than a secondary status update failure.
            }
        }

        private static string ResolveErpReferenceNumber(NetsisItemSlipCreateResponseDto response, string fallback)
        {
            return FirstNonEmpty(
                response.Data?.FisNo,
                response.Data?.BelgeNo,
                response.Data?.KayitNo,
                response.Data?.ReferenceNumber,
                fallback) ?? fallback;
        }

        private static string? FirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }
    }
}
