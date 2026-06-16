using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Mortalities.Application.Services
{
    public class MortalityLineService : IMortalityLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMortalityService _mortalityService;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public MortalityLineService(
            IUnitOfWork unitOfWork,
            IMortalityService mortalityService,
            IMapper mapper,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mortalityService = mortalityService;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<MortalityLineDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.MortalityLines
                    .Query()
                    .Include(x => x.FishBatch)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<MortalityLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("MortalityLineService.NotFound"),
                        _localizationService.GetLocalizedString("MortalityLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = MapMortalityLine(entity);
                return ApiResponse<MortalityLineDto>.SuccessResult(dto, _localizationService.GetLocalizedString("MortalityLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<MortalityLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<MortalityLineDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.MortalityLines
                    .Query()
                    .Include(x => x.FishBatch)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Project)
                    .Include(x => x.ProjectCage)
                        .ThenInclude(x => x!.Cage)
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(MortalityLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(MapMortalityLine).ToList();

                var pagedResponse = new PagedResponse<MortalityLineDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<MortalityLineDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("MortalityLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<MortalityLineDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private MortalityLineDto MapMortalityLine(MortalityLine entity)
        {
            var dto = _mapper.Map<MortalityLineDto>(entity);
            dto.BatchCode = entity.FishBatch?.BatchCode;
            dto.ProjectCode = entity.ProjectCage?.Project?.ProjectCode;
            dto.ProjectName = entity.ProjectCage?.Project?.ProjectName;
            dto.CageCode = entity.ProjectCage?.Cage?.CageCode;
            dto.CageName = entity.ProjectCage?.Cage?.CageName;
            return dto;
        }

        public async Task<ApiResponse<MortalityLineDto>> CreateAsync(CreateMortalityLineDto dto)
        {
            try
            {
                var entity = _mapper.Map<MortalityLine>(dto);
                await _unitOfWork.MortalityLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var mortality = await _unitOfWork.Mortalities
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == entity.MortalityId && !x.IsDeleted);
                if (mortality != null && mortality.Status == DocumentStatus.Draft)
                {
                    var userId = entity.CreatedBy ?? mortality.CreatedBy ?? 1L;
                    var postResult = await _mortalityService.Post(mortality.Id, userId);
                    if (!postResult.Success)
                    {
                        return ApiResponse<MortalityLineDto>.ErrorResult(
                            postResult.Message,
                            postResult.ExceptionMessage,
                            postResult.StatusCode);
                    }
                }

                var result = _mapper.Map<MortalityLineDto>(entity);
                return ApiResponse<MortalityLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("MortalityLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<MortalityLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<MortalityLineDto>> CreateWithAutoHeaderAsync(CreateMortalityLineWithAutoHeaderDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var mortality = await _unitOfWork.Mortalities
                    .Query()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProjectId == dto.ProjectId &&
                        x.Status == DocumentStatus.Draft &&
                        x.MortalityDate.Date == dto.MortalityDate.Date)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (mortality == null)
                {
                    var projectExists = await _unitOfWork.Projects
                        .Query()
                        .AsNoTracking()
                        .AnyAsync(x => x.Id == dto.ProjectId && !x.IsDeleted);

                    if (!projectExists)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ApiResponse<MortalityLineDto>.ErrorResult(
                            _localizationService.GetLocalizedString("MortalityLineService.NotFound"),
                            _localizationService.GetLocalizedString("MortalityLineService.ProjectNotFound"),
                            StatusCodes.Status404NotFound);
                    }

                    mortality = new Mortality
                    {
                        ProjectId = dto.ProjectId,
                        MortalityNo = BuildDocumentNo(dto.ProjectId, dto.MortalityDate),
                        MortalityDate = dto.MortalityDate,
                        Status = DocumentStatus.Draft,
                    };

                    await _unitOfWork.Mortalities.AddAsync(mortality);
                    await _unitOfWork.SaveChangesAsync();
                }

                var entity = await _unitOfWork.MortalityLines
                    .Query()
                    .FirstOrDefaultAsync(x =>
                        !x.IsDeleted &&
                        x.MortalityId == mortality.Id &&
                        x.FishBatchId == dto.FishBatchId &&
                        x.ProjectCageId == dto.ProjectCageId);

                if (entity == null)
                {
                    entity = new MortalityLine
                    {
                        MortalityId = mortality.Id,
                        FishBatchId = dto.FishBatchId,
                        ProjectCageId = dto.ProjectCageId,
                        DeadCount = dto.DeadCount,
                    };

                    await _unitOfWork.MortalityLines.AddAsync(entity);
                }
                else
                {
                    entity.DeadCount = dto.DeadCount;
                    await _unitOfWork.MortalityLines.UpdateAsync(entity);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                if (mortality.Status == DocumentStatus.Draft)
                {
                    var userId = entity.CreatedBy ?? mortality.CreatedBy ?? 1L;
                    var postResult = await _mortalityService.Post(mortality.Id, userId);
                    if (!postResult.Success)
                    {
                        return ApiResponse<MortalityLineDto>.ErrorResult(
                            postResult.Message,
                            postResult.ExceptionMessage,
                            postResult.StatusCode);
                    }
                }

                var result = _mapper.Map<MortalityLineDto>(entity);
                return ApiResponse<MortalityLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("MortalityLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<MortalityLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<MortalityLineDto>> UpdateAsync(long id, UpdateMortalityLineDto dto)
        {
            try
            {
                var repo = _unitOfWork.MortalityLines;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<MortalityLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("MortalityLineService.NotFound"),
                        _localizationService.GetLocalizedString("MortalityLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<MortalityLineDto>(entity);
                return ApiResponse<MortalityLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("MortalityLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<MortalityLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.MortalityLines;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("MortalityLineService.NotFound"),
                        _localizationService.GetLocalizedString("MortalityLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("MortalityLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("MortalityLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static string BuildDocumentNo(long projectId, DateTime mortalityDate)
            => $"MORT-{projectId}-{mortalityDate:yyyyMMdd}";
    }
}
