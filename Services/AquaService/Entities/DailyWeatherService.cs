using AutoMapper;
using aqua_api.DTOs;
using aqua_api.Interfaces;
using aqua_api.Models;
using aqua_api.UnitOfWork;
using aqua_api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Services
{
    public class DailyWeatherService : IDailyWeatherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDailyWeatherRepository _dailyWeatherRepository;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public DailyWeatherService(IUnitOfWork unitOfWork, IDailyWeatherRepository dailyWeatherRepository, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _dailyWeatherRepository = dailyWeatherRepository;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<DailyWeatherDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.DailyWeathers
                    .Query()
                    .Include(x => x.Project)
                    .Include(x => x.WeatherType)
                    .Include(x => x.WeatherSeverity)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<DailyWeatherDto>.ErrorResult(
                        _localizationService.GetLocalizedString("DailyWeatherService.NotFound"),
                        _localizationService.GetLocalizedString("DailyWeatherService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<DailyWeatherDto>(entity);
                return ApiResponse<DailyWeatherDto>.SuccessResult(dto, _localizationService.GetLocalizedString("DailyWeatherService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<DailyWeatherDto>.ErrorResult(
                    _localizationService.GetLocalizedString("DailyWeatherService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<DailyWeatherDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.DailyWeathers
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.Project)
                    .Include(x => x.WeatherType)
                    .Include(x => x.WeatherSeverity)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(DailyWeather.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<DailyWeatherDto>(x)).ToList();

                var pagedResponse = new PagedResponse<DailyWeatherDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<DailyWeatherDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("DailyWeatherService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<DailyWeatherDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("DailyWeatherService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<DailyWeatherDto>> CreateAsync(CreateDailyWeatherDto dto)
        {
            try
            {
                var weatherDate = dto.WeatherDate.Date;
                var alreadyExists = await _unitOfWork.DailyWeathers
                    .Query()
                    .AnyAsync(x => !x.IsDeleted
                        && x.ProjectId == dto.ProjectId
                        && x.WeatherDate.Date == weatherDate);

                if (alreadyExists)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("DailyWeatherService.ProjectDateAlreadyExists"));
                }

                var entity = _mapper.Map<DailyWeather>(dto);
                await _unitOfWork.DailyWeathers.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<DailyWeatherDto>(entity);
                return ApiResponse<DailyWeatherDto>.SuccessResult(result, _localizationService.GetLocalizedString("DailyWeatherService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<DailyWeatherDto>.ErrorResult(
                    ex.Message,
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (DbUpdateException ex)
            {
                var businessMessage = MapDbError(ex);
                return ApiResponse<DailyWeatherDto>.ErrorResult(
                    businessMessage,
                    businessMessage,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return ApiResponse<DailyWeatherDto>.ErrorResult(
                    _localizationService.GetLocalizedString("DailyWeatherService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<DailyWeatherDto>> UpdateAsync(long id, UpdateDailyWeatherDto dto)
        {
            try
            {
                var repo = _unitOfWork.DailyWeathers;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<DailyWeatherDto>.ErrorResult(
                        _localizationService.GetLocalizedString("DailyWeatherService.NotFound"),
                        _localizationService.GetLocalizedString("DailyWeatherService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<DailyWeatherDto>(entity);
                return ApiResponse<DailyWeatherDto>.SuccessResult(result, _localizationService.GetLocalizedString("DailyWeatherService.OperationSuccessful"));
            }
            catch (DbUpdateException ex)
            {
                var businessMessage = MapDbError(ex);
                return ApiResponse<DailyWeatherDto>.ErrorResult(
                    businessMessage,
                    businessMessage,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return ApiResponse<DailyWeatherDto>.ErrorResult(
                    _localizationService.GetLocalizedString("DailyWeatherService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.DailyWeathers;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("DailyWeatherService.NotFound"),
                        _localizationService.GetLocalizedString("DailyWeatherService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("DailyWeatherService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("DailyWeatherService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<long>> CreateDaily(CreateDailyWeatherRequest request, long userId)
        {
            try
            {
                var exists = await _dailyWeatherRepository.ExistsByProjectAndDate(request.ProjectId, request.Date);

                if (exists)
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("DailyWeatherService.ProjectDateAlreadyExists"));

                var entity = new DailyWeather
                {
                    ProjectId = request.ProjectId,
                    WeatherDate = request.Date.Date,
                    WeatherTypeId = request.TypeId,
                    WeatherSeverityId = request.SeverityId,
                    Note = request.Description,
                    IsDeleted = false
                };

                await _dailyWeatherRepository.Add(entity);
                await _unitOfWork.SaveChanges();

                // Daily weather is not a fish delta operation; write zero-delta movement rows
                // for active batch-cage balances so timeline queries can include weather days.
                var activeBalances = await _unitOfWork.Db.BatchCageBalances
                    .Where(x => !x.IsDeleted
                        && x.LiveCount > 0
                        && x.ProjectCage != null
                        && !x.ProjectCage.IsDeleted
                        && x.ProjectCage.ProjectId == request.ProjectId)
                    .ToListAsync();

                foreach (var balance in activeBalances)
                {
                    await _unitOfWork.Db.BatchMovements.AddAsync(new BatchMovement
                    {
                        FishBatchId = balance.FishBatchId,
                        ProjectCageId = balance.ProjectCageId,
                        MovementDate = request.Date.Date,
                        MovementType = BatchMovementType.Adjustment,
                        SignedCount = 0,
                        SignedBiomassGram = 0,
                        FeedGram = null,
                        ActorUserId = userId,
                        ReferenceTable = "RII_DailyWeather",
                        ReferenceId = entity.Id,
                        Note = $"DailyWeather | typeId={request.TypeId} | severityId={request.SeverityId}",
                        CreatedBy = userId,
                        IsDeleted = false
                    });
                }

                if (activeBalances.Count > 0)
                {
                    await _unitOfWork.SaveChanges();
                }

                return ApiResponse<long>.SuccessResult(
                    entity.Id,
                    _localizationService.GetLocalizedString("DailyWeatherService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<long>.ErrorResult(
                    _localizationService.GetLocalizedString("DailyWeatherService.BusinessRuleError"),
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return ApiResponse<long>.ErrorResult(
                    _localizationService.GetLocalizedString("DailyWeatherService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private string MapDbError(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            if (message.Contains("UX_RII_DailyWeather_ProjectDate_Active", StringComparison.OrdinalIgnoreCase))
            {
                return _localizationService.GetLocalizedString("DailyWeatherService.ProjectDateAlreadyExists");
            }

            if (message.Contains("FK_RII_DailyWeather_Project", StringComparison.OrdinalIgnoreCase))
            {
                return _localizationService.GetLocalizedString("DailyWeatherService.InvalidProjectSelection");
            }

            if (message.Contains("FK_RII_DailyWeather_WeatherType", StringComparison.OrdinalIgnoreCase))
            {
                return _localizationService.GetLocalizedString("DailyWeatherService.InvalidWeatherTypeSelection");
            }

            if (message.Contains("FK_RII_DailyWeather_WeatherSeverity", StringComparison.OrdinalIgnoreCase))
            {
                return _localizationService.GetLocalizedString("DailyWeatherService.InvalidWeatherSeveritySelection");
            }

            return _localizationService.GetLocalizedString("DailyWeatherService.SaveFailed");
        }
    }
}
