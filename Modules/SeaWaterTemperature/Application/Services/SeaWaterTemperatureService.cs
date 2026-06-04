using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using SeaWaterTemperatureEntity = aqua_api.Modules.SeaWaterTemperature.Domain.Entities.SeaWaterTemperature;

namespace aqua_api.Modules.SeaWaterTemperature.Application.Services
{
    public class SeaWaterTemperatureService : ISeaWaterTemperatureService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SeaWaterTemperatureService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<SeaWaterTemperatureDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await BaseQuery()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                {
                    return ApiResponse<SeaWaterTemperatureDto>.ErrorResult(
                        "Deniz suyu sıcaklık kaydı bulunamadı.",
                        "Deniz suyu sıcaklık kaydı bulunamadı.",
                        StatusCodes.Status404NotFound);
                }

                return ApiResponse<SeaWaterTemperatureDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<SeaWaterTemperatureDto>.ErrorResult(
                    "Deniz suyu sıcaklık kaydı getirilemedi.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<SeaWaterTemperatureDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = BaseQuery()
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(SeaWaterTemperatureEntity.RecordDate) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var pagedResponse = new PagedResponse<SeaWaterTemperatureDto>
                {
                    Items = entities.Select(Map).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<SeaWaterTemperatureDto>>.SuccessResult(pagedResponse, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<SeaWaterTemperatureDto>>.ErrorResult(
                    "Deniz suyu sıcaklık kayıtları getirilemedi.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<SeaWaterTemperatureDto>> CreateAsync(CreateSeaWaterTemperatureDto dto)
        {
            try
            {
                var validation = await ValidateAsync(dto);
                if (!validation.Success)
                {
                    return validation;
                }

                var entity = new SeaWaterTemperatureEntity
                {
                    ProjectId = dto.ProjectId,
                    ProjectCageId = dto.ProjectCageId,
                    RecordDate = dto.RecordDate.Date,
                    WaterTemperatureCelsius = dto.WaterTemperatureCelsius,
                    WeatherDescription = dto.WeatherDescription.Trim(),
                    Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim()
                };

                await _unitOfWork.Repository<SeaWaterTemperatureEntity>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var saved = await BaseQuery().FirstAsync(x => x.Id == entity.Id);
                return ApiResponse<SeaWaterTemperatureDto>.SuccessResult(Map(saved), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<SeaWaterTemperatureDto>.ErrorResult(
                    "Deniz suyu sıcaklık kaydı oluşturulamadı.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<SeaWaterTemperatureDto>> UpdateAsync(long id, UpdateSeaWaterTemperatureDto dto)
        {
            try
            {
                var entity = await _unitOfWork.Db.SeaWaterTemperatures
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<SeaWaterTemperatureDto>.ErrorResult(
                        "Deniz suyu sıcaklık kaydı bulunamadı.",
                        "Deniz suyu sıcaklık kaydı bulunamadı.",
                        StatusCodes.Status404NotFound);
                }

                var validation = await ValidateAsync(dto, id);
                if (!validation.Success)
                {
                    return validation;
                }

                entity.ProjectId = dto.ProjectId;
                entity.ProjectCageId = dto.ProjectCageId;
                entity.RecordDate = dto.RecordDate.Date;
                entity.WaterTemperatureCelsius = dto.WaterTemperatureCelsius;
                entity.WeatherDescription = dto.WeatherDescription.Trim();
                entity.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();

                await _unitOfWork.Repository<SeaWaterTemperatureEntity>().UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var saved = await BaseQuery().FirstAsync(x => x.Id == entity.Id);
                return ApiResponse<SeaWaterTemperatureDto>.SuccessResult(Map(saved), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<SeaWaterTemperatureDto>.ErrorResult(
                    "Deniz suyu sıcaklık kaydı güncellenemedi.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var deleted = await _unitOfWork.Repository<SeaWaterTemperatureEntity>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        "Deniz suyu sıcaklık kaydı bulunamadı.",
                        "Deniz suyu sıcaklık kaydı bulunamadı.",
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    "Deniz suyu sıcaklık kaydı silinemedi.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private IQueryable<SeaWaterTemperatureEntity> BaseQuery()
        {
            return _unitOfWork.Db.SeaWaterTemperatures
                .AsNoTracking()
                .Include(x => x.Project)
                .Include(x => x.ProjectCage)
                    .ThenInclude(x => x!.Cage)
                .Where(x => !x.IsDeleted);
        }

        private async Task<ApiResponse<SeaWaterTemperatureDto>> ValidateAsync(CreateSeaWaterTemperatureDto dto, long? currentId = null)
        {
            if (dto.ProjectId <= 0)
            {
                return ApiResponse<SeaWaterTemperatureDto>.ErrorResult("Proje seçimi zorunludur.", "Proje seçimi zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (dto.ProjectCageId <= 0)
            {
                return ApiResponse<SeaWaterTemperatureDto>.ErrorResult("Kafes seçimi zorunludur.", "Kafes seçimi zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(dto.WeatherDescription) && dto.WaterTemperatureCelsius == null)
            {
                return ApiResponse<SeaWaterTemperatureDto>.ErrorResult(
                    "Sıcaklık veya hava açıklamasından en az biri girilmelidir.",
                    "Sıcaklık veya hava açıklamasından en az biri girilmelidir.",
                    StatusCodes.Status400BadRequest);
            }

            var projectCageExists = await _unitOfWork.Db.ProjectCages
                .AnyAsync(x => x.Id == dto.ProjectCageId && x.ProjectId == dto.ProjectId && !x.IsDeleted);

            if (!projectCageExists)
            {
                return ApiResponse<SeaWaterTemperatureDto>.ErrorResult(
                    "Seçilen kafes bu projeye bağlı değildir.",
                    "Seçilen kafes bu projeye bağlı değildir.",
                    StatusCodes.Status400BadRequest);
            }

            var recordDate = dto.RecordDate.Date;
            var exists = await _unitOfWork.Db.SeaWaterTemperatures
                .AnyAsync(x =>
                    x.ProjectId == dto.ProjectId &&
                    x.ProjectCageId == dto.ProjectCageId &&
                    x.RecordDate == recordDate &&
                    !x.IsDeleted &&
                    (!currentId.HasValue || x.Id != currentId.Value));

            if (exists)
            {
                return ApiResponse<SeaWaterTemperatureDto>.ErrorResult(
                    "Bu proje, kafes ve tarih için deniz suyu sıcaklık kaydı zaten var.",
                    "Bu proje, kafes ve tarih için deniz suyu sıcaklık kaydı zaten var.",
                    StatusCodes.Status409Conflict);
            }

            return ApiResponse<SeaWaterTemperatureDto>.SuccessResult(new SeaWaterTemperatureDto(), "Valid");
        }

        private static SeaWaterTemperatureDto Map(SeaWaterTemperatureEntity entity)
        {
            return new SeaWaterTemperatureDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                ProjectCode = entity.Project?.ProjectCode,
                ProjectName = entity.Project?.ProjectName,
                ProjectCageId = entity.ProjectCageId,
                CageId = entity.ProjectCage?.CageId,
                CageCode = entity.ProjectCage?.Cage?.CageCode,
                CageName = entity.ProjectCage?.Cage?.CageName,
                RecordDate = entity.RecordDate,
                WaterTemperatureCelsius = entity.WaterTemperatureCelsius,
                WeatherDescription = entity.WeatherDescription,
                Note = entity.Note
            };
        }
    }
}
