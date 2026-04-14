using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class WeatherSeverityService : IWeatherSeverityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public WeatherSeverityService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<WeatherSeverityDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.WeatherSeverities
                    .Query()
                    .Include(x => x.WeatherType)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<WeatherSeverityDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WeatherSeverityService.NotFound"),
                        _localizationService.GetLocalizedString("WeatherSeverityService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<WeatherSeverityDto>(entity);
                return ApiResponse<WeatherSeverityDto>.SuccessResult(dto, _localizationService.GetLocalizedString("WeatherSeverityService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WeatherSeverityDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WeatherSeverityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<WeatherSeverityDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.WeatherSeverities
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.WeatherType)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(WeatherSeverity.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<WeatherSeverityDto>(x)).ToList();

                var pagedResponse = new PagedResponse<WeatherSeverityDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<WeatherSeverityDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("WeatherSeverityService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<WeatherSeverityDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("WeatherSeverityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WeatherSeverityDto>> CreateAsync(CreateWeatherSeverityDto dto)
        {
            try
            {
                var weatherTypeExists = await _unitOfWork.WeatherTypes
                    .Query()
                    .AnyAsync(x => x.Id == dto.WeatherTypeId && !x.IsDeleted);

                if (!weatherTypeExists)
                {
                    return ApiResponse<WeatherSeverityDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WeatherTypeService.NotFound"),
                        _localizationService.GetLocalizedString("WeatherTypeService.NotFound"),
                        StatusCodes.Status400BadRequest);
                }

                var entity = _mapper.Map<WeatherSeverity>(dto);
                await _unitOfWork.WeatherSeverities.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<WeatherSeverityDto>(entity);
                return ApiResponse<WeatherSeverityDto>.SuccessResult(result, _localizationService.GetLocalizedString("WeatherSeverityService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WeatherSeverityDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WeatherSeverityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WeatherSeverityDto>> UpdateAsync(long id, UpdateWeatherSeverityDto dto)
        {
            try
            {
                var repo = _unitOfWork.WeatherSeverities;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<WeatherSeverityDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WeatherSeverityService.NotFound"),
                        _localizationService.GetLocalizedString("WeatherSeverityService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var weatherTypeExists = await _unitOfWork.WeatherTypes
                    .Query()
                    .AnyAsync(x => x.Id == dto.WeatherTypeId && !x.IsDeleted);

                if (!weatherTypeExists)
                {
                    return ApiResponse<WeatherSeverityDto>.ErrorResult(
                        _localizationService.GetLocalizedString("WeatherTypeService.NotFound"),
                        _localizationService.GetLocalizedString("WeatherTypeService.NotFound"),
                        StatusCodes.Status400BadRequest);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<WeatherSeverityDto>(entity);
                return ApiResponse<WeatherSeverityDto>.SuccessResult(result, _localizationService.GetLocalizedString("WeatherSeverityService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<WeatherSeverityDto>.ErrorResult(
                    _localizationService.GetLocalizedString("WeatherSeverityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.WeatherSeverities;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("WeatherSeverityService.NotFound"),
                        _localizationService.GetLocalizedString("WeatherSeverityService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("WeatherSeverityService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("WeatherSeverityService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
