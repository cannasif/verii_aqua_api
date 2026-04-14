using AutoMapper;
using aqua_api.Shared.Infrastructure.Time;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class MortalityService : IMortalityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMortalityRepository _mortalityRepository;
        private readonly IBalanceLedgerManager _balanceLedgerManager;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public MortalityService(
            IUnitOfWork unitOfWork,
            IMortalityRepository mortalityRepository,
            IBalanceLedgerManager balanceLedgerManager,
            IMapper mapper,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mortalityRepository = mortalityRepository;
            _balanceLedgerManager = balanceLedgerManager;
            _mapper = mapper;
            _localizationService = localizationService;
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
                await _unitOfWork.BeginTransaction();

                var mortality = await _mortalityRepository.GetForPost(mortalityId)
                    ?? throw new InvalidOperationException(_localizationService.GetLocalizedString("MortalityService.MortalityNotFound"));

                EnsureDraftStatus(mortality.Status, nameof(Mortality));

                foreach (var line in mortality.Lines.Where(x => !x.IsDeleted))
                {
                    var balance = await _unitOfWork.Db.BatchCageBalances
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.FishBatchId == line.FishBatchId && x.ProjectCageId == line.ProjectCageId && !x.IsDeleted);

                    var biomassDelta = balance != null
                        ? -Math.Round(balance.AverageGram * line.DeadCount, 3, MidpointRounding.AwayFromZero)
                        : (decimal?)null;

                    await _balanceLedgerManager.ApplyDelta(
                        mortality.ProjectId,
                        line.FishBatchId,
                        line.ProjectCageId,
                        -line.DeadCount,
                        biomassDelta,
                        BatchMovementType.Mortality,
                        mortality.MortalityDate,
                        "Mortality",
                        "RII_Mortality",
                        mortality.Id,
                        line.ProjectCageId,
                        null,
                        null,
                        null,
                        balance?.AverageGram,
                        balance?.AverageGram,
                        userId);
                }

                mortality.Status = DocumentStatus.Posted;
                mortality.UpdatedBy = userId;
                mortality.UpdatedDate = DateTimeProvider.UtcNow;

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

        private void EnsureDraftStatus(DocumentStatus status, string documentName)
        {
            if (status != DocumentStatus.Draft)
                throw new InvalidOperationException(_localizationService.GetLocalizedString("General.DocumentMustBeDraftBeforePosting", documentName));
        }

    }
}
