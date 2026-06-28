using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using aqua_api.Shared.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using CurrentDirectionMatchEntity = aqua_api.Modules.CurrentDirection.Domain.Entities.CurrentDirectionMatch;
using SeaWaterTemperatureEntity = aqua_api.Modules.SeaWaterTemperature.Domain.Entities.SeaWaterTemperature;
using WindDirectionMatchEntity = aqua_api.Modules.WindDirection.Domain.Entities.WindDirectionMatch;

namespace aqua_api.Modules.DailyWeathers.Application.Services
{
    public class DailyEnvironmentalEntryService : IDailyEnvironmentalEntryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizationService;

        public DailyEnvironmentalEntryService(IUnitOfWork unitOfWork, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<DailyEnvironmentalEntryResultDto>> CreateAsync(CreateDailyEnvironmentalEntryRequest request, long userId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var validationError = await ValidateAsync(request);
                if (validationError != null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<DailyEnvironmentalEntryResultDto>.ErrorResult(validationError, validationError, StatusCodes.Status400BadRequest);
                }

                var recordDate = request.Date.Date;
                var now = DateTimeProvider.Now;
                var description = request.Description?.Trim();
                var weatherDescription = BuildWeatherDescription(request.WaterTemperatureCelsius, description);

                var dailyWeather = await UpsertDailyWeatherAsync(request, recordDate, description, now, userId);
                var shouldCreateDailyWeatherMovements = dailyWeather.Id == 0;
                if (shouldCreateDailyWeatherMovements)
                {
                    await _unitOfWork.SaveChangesAsync();
                }

                await UpsertDailyWeatherMovementsAsync(dailyWeather, request, recordDate, userId, shouldCreateDailyWeatherMovements);
                var seaWaterTemperature = await UpsertSeaWaterTemperatureAsync(request, recordDate, weatherDescription, description, now, userId);
                var windDirectionMatch = await UpsertWindDirectionMatchAsync(request, recordDate, description, now, userId);
                var currentDirectionMatch = await UpsertCurrentDirectionMatchAsync(request, recordDate, description, now, userId);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<DailyEnvironmentalEntryResultDto>.SuccessResult(
                    new DailyEnvironmentalEntryResultDto
                    {
                        DailyWeatherId = dailyWeather.Id,
                        SeaWaterTemperatureId = seaWaterTemperature.Id,
                        WindDirectionMatchId = windDirectionMatch.Id,
                        CurrentDirectionMatchId = currentDirectionMatch.Id
                    },
                    _localizationService.GetLocalizedString("DailyWeatherService.OperationSuccessful"));
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                var message = MapDbError(ex);
                return ApiResponse<DailyEnvironmentalEntryResultDto>.ErrorResult(message, message, StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<DailyEnvironmentalEntryResultDto>.ErrorResult(
                    "Günlük çevresel giriş kaydedilemedi.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<string?> ValidateAsync(CreateDailyEnvironmentalEntryRequest request)
        {
            if (request.ProjectId <= 0)
            {
                return "Proje seçimi zorunludur.";
            }

            if (request.ProjectCageId <= 0)
            {
                return "Kafes seçimi zorunludur.";
            }

            if (request.TypeId <= 0)
            {
                return "Hava tipi seçimi zorunludur.";
            }

            if (request.SeverityId <= 0)
            {
                return "Hava şiddeti seçimi zorunludur.";
            }

            if (request.WindDirectionId <= 0)
            {
                return "Rüzgar yönü seçimi zorunludur.";
            }

            if (request.CurrentDirectionId <= 0)
            {
                return "Akıntı yönü seçimi zorunludur.";
            }

            if (request.WaterTemperatureCelsius is < -5 or > 45)
            {
                return "Su sıcaklığı -5 ile 45 °C arasında olmalıdır.";
            }

            if (request.WaterTemperatureCelsius == null && string.IsNullOrWhiteSpace(request.Description))
            {
                return "Su sıcaklığı veya açıklama alanlarından en az biri girilmelidir.";
            }

            var projectCageExists = await _unitOfWork.Db.ProjectCages.AnyAsync(x =>
                x.Id == request.ProjectCageId &&
                x.ProjectId == request.ProjectId &&
                !x.IsDeleted);

            if (!projectCageExists)
            {
                return "Seçilen kafes bu projeye bağlı değildir.";
            }

            var weatherTypeExists = await _unitOfWork.Db.WeatherTypes.AnyAsync(x => x.Id == request.TypeId && !x.IsDeleted);
            if (!weatherTypeExists)
            {
                return _localizationService.GetLocalizedString("DailyWeatherService.InvalidWeatherTypeSelection");
            }

            var weatherSeverityExists = await _unitOfWork.Db.WeatherSeverities.AnyAsync(x => x.Id == request.SeverityId && !x.IsDeleted);
            if (!weatherSeverityExists)
            {
                return _localizationService.GetLocalizedString("DailyWeatherService.InvalidWeatherSeveritySelection");
            }

            var windDirectionExists = await _unitOfWork.Db.WindDirections.AnyAsync(x => x.Id == request.WindDirectionId && !x.IsDeleted);
            if (!windDirectionExists)
            {
                return "Seçilen rüzgar yönü bulunamadı.";
            }

            var currentDirectionExists = await _unitOfWork.Db.CurrentDirections.AnyAsync(x => x.Id == request.CurrentDirectionId && !x.IsDeleted);
            if (!currentDirectionExists)
            {
                return "Seçilen akıntı yönü bulunamadı.";
            }

            return null;
        }

        private async Task<DailyWeather> UpsertDailyWeatherAsync(
            CreateDailyEnvironmentalEntryRequest request,
            DateTime recordDate,
            string? description,
            DateTime now,
            long userId)
        {
            var entity = await _unitOfWork.Db.DailyWeathers
                .FirstOrDefaultAsync(x => x.ProjectId == request.ProjectId && x.WeatherDate == recordDate && !x.IsDeleted);

            if (entity == null)
            {
                entity = new DailyWeather
                {
                    ProjectId = request.ProjectId,
                    WeatherDate = recordDate,
                    WeatherTypeId = request.TypeId,
                    WeatherSeverityId = request.SeverityId,
                    Note = description,
                    CreatedDate = now,
                    CreatedBy = userId,
                    IsDeleted = false
                };

                await _unitOfWork.Db.DailyWeathers.AddAsync(entity);
                return entity;
            }

            entity.WeatherTypeId = request.TypeId;
            entity.WeatherSeverityId = request.SeverityId;
            entity.Note = description;
            entity.UpdatedDate = now;
            entity.UpdatedBy = userId;
            return entity;
        }

        private async Task UpsertDailyWeatherMovementsAsync(
            DailyWeather dailyWeather,
            CreateDailyEnvironmentalEntryRequest request,
            DateTime recordDate,
            long userId,
            bool shouldCreateMovements)
        {
            if (!shouldCreateMovements)
            {
                return;
            }

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
                    MovementDate = recordDate,
                    MovementType = BatchMovementType.Adjustment,
                    SignedCount = 0,
                    SignedBiomassGram = 0,
                    FeedGram = null,
                    ActorUserId = userId,
                    ReferenceTable = "RII_DAILY_WEATHER",
                    ReferenceId = dailyWeather.Id,
                    Note = $"DailyWeather | typeId={request.TypeId} | severityId={request.SeverityId}",
                    CreatedBy = userId,
                    IsDeleted = false
                });
            }
        }

        private async Task<SeaWaterTemperatureEntity> UpsertSeaWaterTemperatureAsync(
            CreateDailyEnvironmentalEntryRequest request,
            DateTime recordDate,
            string weatherDescription,
            string? description,
            DateTime now,
            long userId)
        {
            var entity = await _unitOfWork.Db.SeaWaterTemperatures.FirstOrDefaultAsync(x =>
                x.ProjectId == request.ProjectId &&
                x.ProjectCageId == request.ProjectCageId &&
                x.RecordDate == recordDate &&
                !x.IsDeleted);

            if (entity == null)
            {
                entity = new SeaWaterTemperatureEntity
                {
                    ProjectId = request.ProjectId,
                    ProjectCageId = request.ProjectCageId,
                    RecordDate = recordDate,
                    WaterTemperatureCelsius = request.WaterTemperatureCelsius,
                    WeatherDescription = weatherDescription,
                    Note = description,
                    CreatedDate = now,
                    CreatedBy = userId,
                    IsDeleted = false
                };

                await _unitOfWork.Db.SeaWaterTemperatures.AddAsync(entity);
                return entity;
            }

            entity.WaterTemperatureCelsius = request.WaterTemperatureCelsius;
            entity.WeatherDescription = weatherDescription;
            entity.Note = description;
            entity.UpdatedDate = now;
            entity.UpdatedBy = userId;
            return entity;
        }

        private async Task<WindDirectionMatchEntity> UpsertWindDirectionMatchAsync(
            CreateDailyEnvironmentalEntryRequest request,
            DateTime recordDate,
            string? description,
            DateTime now,
            long userId)
        {
            var entity = await _unitOfWork.Db.WindDirectionMatches.FirstOrDefaultAsync(x =>
                x.ProjectId == request.ProjectId &&
                x.ProjectCageId == request.ProjectCageId &&
                x.RecordDate == recordDate &&
                !x.IsDeleted);

            if (entity == null)
            {
                entity = new WindDirectionMatchEntity
                {
                    ProjectId = request.ProjectId,
                    ProjectCageId = request.ProjectCageId,
                    WindDirectionId = request.WindDirectionId,
                    RecordDate = recordDate,
                    Note = description,
                    CreatedDate = now,
                    CreatedBy = userId,
                    IsDeleted = false
                };

                await _unitOfWork.Db.WindDirectionMatches.AddAsync(entity);
                return entity;
            }

            entity.WindDirectionId = request.WindDirectionId;
            entity.Note = description;
            entity.UpdatedDate = now;
            entity.UpdatedBy = userId;
            return entity;
        }

        private async Task<CurrentDirectionMatchEntity> UpsertCurrentDirectionMatchAsync(
            CreateDailyEnvironmentalEntryRequest request,
            DateTime recordDate,
            string? description,
            DateTime now,
            long userId)
        {
            var entity = await _unitOfWork.Db.CurrentDirectionMatches.FirstOrDefaultAsync(x =>
                x.ProjectId == request.ProjectId &&
                x.ProjectCageId == request.ProjectCageId &&
                x.RecordDate == recordDate &&
                !x.IsDeleted);

            if (entity == null)
            {
                entity = new CurrentDirectionMatchEntity
                {
                    ProjectId = request.ProjectId,
                    ProjectCageId = request.ProjectCageId,
                    CurrentDirectionId = request.CurrentDirectionId,
                    RecordDate = recordDate,
                    Note = description,
                    CreatedDate = now,
                    CreatedBy = userId,
                    IsDeleted = false
                };

                await _unitOfWork.Db.CurrentDirectionMatches.AddAsync(entity);
                return entity;
            }

            entity.CurrentDirectionId = request.CurrentDirectionId;
            entity.Note = description;
            entity.UpdatedDate = now;
            entity.UpdatedBy = userId;
            return entity;
        }

        private static string BuildWeatherDescription(decimal? waterTemperatureCelsius, string? description)
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                return description.Length > 150 ? description[..150] : description;
            }

            return waterTemperatureCelsius.HasValue
                ? $"{waterTemperatureCelsius:0.###} °C"
                : "Günlük çevresel giriş";
        }

        private string MapDbError(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;

            if (message.Contains("UX_RII_DAILY_WEATHER_PROJECT_DATE_ACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                return _localizationService.GetLocalizedString("DailyWeatherService.ProjectDateAlreadyExists");
            }

            if (message.Contains("UX_RII_SEA_WATER_TEMPERATURE_PROJECT_CAGE_DATE_ACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                return "Bu proje, kafes ve tarih için deniz suyu sıcaklık kaydı zaten var.";
            }

            if (message.Contains("UX_RII_WIND_DIRECTION_MATCHES_PROJECT_CAGE_DATE_ACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                return "Bu proje, kafes ve tarih için rüzgar yönü kaydı zaten var.";
            }

            if (message.Contains("UX_RRII_CURRENT_DIRECTION_MATCHES_PROJECT_CAGE_DATE_ACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                return "Bu proje, kafes ve tarih için akıntı yönü kaydı zaten var.";
            }

            return "Günlük çevresel giriş kaydedilemedi.";
        }
    }
}
