using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class WeighingLineService : IWeighingLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public WeighingLineService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<WeighingLineDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.WeighingLines
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<WeighingLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WeighingLineService.NotFound"),
                        _localizationService.GetLocalizedString("WeighingLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<WeighingLineDto>(entity);
                return ApiResponse<WeighingLineDto>.SuccessResult(dto, _localizationService.GetLocalizedString("WeighingLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WeighingLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<WeighingLineDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.WeighingLines
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(WeighingLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<WeighingLineDto>(x)).ToList();

                var pagedResponse = new PagedResponse<WeighingLineDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<WeighingLineDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("WeighingLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<WeighingLineDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WeighingLineDto>> CreateAsync(CreateWeighingLineDto dto)
        {
            try
            {
                var entity = _mapper.Map<WeighingLine>(dto);
                await _unitOfWork.WeighingLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<WeighingLineDto>(entity);
                return ApiResponse<WeighingLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("WeighingLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WeighingLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WeighingLineDto>> CreateWithAutoHeaderAsync(CreateWeighingLineWithAutoHeaderDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var weighing = await _unitOfWork.Weighings
                    .Query()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProjectId == dto.ProjectId &&
                        x.Status == DocumentStatus.Draft &&
                        x.WeighingDate.Date == dto.WeighingDate.Date)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (weighing == null)
                {
                    var project = await _unitOfWork.Projects
                        .Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == dto.ProjectId && !x.IsDeleted);

                    if (project == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ApiResponse<WeighingLineDto>.ErrorResult(
                            _localizationService.GetLocalizedString("WeighingLineService.NotFound"),
                            "Project not found.",
                            StatusCodes.Status404NotFound);
                    }

                    weighing = new Weighing
                    {
                        ProjectId = dto.ProjectId,
                        WeighingDate = dto.WeighingDate,
                        Status = DocumentStatus.Draft,
                        WeighingNo = BuildDocumentNo(project.ProjectCode, project.ProjectName),
                    };

                    await _unitOfWork.Weighings.AddAsync(weighing);
                    await _unitOfWork.SaveChangesAsync();
                }

                var entity = new WeighingLine
                {
                    WeighingId = weighing.Id,
                    FishBatchId = dto.FishBatchId,
                    ProjectCageId = dto.ProjectCageId,
                    MeasuredCount = dto.MeasuredCount,
                    MeasuredAverageGram = dto.MeasuredAverageGram,
                    MeasuredBiomassGram = dto.MeasuredBiomassGram,
                };

                await _unitOfWork.WeighingLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var result = _mapper.Map<WeighingLineDto>(entity);
                return ApiResponse<WeighingLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("WeighingLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<WeighingLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WeighingLineDto>> UpdateAsync(long id, UpdateWeighingLineDto dto)
        {
            try
            {
                var repo = _unitOfWork.WeighingLines;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<WeighingLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WeighingLineService.NotFound"),
                        _localizationService.GetLocalizedString("WeighingLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<WeighingLineDto>(entity);
                return ApiResponse<WeighingLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("WeighingLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WeighingLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.WeighingLines;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("WeighingLineService.NotFound"),
                        _localizationService.GetLocalizedString("WeighingLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("WeighingLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WeighingLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static string BuildDocumentNo(string? projectCode, string? projectName)
        {
            var baseValue = !string.IsNullOrWhiteSpace(projectCode) ? projectCode : projectName;
            var normalized = string.IsNullOrWhiteSpace(baseValue) ? "DOC" : baseValue.Trim();
            return $"{normalized}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
    }
}
