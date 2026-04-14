using AutoMapper;
using aqua_api.Shared.Infrastructure.Time;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class StockConvertService : IStockConvertService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockConvertRepository _stockConvertRepository;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public StockConvertService(
            IUnitOfWork unitOfWork,
            IStockConvertRepository stockConvertRepository,
            IBalanceLedgerManager balanceLedgerManager,
            IMapper mapper,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _stockConvertRepository = stockConvertRepository;
            _balanceLedgerManager = balanceLedgerManager;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<StockConvertDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.StockConverts
                    .Query()
                    .Include(x => x.Project)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<StockConvertDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockConvertService.NotFound"),
                        _localizationService.GetLocalizedString("StockConvertService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<StockConvertDto>(entity);
                return ApiResponse<StockConvertDto>.SuccessResult(dto, _localizationService.GetLocalizedString("StockConvertService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockConvertDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<StockConvertDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.StockConverts
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.Project)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(StockConvert.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<StockConvertDto>(x)).ToList();

                var pagedResponse = new PagedResponse<StockConvertDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<StockConvertDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("StockConvertService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<StockConvertDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<StockConvertDto>> CreateAsync(CreateStockConvertDto dto)
        {
            try
            {
                var entity = _mapper.Map<StockConvert>(dto);
                await _unitOfWork.StockConverts.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<StockConvertDto>(entity);
                return ApiResponse<StockConvertDto>.SuccessResult(result, _localizationService.GetLocalizedString("StockConvertService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockConvertDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<StockConvertDto>> UpdateAsync(long id, UpdateStockConvertDto dto)
        {
            try
            {
                var repo = _unitOfWork.StockConverts;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<StockConvertDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockConvertService.NotFound"),
                        _localizationService.GetLocalizedString("StockConvertService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<StockConvertDto>(entity);
                return ApiResponse<StockConvertDto>.SuccessResult(result, _localizationService.GetLocalizedString("StockConvertService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockConvertDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.StockConverts;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("StockConvertService.NotFound"),
                        _localizationService.GetLocalizedString("StockConvertService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("StockConvertService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> Post(long stockConvertId, long userId)
        {
            try
            {
                await _unitOfWork.BeginTransaction();

                var convert = await _stockConvertRepository.GetForPost(stockConvertId)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("StockConvertService.StockConvertNotFound"));

                EnsureDraftStatus(convert.Status, nameof(StockConvert));

                foreach (var line in convert.Lines.Where(x => !x.IsDeleted))
                {
                    var fromBatch = line.FromFishBatch
                        ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("StockConvertService.FromFishBatchNotFound"));

                    var fromStockId = fromBatch.FishStockId;
                    long? toStockId = line.ToFishBatch?.FishStockId;
                    var fromAverageGram = line.AverageGram;
                    if (line.NewAverageGram <= 0)
                    {
                        throw new InvalidOperationException(_localizationService.GetLocalizedString("StockConvertService.GramIncrementMustBeGreaterThanZero"));
                    }
                    // NewAverageGram is treated as increment gram entered by user.
                    var toAverageGram = BatchMath.CalculateIncrementedAverageGram(fromAverageGram, line.NewAverageGram);
                    var fromBiomassGram = line.BiomassGram;
                    var toBiomassGram = BatchMath.CalculateBiomassGram(line.FishCount, toAverageGram);

                    await _balanceLedgerManager.ApplyDelta(
                        convert.ProjectId,
                        line.FromFishBatchId,
                        line.FromProjectCageId,
                        -line.FishCount,
                        -fromBiomassGram,
                        BatchMovementType.StockConvert,
                        convert.ConvertDate,
                        "Stock convert out",
                        "RII_StockConvert",
                        convert.Id,
                        line.FromProjectCageId,
                        line.ToProjectCageId,
                        fromStockId,
                        toStockId,
                        fromAverageGram,
                        toAverageGram,
                        userId);

                    await _balanceLedgerManager.ApplyDelta(
                        convert.ProjectId,
                        line.ToFishBatchId,
                        line.ToProjectCageId,
                        line.FishCount,
                        toBiomassGram,
                        BatchMovementType.StockConvert,
                        convert.ConvertDate,
                        "Stock convert in",
                        "RII_StockConvert",
                        convert.Id,
                        line.FromProjectCageId,
                        line.ToProjectCageId,
                        fromStockId,
                        toStockId,
                        fromAverageGram,
                        toAverageGram,
                        userId);
                }

                convert.Status = DocumentStatus.Posted;
                convert.UpdatedBy = userId;
                convert.UpdatedDate = DateTimeProvider.UtcNow;

                await _unitOfWork.SaveChanges();
                await _unitOfWork.Commit();

                return ApiResponse<bool>.SuccessResult(
                    true,
                    _localizationService.GetLocalizedString("StockConvertService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertService.BusinessRuleError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private void EnsureDraftStatus(DocumentStatus status, string documentName)
        {
            if (status != DocumentStatus.Draft)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("General.DocumentMustBeDraftBeforePosting", documentName));
        }

    }
}
