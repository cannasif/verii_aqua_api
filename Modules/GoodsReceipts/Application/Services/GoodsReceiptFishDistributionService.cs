using AutoMapper;
using aqua_api.Shared.Common.Helpers;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.GoodsReceipts.Application.Services
{
    public class GoodsReceiptFishDistributionService : IGoodsReceiptFishDistributionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public GoodsReceiptFishDistributionService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<GoodsReceiptFishDistributionDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.GoodsReceiptFishDistributions
                    .Query()
                    .Include(x => x.GoodsReceiptLine)
                        .ThenInclude(x => x!.Stock)
                    .Include(x => x.FishBatch)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<GoodsReceiptFishDistributionDto>.ErrorResult(
                        _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.NotFound"),
                        _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = MapGoodsReceiptFishDistribution(entity);
                return ApiResponse<GoodsReceiptFishDistributionDto>.SuccessResult(dto, _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<GoodsReceiptFishDistributionDto>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<GoodsReceiptFishDistributionDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.GoodsReceiptFishDistributions
                    .Query()
                    .Include(x => x.GoodsReceiptLine)
                        .ThenInclude(x => x!.Stock)
                    .Include(x => x.FishBatch)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(GoodsReceiptFishDistribution.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(MapGoodsReceiptFishDistribution).ToList();

                var pagedResponse = new PagedResponse<GoodsReceiptFishDistributionDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<GoodsReceiptFishDistributionDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<GoodsReceiptFishDistributionDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private GoodsReceiptFishDistributionDto MapGoodsReceiptFishDistribution(GoodsReceiptFishDistribution entity)
        {
            var dto = _mapper.Map<GoodsReceiptFishDistributionDto>(entity);
            dto.StockCode = entity.GoodsReceiptLine?.Stock?.ErpStockCode;
            dto.StockName = entity.GoodsReceiptLine?.Stock?.StockName;
            dto.BatchCode = entity.FishBatch?.BatchCode;
            dto.ProjectCode = entity.ProjectCage?.Project?.ProjectCode;
            dto.ProjectName = entity.ProjectCage?.Project?.ProjectName;
            dto.CageCode = entity.ProjectCage?.Cage?.CageCode;
            dto.CageName = entity.ProjectCage?.Cage?.CageName;
            return dto;
        }

        public async Task<ApiResponse<GoodsReceiptFishDistributionDto>> CreateAsync(CreateGoodsReceiptFishDistributionDto dto)
        {
            try
            {
                var validation = await ValidateAsync(dto);
                if (!validation.Success)
                {
                    return validation;
                }

                var entity = _mapper.Map<GoodsReceiptFishDistribution>(dto);
                await _unitOfWork.GoodsReceiptFishDistributions.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<GoodsReceiptFishDistributionDto>(entity);
                return ApiResponse<GoodsReceiptFishDistributionDto>.SuccessResult(result, _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.OperationSuccessful"));
            }
            catch (DbUpdateException ex) when (DbUpdateExceptionHelper.TryGetUniqueViolation(ex, out _))
            {
                return DistributionAlreadyExists();
            }
            catch (Exception ex)
            {
                return ApiResponse<GoodsReceiptFishDistributionDto>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<GoodsReceiptFishDistributionDto>> UpdateAsync(long id, UpdateGoodsReceiptFishDistributionDto dto)
        {
            try
            {
                var repo = _unitOfWork.GoodsReceiptFishDistributions;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<GoodsReceiptFishDistributionDto>.ErrorResult(
                        _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.NotFound"),
                        _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var validation = await ValidateAsync(dto, id);
                if (!validation.Success)
                {
                    return validation;
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<GoodsReceiptFishDistributionDto>(entity);
                return ApiResponse<GoodsReceiptFishDistributionDto>.SuccessResult(result, _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.OperationSuccessful"));
            }
            catch (DbUpdateException ex) when (DbUpdateExceptionHelper.TryGetUniqueViolation(ex, out _))
            {
                return DistributionAlreadyExists();
            }
            catch (Exception ex)
            {
                return ApiResponse<GoodsReceiptFishDistributionDto>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.GoodsReceiptFishDistributions;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.NotFound"),
                        _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<ApiResponse<GoodsReceiptFishDistributionDto>> ValidateAsync(CreateGoodsReceiptFishDistributionDto dto, long? currentId = null)
        {
            if (dto.GoodsReceiptLineId <= 0)
            {
                return BadRequest("GoodsReceiptFishDistributionService.GoodsReceiptLineRequired");
            }

            if (dto.ProjectCageId <= 0)
            {
                return BadRequest("GoodsReceiptFishDistributionService.ProjectCageRequired");
            }

            if (dto.FishBatchId <= 0)
            {
                return BadRequest("GoodsReceiptFishDistributionService.FishBatchRequired");
            }

            if (dto.FishCount <= 0)
            {
                return BadRequest("GoodsReceiptFishDistributionService.FishCountMustBePositive");
            }

            var line = await _unitOfWork.Db.GoodsReceiptLines
                .Include(x => x.GoodsReceipt)
                .FirstOrDefaultAsync(x => x.Id == dto.GoodsReceiptLineId && !x.IsDeleted);
            if (line == null)
            {
                return BadRequest("GoodsReceiptFishDistributionService.GoodsReceiptLineNotFound");
            }

            if (line.GoodsReceipt?.Status != DocumentStatus.Draft)
            {
                return BadRequest("GoodsReceiptFishDistributionService.GoodsReceiptMustBeDraft");
            }

            var projectCage = await _unitOfWork.Db.ProjectCages
                .FirstOrDefaultAsync(x => x.Id == dto.ProjectCageId && !x.IsDeleted);
            if (projectCage == null)
            {
                return BadRequest("GoodsReceiptFishDistributionService.ProjectCageNotFound");
            }

            var fishBatch = await _unitOfWork.Db.FishBatches
                .FirstOrDefaultAsync(x => x.Id == dto.FishBatchId && !x.IsDeleted);
            if (fishBatch == null)
            {
                return BadRequest("GoodsReceiptFishDistributionService.FishBatchNotFound");
            }

            var receiptProjectId = line.GoodsReceipt?.ProjectId;
            if (receiptProjectId.HasValue && projectCage.ProjectId != receiptProjectId.Value)
            {
                return BadRequest("GoodsReceiptFishDistributionService.ProjectCageMustBelongToReceiptProject");
            }

            if (receiptProjectId.HasValue && fishBatch.ProjectId != receiptProjectId.Value)
            {
                return BadRequest("GoodsReceiptFishDistributionService.FishBatchMustBelongToReceiptProject");
            }

            var duplicateExists = await _unitOfWork.Db.GoodsReceiptFishDistributions.AnyAsync(x =>
                !x.IsDeleted &&
                x.GoodsReceiptLineId == dto.GoodsReceiptLineId &&
                x.ProjectCageId == dto.ProjectCageId &&
                (!currentId.HasValue || x.Id != currentId.Value));
            if (duplicateExists)
            {
                return DistributionAlreadyExists();
            }

            return ApiResponse<GoodsReceiptFishDistributionDto>.SuccessResult(new GoodsReceiptFishDistributionDto(), string.Empty);
        }

        private ApiResponse<GoodsReceiptFishDistributionDto> BadRequest(string key)
        {
            var message = _localizationService.GetLocalizedString(key);
            return ApiResponse<GoodsReceiptFishDistributionDto>.ErrorResult(message, message, StatusCodes.Status400BadRequest);
        }

        private ApiResponse<GoodsReceiptFishDistributionDto> DistributionAlreadyExists()
        {
            var message = _localizationService.GetLocalizedString("GoodsReceiptFishDistributionService.DistributionAlreadyExists");
            return ApiResponse<GoodsReceiptFishDistributionDto>.ErrorResult(message, message, StatusCodes.Status409Conflict);
        }
    }
}
