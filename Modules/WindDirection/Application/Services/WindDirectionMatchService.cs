using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WindDirectionMatchEntity = aqua_api.Modules.WindDirection.Domain.Entities.WindDirectionMatch;

namespace aqua_api.Modules.WindDirection.Application.Services
{
    public class WindDirectionMatchService : IWindDirectionMatchService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WindDirectionMatchService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<WindDirectionMatchDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                {
                    return ApiResponse<WindDirectionMatchDto>.ErrorResult("Rüzgar yönü günlük kaydı bulunamadı.", "Rüzgar yönü günlük kaydı bulunamadı.", StatusCodes.Status404NotFound);
                }

                return ApiResponse<WindDirectionMatchDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<WindDirectionMatchDto>.ErrorResult("Rüzgar yönü günlük kaydı getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<WindDirectionMatchDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = BaseQuery().ApplyFilters(request.Filters, request.FilterLogic);
                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(WindDirectionMatchEntity.RecordDate) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                var response = new PagedResponse<WindDirectionMatchDto>
                {
                    Items = entities.Select(Map).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<WindDirectionMatchDto>>.SuccessResult(response, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<WindDirectionMatchDto>>.ErrorResult("Rüzgar yönü günlük kayıtları getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WindDirectionMatchDto>> CreateAsync(CreateWindDirectionMatchDto dto)
        {
            try
            {
                var validation = await ValidateAsync(dto);
                if (!validation.Success)
                {
                    return validation;
                }

                var entity = new WindDirectionMatchEntity
                {
                    ProjectId = dto.ProjectId,
                    ProjectCageId = dto.ProjectCageId,
                    WindDirectionId = dto.WindDirectionId,
                    RecordDate = dto.RecordDate.Date,
                    Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim()
                };

                await _unitOfWork.Repository<WindDirectionMatchEntity>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var saved = await BaseQuery().FirstAsync(x => x.Id == entity.Id);
                return ApiResponse<WindDirectionMatchDto>.SuccessResult(Map(saved), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<WindDirectionMatchDto>.ErrorResult("Rüzgar yönü günlük kaydı oluşturulamadı.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WindDirectionMatchDto>> UpdateAsync(long id, UpdateWindDirectionMatchDto dto)
        {
            try
            {
                var entity = await _unitOfWork.Db.WindDirectionMatches.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
                if (entity == null)
                {
                    return ApiResponse<WindDirectionMatchDto>.ErrorResult("Rüzgar yönü günlük kaydı bulunamadı.", "Rüzgar yönü günlük kaydı bulunamadı.", StatusCodes.Status404NotFound);
                }

                var validation = await ValidateAsync(dto, id);
                if (!validation.Success)
                {
                    return validation;
                }

                entity.ProjectId = dto.ProjectId;
                entity.ProjectCageId = dto.ProjectCageId;
                entity.WindDirectionId = dto.WindDirectionId;
                entity.RecordDate = dto.RecordDate.Date;
                entity.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();

                await _unitOfWork.Repository<WindDirectionMatchEntity>().UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var saved = await BaseQuery().FirstAsync(x => x.Id == entity.Id);
                return ApiResponse<WindDirectionMatchDto>.SuccessResult(Map(saved), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<WindDirectionMatchDto>.ErrorResult("Rüzgar yönü günlük kaydı güncellenemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var deleted = await _unitOfWork.Repository<WindDirectionMatchEntity>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult("Rüzgar yönü günlük kaydı bulunamadı.", "Rüzgar yönü günlük kaydı bulunamadı.", StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult("Rüzgar yönü günlük kaydı silinemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        private IQueryable<WindDirectionMatchEntity> BaseQuery()
        {
            return _unitOfWork.Db.WindDirectionMatches
                .AsNoTracking()
                .Include(x => x.Project)
                .Include(x => x.ProjectCage)
                    .ThenInclude(x => x!.Cage)
                .Include(x => x.WindDirection)
                .Where(x => !x.IsDeleted);
        }

        private async Task<ApiResponse<WindDirectionMatchDto>> ValidateAsync(CreateWindDirectionMatchDto dto, long? currentId = null)
        {
            if (dto.ProjectId <= 0)
            {
                return ApiResponse<WindDirectionMatchDto>.ErrorResult("Proje seçimi zorunludur.", "Proje seçimi zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (dto.ProjectCageId <= 0)
            {
                return ApiResponse<WindDirectionMatchDto>.ErrorResult("Kafes seçimi zorunludur.", "Kafes seçimi zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (dto.WindDirectionId <= 0)
            {
                return ApiResponse<WindDirectionMatchDto>.ErrorResult("Rüzgar yönü seçimi zorunludur.", "Rüzgar yönü seçimi zorunludur.", StatusCodes.Status400BadRequest);
            }

            var projectCageExists = await _unitOfWork.Db.ProjectCages.AnyAsync(x => x.Id == dto.ProjectCageId && x.ProjectId == dto.ProjectId && !x.IsDeleted);
            if (!projectCageExists)
            {
                return ApiResponse<WindDirectionMatchDto>.ErrorResult("Seçilen kafes bu projeye bağlı değildir.", "Seçilen kafes bu projeye bağlı değildir.", StatusCodes.Status400BadRequest);
            }

            var windDirectionExists = await _unitOfWork.Db.WindDirections.AnyAsync(x => x.Id == dto.WindDirectionId && !x.IsDeleted);
            if (!windDirectionExists)
            {
                return ApiResponse<WindDirectionMatchDto>.ErrorResult("Seçilen rüzgar yönü bulunamadı.", "Seçilen rüzgar yönü bulunamadı.", StatusCodes.Status400BadRequest);
            }

            var recordDate = dto.RecordDate.Date;
            var duplicateExists = await _unitOfWork.Db.WindDirectionMatches.AnyAsync(x =>
                x.ProjectId == dto.ProjectId &&
                x.ProjectCageId == dto.ProjectCageId &&
                x.RecordDate == recordDate &&
                !x.IsDeleted &&
                (!currentId.HasValue || x.Id != currentId.Value));

            if (duplicateExists)
            {
                return ApiResponse<WindDirectionMatchDto>.ErrorResult("Bu proje, kafes ve tarih için rüzgar yönü kaydı zaten var.", "Bu proje, kafes ve tarih için rüzgar yönü kaydı zaten var.", StatusCodes.Status409Conflict);
            }

            return ApiResponse<WindDirectionMatchDto>.SuccessResult(new WindDirectionMatchDto(), "Valid");
        }

        private static WindDirectionMatchDto Map(WindDirectionMatchEntity entity)
        {
            return new WindDirectionMatchDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                ProjectCode = entity.Project?.ProjectCode,
                ProjectName = entity.Project?.ProjectName,
                ProjectCageId = entity.ProjectCageId,
                CageId = entity.ProjectCage?.CageId,
                CageCode = entity.ProjectCage?.Cage?.CageCode,
                CageName = entity.ProjectCage?.Cage?.CageName,
                WindDirectionId = entity.WindDirectionId,
                WindDirectionName = entity.WindDirection?.Name,
                RecordDate = entity.RecordDate,
                Note = entity.Note
            };
        }
    }
}
