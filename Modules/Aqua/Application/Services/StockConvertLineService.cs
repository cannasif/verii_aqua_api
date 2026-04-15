using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class StockConvertLineService : IStockConvertLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockConvertService _stockConvertService;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public StockConvertLineService(
            IUnitOfWork unitOfWork,
            IStockConvertService stockConvertService,
            IMapper mapper,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _stockConvertService = stockConvertService;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<StockConvertLineDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.StockConvertLines
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<StockConvertLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockConvertLineService.NotFound"),
                        _localizationService.GetLocalizedString("StockConvertLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<StockConvertLineDto>(entity);
                return ApiResponse<StockConvertLineDto>.SuccessResult(dto, _localizationService.GetLocalizedString("StockConvertLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockConvertLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<StockConvertLineDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.StockConvertLines
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(StockConvertLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<StockConvertLineDto>(x)).ToList();

                var pagedResponse = new PagedResponse<StockConvertLineDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<StockConvertLineDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("StockConvertLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<StockConvertLineDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<StockConvertLineDto>> CreateAsync(CreateStockConvertLineDto dto)
        {
            try
            {
                if (dto.NewAverageGram <= 0)
                {
                    return ApiResponse<StockConvertLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockConvertLineService.BusinessRuleError"),
                        "NewAverageGram must be greater than 0.",
                        StatusCodes.Status400BadRequest);
                }

                var entity = _mapper.Map<StockConvertLine>(dto);
                await _unitOfWork.StockConvertLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var stockConvert = await _unitOfWork.StockConverts
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == entity.StockConvertId && !x.IsDeleted);
                if (stockConvert != null && stockConvert.Status == DocumentStatus.Draft)
                {
                    var userId = entity.CreatedBy ?? stockConvert.CreatedBy ?? 1L;
                    var postResult = await _stockConvertService.Post(stockConvert.Id, userId);
                    if (!postResult.Success)
                    {
                        return ApiResponse<StockConvertLineDto>.ErrorResult(
                            postResult.Message,
                            postResult.ExceptionMessage,
                            postResult.StatusCode);
                    }
                }

                var result = _mapper.Map<StockConvertLineDto>(entity);
                return ApiResponse<StockConvertLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("StockConvertLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockConvertLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<StockConvertLineDto>> CreateWithAutoHeaderAsync(CreateStockConvertLineWithAutoHeaderDto dto)
        {
            try
            {
                if (dto.NewAverageGram <= 0)
                {
                    return ApiResponse<StockConvertLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockConvertLineService.BusinessRuleError"),
                        "NewAverageGram must be greater than 0.",
                        StatusCodes.Status400BadRequest);
                }

                await _unitOfWork.BeginTransactionAsync();

                var stockConvert = await _unitOfWork.StockConverts
                    .Query()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProjectId == dto.ProjectId &&
                        x.Status == DocumentStatus.Draft &&
                        x.ConvertDate.Date == dto.ConvertDate.Date)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (stockConvert == null)
                {
                    var project = await _unitOfWork.Projects
                        .Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == dto.ProjectId && !x.IsDeleted);

                    if (project == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ApiResponse<StockConvertLineDto>.ErrorResult(
                            _localizationService.GetLocalizedString("StockConvertLineService.NotFound"),
                            "Project not found.",
                            StatusCodes.Status404NotFound);
                    }

                    stockConvert = new StockConvert
                    {
                        ProjectId = dto.ProjectId,
                        ConvertDate = dto.ConvertDate,
                        Status = DocumentStatus.Draft,
                        ConvertNo = BuildDocumentNo(project.ProjectCode, project.ProjectName),
                    };

                    await _unitOfWork.StockConverts.AddAsync(stockConvert);
                    await _unitOfWork.SaveChangesAsync();
                }

                var entity = new StockConvertLine
                {
                    StockConvertId = stockConvert.Id,
                    FromFishBatchId = dto.FromFishBatchId,
                    ToFishBatchId = dto.ToFishBatchId,
                    FromProjectCageId = dto.FromProjectCageId,
                    ToProjectCageId = dto.ToProjectCageId,
                    FishCount = dto.FishCount,
                    AverageGram = dto.AverageGram,
                    NewAverageGram = dto.NewAverageGram,
                    BiomassGram = dto.BiomassGram,
                };

                await _unitOfWork.StockConvertLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                if (stockConvert.Status == DocumentStatus.Draft)
                {
                    var userId = entity.CreatedBy ?? stockConvert.CreatedBy ?? 1L;
                    var postResult = await _stockConvertService.Post(stockConvert.Id, userId);
                    if (!postResult.Success)
                    {
                        return ApiResponse<StockConvertLineDto>.ErrorResult(
                            postResult.Message,
                            postResult.ExceptionMessage,
                            postResult.StatusCode);
                    }
                }

                var result = _mapper.Map<StockConvertLineDto>(entity);
                return ApiResponse<StockConvertLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("StockConvertLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<StockConvertLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<StockConvertLineDto>> UpdateAsync(long id, UpdateStockConvertLineDto dto)
        {
            try
            {
                if (dto.NewAverageGram <= 0)
                {
                    return ApiResponse<StockConvertLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockConvertLineService.BusinessRuleError"),
                        "NewAverageGram must be greater than 0.",
                        StatusCodes.Status400BadRequest);
                }

                var repo = _unitOfWork.StockConvertLines;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<StockConvertLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockConvertLineService.NotFound"),
                        _localizationService.GetLocalizedString("StockConvertLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<StockConvertLineDto>(entity);
                return ApiResponse<StockConvertLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("StockConvertLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<StockConvertLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.StockConvertLines;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("StockConvertLineService.NotFound"),
                        _localizationService.GetLocalizedString("StockConvertLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("StockConvertLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("StockConvertLineService.InternalServerError"),
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
