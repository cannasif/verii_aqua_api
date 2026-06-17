using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Feedings.Application.Services
{
    public class FeedingDistributionService : IFeedingDistributionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;
        private static readonly IReadOnlyDictionary<string, string> ColumnMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["feedingId"] = "FeedingLine.FeedingId",
            ["batchCode"] = "FishBatch.BatchCode",
            ["projectCode"] = "ProjectCage.Project.ProjectCode",
            ["projectName"] = "ProjectCage.Project.ProjectName",
            ["cageCode"] = "ProjectCage.Cage.CageCode",
            ["cageName"] = "ProjectCage.Cage.CageName",
            ["isERPIntegrated"] = "FeedingLine.Feeding.IsERPIntegrated",
            ["erpReferenceNumber"] = "FeedingLine.Feeding.ERPReferenceNumber",
            ["erpIntegrationDate"] = "FeedingLine.Feeding.ERPIntegrationDate",
            ["erpIntegrationStatus"] = "FeedingLine.Feeding.ERPIntegrationStatus",
            ["erpErrorMessage"] = "FeedingLine.Feeding.ERPErrorMessage"
        };

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
                    .Include(x => x.FeedingLine)
                        .ThenInclude(x => x!.Feeding)
                    .Include(x => x.FishBatch)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<FeedingDistributionDto>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingDistributionService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingDistributionService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = MapFeedingDistribution(entity);
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
                    .ApplyFilters(request.Filters, request.FilterLogic, ColumnMapping);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(FeedingDistribution.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection, ColumnMapping);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .Include(x => x.FeedingLine)
                        .ThenInclude(x => x!.Feeding)
                    .Include(x => x.FishBatch)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .ToListAsync();

                var items = entities.Select(MapFeedingDistribution).ToList();

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
                var feedingLine = await _unitOfWork.FeedingLines
                    .Query(tracking: true)
                    .Include(x => x.Feeding)
                    .FirstOrDefaultAsync(x => x.Id == dto.FeedingLineId && !x.IsDeleted);

                if (feedingLine == null)
                {
                    return ApiResponse<FeedingDistributionDto>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingLineService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                EnsureFeedingCanBeChanged(feedingLine.Feeding);

                var entity = await _unitOfWork.FeedingDistributions
                    .Query(tracking: true)
                    .Include(x => x.FishBatch)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .FirstOrDefaultAsync(x =>
                        !x.IsDeleted &&
                        x.FeedingLineId == dto.FeedingLineId &&
                        x.FishBatchId == dto.FishBatchId &&
                        x.ProjectCageId == dto.ProjectCageId);

                if (entity == null)
                {
                    entity = _mapper.Map<FeedingDistribution>(dto);
                    await _unitOfWork.FeedingDistributions.AddAsync(entity);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    entity.FeedGram = Math.Round(entity.FeedGram + dto.FeedGram, 3, MidpointRounding.AwayFromZero);
                    await _unitOfWork.SaveChangesAsync();
                }

                if (feedingLine?.Feeding != null && feedingLine.Feeding.Status == DocumentStatus.Posted)
                {
                    await AddOrUpdateFeedingMovementAsync(entity, feedingLine);
                }

                var result = MapFeedingDistribution(entity);
                return ApiResponse<FeedingDistributionDto>.SuccessResult(result, _localizationService.GetLocalizedString("FeedingDistributionService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<FeedingDistributionDto>.ErrorResult(
                    ex.Message,
                    ex.Message,
                    StatusCodes.Status400BadRequest);
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
                var entity = await _unitOfWork.FeedingDistributions
                    .Query(tracking: true)
                    .Include(x => x.FeedingLine)
                        .ThenInclude(x => x!.Feeding)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<FeedingDistributionDto>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingDistributionService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingDistributionService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                EnsureFeedingCanBeChanged(entity.FeedingLine?.Feeding);

                _mapper.Map(dto, entity);
                await _unitOfWork.FeedingDistributions.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = MapFeedingDistribution(entity);
                return ApiResponse<FeedingDistributionDto>.SuccessResult(result, _localizationService.GetLocalizedString("FeedingDistributionService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<FeedingDistributionDto>.ErrorResult(
                    ex.Message,
                    ex.Message,
                    StatusCodes.Status400BadRequest);
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
                var entity = await _unitOfWork.FeedingDistributions
                    .Query(tracking: true)
                    .Include(x => x.FeedingLine)
                        .ThenInclude(x => x!.Feeding)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingDistributionService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingDistributionService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                EnsureFeedingCanBeChanged(entity.FeedingLine?.Feeding);

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
            catch (InvalidOperationException ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    ex.Message,
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingDistributionService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private void EnsureFeedingCanBeChanged(Feeding? feeding)
        {
            if (feeding?.IsERPIntegrated == true)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("FeedingDistributionService.ErpIntegratedCannotBeChanged"));
            }
        }

        private FeedingDistributionDto MapFeedingDistribution(FeedingDistribution entity)
        {
            var dto = _mapper.Map<FeedingDistributionDto>(entity);
            dto.BatchCode = entity.FishBatch?.BatchCode;
            dto.ProjectId = entity.ProjectCage?.ProjectId;
            dto.ProjectCode = entity.ProjectCage?.Project?.ProjectCode;
            dto.ProjectName = entity.ProjectCage?.Project?.ProjectName;
            dto.CageCode = entity.ProjectCage?.Cage?.CageCode;
            dto.CageName = entity.ProjectCage?.Cage?.CageName;
            dto.IsERPIntegrated = entity.FeedingLine?.Feeding?.IsERPIntegrated ?? false;
            dto.ERPReferenceNumber = entity.FeedingLine?.Feeding?.ERPReferenceNumber;
            dto.ERPIntegrationDate = entity.FeedingLine?.Feeding?.ERPIntegrationDate;
            dto.ERPIntegrationStatus = entity.FeedingLine?.Feeding?.ERPIntegrationStatus;
            dto.ERPErrorMessage = entity.FeedingLine?.Feeding?.ERPErrorMessage;
            return dto;
        }

        private async Task AddOrUpdateFeedingMovementAsync(FeedingDistribution entity, FeedingLine feedingLine)
        {
            var movement = await _unitOfWork.Db.BatchMovements
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.ReferenceTable == "RII_FeedingDistribution" &&
                    x.ReferenceId == entity.Id &&
                    x.MovementType == BatchMovementType.Feeding);

            if (movement != null)
            {
                movement.FeedGram = entity.FeedGram;
                movement.Note = $"FeedingDistribution | feedGram={entity.FeedGram}";
                await _unitOfWork.SaveChangesAsync();
                return;
            }

            var actorUserId = entity.CreatedBy
                ?? feedingLine.CreatedBy
                ?? feedingLine.Feeding?.CreatedBy
                ?? 1L;

            await _unitOfWork.Db.BatchMovements.AddAsync(new BatchMovement
            {
                FishBatchId = entity.FishBatchId,
                ProjectCageId = entity.ProjectCageId,
                MovementDate = feedingLine.Feeding?.FeedingDate ?? DateTime.UtcNow.Date,
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
}
