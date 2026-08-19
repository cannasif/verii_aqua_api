using AutoMapper;
using aqua_api.Shared.Infrastructure.Time;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Weighings.Application.Services
{
    public class WeighingService : IWeighingService
    {
        private static readonly IReadOnlyDictionary<string, string> PagedColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["WeighingNo"] = "WeighingNo",
            ["WeighingDate"] = "WeighingDate",
            ["ProjectCode"] = "Project.ProjectCode",
            ["ProjectName"] = "Project.ProjectName"
        };
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWeighingRepository _weighingRepository;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public WeighingService(
            IUnitOfWork unitOfWork,
            IWeighingRepository weighingRepository,
            IBalanceLedgerManager balanceLedgerManager,
            IMapper mapper,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _weighingRepository = weighingRepository;
            _balanceLedgerManager = balanceLedgerManager;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<WeighingDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Weighings
                    .Query()
                    .Include(x => x.Project)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<WeighingDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WeighingService.NotFound"),
                        _localizationService.GetLocalizedString("WeighingService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<WeighingDto>(entity);
                return ApiResponse<WeighingDto>.SuccessResult(dto, _localizationService.GetLocalizedString("WeighingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WeighingDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<WeighingDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Weighings
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.Project)
                    .ApplySearch(request, PagedColumns)
                    .ApplyFilters(request.Filters, request.FilterLogic, PagedColumns);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(Weighing.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection, PagedColumns);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<WeighingDto>(x)).ToList();

                var pagedResponse = new PagedResponse<WeighingDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<WeighingDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("WeighingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<WeighingDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WeighingDto>> CreateAsync(CreateWeighingDto dto)
        {
            try
            {
                var entity = _mapper.Map<Weighing>(dto);
                await _unitOfWork.Weighings.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<WeighingDto>(entity);
                return ApiResponse<WeighingDto>.SuccessResult(result, _localizationService.GetLocalizedString("WeighingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WeighingDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WeighingDto>> UpdateAsync(long id, UpdateWeighingDto dto)
        {
            try
            {
                var repo = _unitOfWork.Weighings;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<WeighingDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WeighingService.NotFound"),
                        _localizationService.GetLocalizedString("WeighingService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<WeighingDto>(entity);
                return ApiResponse<WeighingDto>.SuccessResult(result, _localizationService.GetLocalizedString("WeighingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WeighingDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.Weighings;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("WeighingService.NotFound"),
                        _localizationService.GetLocalizedString("WeighingService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("WeighingService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> Post(long weighingId, long userId)
        {
            try
            {
                await _unitOfWork.BeginTransaction();

                var weighing = await _weighingRepository.GetForPost(weighingId)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("WeighingService.WeighingNotFound"));

                EnsureDraftStatus(weighing.Status, nameof(Weighing));

                var touchedFishBatchIds = new HashSet<long>();
                foreach (var line in weighing.Lines.Where(x => !x.IsDeleted))
                {
                    if (line.FishBatch == null)
                    {
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("WeighingService.FishBatchNotFoundForWeighingLine"));
                    }
                    if (line.MeasuredAverageGram <= 0m || line.MeasuredCount <= 0)
                    {
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("WeighingService.BusinessRuleError"));
                    }

                    var sourceMass = await _balanceLedgerManager.GetCageMassSnapshotAsync(
                        line.FishBatchId,
                        line.ProjectCageId,
                        weighing.WeighingDate,
                        BatchMovementType.Weighing);
                    if (sourceMass.LiveCount <= 0 || sourceMass.AverageGram <= 0m)
                    {
                        throw new InvalidOperationException(
                            _localizationService.GetLocalizedString("FishGrowthService.ActiveBalanceNotFound"));
                    }

                    var targetBiomassGram = BatchMath.CalculateBiomassGram(
                        sourceMass.LiveCount,
                        line.MeasuredAverageGram);
                    var biomassDelta = Math.Round(
                        targetBiomassGram - sourceMass.BiomassGram,
                        3,
                        MidpointRounding.AwayFromZero);
                    line.MeasuredBiomassGram = BatchMath.CalculateBiomassGram(
                        line.MeasuredCount,
                        line.MeasuredAverageGram);
                    touchedFishBatchIds.Add(line.FishBatchId);

                    await _balanceLedgerManager.ApplyDelta(
                        weighing.ProjectId,
                        line.FishBatchId,
                        line.ProjectCageId,
                        0,
                        biomassDelta,
                        BatchMovementType.Weighing,
                        weighing.WeighingDate,
                        "Weighing average update",
                        "RII_WEIGHING",
                        weighing.Id,
                        line.ProjectCageId,
                        line.ProjectCageId,
                        null,
                        null,
                        sourceMass.AverageGram,
                        line.MeasuredAverageGram,
                        userId);
                }

                await _unitOfWork.SaveChanges();
                await UpdateFishBatchAveragesAsync(touchedFishBatchIds, userId);

                weighing.Status = DocumentStatus.Posted;
                weighing.UpdatedBy = userId;
                weighing.UpdatedDate = DateTimeProvider.UtcNow;

                await _unitOfWork.SaveChanges();
                await _unitOfWork.Commit();

                return ApiResponse<bool>.SuccessResult(
                    true,
                    _localizationService.GetLocalizedString("WeighingService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingService.BusinessRuleError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private void EnsureDraftStatus(DocumentStatus status, string documentName)
        {
            if (status != DocumentStatus.Draft)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("General.DocumentMustBeDraftBeforePosting", documentName));
        }

        private async Task UpdateFishBatchAveragesAsync(IEnumerable<long> fishBatchIds, long userId)
        {
            foreach (var fishBatchId in fishBatchIds.Distinct())
            {
                var cageBalances = await _unitOfWork.Db.BatchCageBalances
                    .Where(x => !x.IsDeleted && x.FishBatchId == fishBatchId)
                    .Select(x => new { x.LiveCount, x.BiomassGram })
                    .ToListAsync();
                var warehouseBalances = await _unitOfWork.Db.BatchWarehouseBalances
                    .Where(x => !x.IsDeleted && x.FishBatchId == fishBatchId)
                    .Select(x => new { x.LiveCount, x.BiomassGram })
                    .ToListAsync();
                var totalCount = cageBalances.Sum(x => (long)x.LiveCount)
                    + warehouseBalances.Sum(x => (long)x.LiveCount);
                var totalBiomassGram = cageBalances.Sum(x => x.BiomassGram)
                    + warehouseBalances.Sum(x => x.BiomassGram);
                var fishBatch = await _unitOfWork.Db.FishBatches
                    .FirstOrDefaultAsync(x => x.Id == fishBatchId && !x.IsDeleted)
                    ?? throw new InvalidOperationException(
                        _localizationService.GetLocalizedString("WeighingService.FishBatchNotFoundForWeighingLine"));

                fishBatch.CurrentAverageGram = totalCount > 0
                    ? Math.Round(totalBiomassGram / totalCount, 3, MidpointRounding.AwayFromZero)
                    : 0m;
                fishBatch.UpdatedBy = userId;
                fishBatch.UpdatedDate = DateTimeProvider.UtcNow;
            }
        }

    }
}
