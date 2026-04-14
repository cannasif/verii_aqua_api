using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class FeedingDistributionService : IFeedingDistributionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public FeedingDistributionService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<FeedingDistributionDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.FeedingDistributions
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<FeedingDistributionDto>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingDistributionService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingDistributionService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<FeedingDistributionDto>(entity);
                return ApiResponse<FeedingDistributionDto>.SuccessResult(dto, _localizationService.GetLocalizedString("FeedingDistributionService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedingDistributionDto>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingDistributionService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<FeedingDistributionDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.FeedingDistributions
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(FeedingDistribution.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<FeedingDistributionDto>(x)).ToList();

                var pagedResponse = new PagedResponse<FeedingDistributionDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<FeedingDistributionDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("FeedingDistributionService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<FeedingDistributionDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingDistributionService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<FeedingDistributionDto>> CreateAsync(CreateFeedingDistributionDto dto)
        {
            try
            {
                var entity = _mapper.Map<FeedingDistribution>(dto);
                await _unitOfWork.FeedingDistributions.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var feedingLine = await _unitOfWork.FeedingLines
                    .Query()
                    .Include(x => x.Feeding)
                    .FirstOrDefaultAsync(x => x.Id == entity.FeedingLineId && !x.IsDeleted);

                if (feedingLine?.Feeding != null && feedingLine.Feeding.Status == DocumentStatus.Posted)
                {
                    var activeBalance = await _unitOfWork.Db.BatchCageBalances
                        .Where(x => !x.IsDeleted
                            && x.ProjectCageId == entity.ProjectCageId
                            && x.LiveCount > 0)
                        .OrderByDescending(x => x.LiveCount)
                        .ThenByDescending(x => x.Id)
                        .FirstOrDefaultAsync();

                    if (activeBalance != null)
                    {
                        var actorUserId = entity.CreatedBy
                            ?? feedingLine.CreatedBy
                            ?? feedingLine.Feeding.CreatedBy
                            ?? 1L;
                        await _unitOfWork.Db.BatchMovements.AddAsync(new BatchMovement
                        {
                            FishBatchId = activeBalance.FishBatchId,
                            ProjectCageId = entity.ProjectCageId,
                            MovementDate = feedingLine.Feeding.FeedingDate,
                            MovementType = BatchMovementType.Feeding,
                            SignedCount = 0,
                            SignedBiomassGram = 0,
                            FeedGram = entity.FeedGram,
                            ActorUserId = actorUserId,
                            ReferenceTable = "RII_FeedingDistribution",
                            ReferenceId = entity.Id,
                            Note = $"FeedingDistribution | feedGram={entity.FeedGram}",
                            CreatedBy = actorUserId,
                            IsDeleted = false
                        });

                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                var result = _mapper.Map<FeedingDistributionDto>(entity);
                return ApiResponse<FeedingDistributionDto>.SuccessResult(result, _localizationService.GetLocalizedString("FeedingDistributionService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedingDistributionDto>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingDistributionService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<FeedingDistributionDto>> UpdateAsync(long id, UpdateFeedingDistributionDto dto)
        {
            try
            {
                var repo = _unitOfWork.FeedingDistributions;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<FeedingDistributionDto>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingDistributionService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingDistributionService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<FeedingDistributionDto>(entity);
                return ApiResponse<FeedingDistributionDto>.SuccessResult(result, _localizationService.GetLocalizedString("FeedingDistributionService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedingDistributionDto>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingDistributionService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.FeedingDistributions;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingDistributionService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingDistributionService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("FeedingDistributionService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingDistributionService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
