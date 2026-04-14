using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class FeedingService : IFeedingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public FeedingService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
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
    }
}
