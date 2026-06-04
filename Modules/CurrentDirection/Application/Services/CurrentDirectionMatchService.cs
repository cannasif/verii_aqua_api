using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using CurrentDirectionMatchEntity = aqua_api.Modules.CurrentDirection.Domain.Entities.CurrentDirectionMatch;

namespace aqua_api.Modules.CurrentDirection.Application.Services
{
    public class CurrentDirectionMatchService : ICurrentDirectionMatchService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CurrentDirectionMatchService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<CurrentDirectionMatchDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                {
                    return ApiResponse<CurrentDirectionMatchDto>.ErrorResult("Akıntı yönü günlük kaydı bulunamadı.", "Akıntı yönü günlük kaydı bulunamadı.", StatusCodes.Status404NotFound);
                }

                return ApiResponse<CurrentDirectionMatchDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<CurrentDirectionMatchDto>.ErrorResult("Akıntı yönü günlük kaydı getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<CurrentDirectionMatchDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = BaseQuery().ApplyFilters(request.Filters, request.FilterLogic);
                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(CurrentDirectionMatchEntity.RecordDate) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                var response = new PagedResponse<CurrentDirectionMatchDto>
                {
                    Items = entities.Select(Map).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<CurrentDirectionMatchDto>>.SuccessResult(response, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<CurrentDirectionMatchDto>>.ErrorResult("Akıntı yönü günlük kayıtları getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<CurrentDirectionMatchDto>> CreateAsync(CreateCurrentDirectionMatchDto dto)
        {
            try
            {
                var validation = await ValidateAsync(dto);
                if (!validation.Success)
                {
                    return validation;
                }

                var entity = new CurrentDirectionMatchEntity
                {
                    ProjectId = dto.ProjectId,
                    ProjectCageId = dto.ProjectCageId,
                    CurrentDirectionId = dto.CurrentDirectionId,
                    RecordDate = dto.RecordDate.Date,
                    Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim()
                };

                await _unitOfWork.Repository<CurrentDirectionMatchEntity>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var saved = await BaseQuery().FirstAsync(x => x.Id == entity.Id);
                return ApiResponse<CurrentDirectionMatchDto>.SuccessResult(Map(saved), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<CurrentDirectionMatchDto>.ErrorResult("Akıntı yönü günlük kaydı oluşturulamadı.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<CurrentDirectionMatchDto>> UpdateAsync(long id, UpdateCurrentDirectionMatchDto dto)
        {
            try
            {
                var entity = await _unitOfWork.Db.CurrentDirectionMatches.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
                if (entity == null)
                {
                    return ApiResponse<CurrentDirectionMatchDto>.ErrorResult("Akıntı yönü günlük kaydı bulunamadı.", "Akıntı yönü günlük kaydı bulunamadı.", StatusCodes.Status404NotFound);
                }

                var validation = await ValidateAsync(dto, id);
                if (!validation.Success)
                {
                    return validation;
                }

                entity.ProjectId = dto.ProjectId;
                entity.ProjectCageId = dto.ProjectCageId;
                entity.CurrentDirectionId = dto.CurrentDirectionId;
                entity.RecordDate = dto.RecordDate.Date;
                entity.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();

                await _unitOfWork.Repository<CurrentDirectionMatchEntity>().UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var saved = await BaseQuery().FirstAsync(x => x.Id == entity.Id);
                return ApiResponse<CurrentDirectionMatchDto>.SuccessResult(Map(saved), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<CurrentDirectionMatchDto>.ErrorResult("Akıntı yönü günlük kaydı güncellenemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var deleted = await _unitOfWork.Repository<CurrentDirectionMatchEntity>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult("Akıntı yönü günlük kaydı bulunamadı.", "Akıntı yönü günlük kaydı bulunamadı.", StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult("Akıntı yönü günlük kaydı silinemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        private IQueryable<CurrentDirectionMatchEntity> BaseQuery()
        {
            return _unitOfWork.Db.CurrentDirectionMatches
                .AsNoTracking()
                .Include(x => x.Project)
                .Include(x => x.ProjectCage)
                    .ThenInclude(x => x!.Cage)
                .Include(x => x.CurrentDirection)
                .Where(x => !x.IsDeleted);
        }

        private async Task<ApiResponse<CurrentDirectionMatchDto>> ValidateAsync(CreateCurrentDirectionMatchDto dto, long? currentId = null)
        {
            if (dto.ProjectId <= 0)
            {
                return ApiResponse<CurrentDirectionMatchDto>.ErrorResult("Proje seçimi zorunludur.", "Proje seçimi zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (dto.ProjectCageId <= 0)
            {
                return ApiResponse<CurrentDirectionMatchDto>.ErrorResult("Kafes seçimi zorunludur.", "Kafes seçimi zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (dto.CurrentDirectionId <= 0)
            {
                return ApiResponse<CurrentDirectionMatchDto>.ErrorResult("Akıntı yönü seçimi zorunludur.", "Akıntı yönü seçimi zorunludur.", StatusCodes.Status400BadRequest);
            }

            var projectCageExists = await _unitOfWork.Db.ProjectCages.AnyAsync(x => x.Id == dto.ProjectCageId && x.ProjectId == dto.ProjectId && !x.IsDeleted);
            if (!projectCageExists)
            {
                return ApiResponse<CurrentDirectionMatchDto>.ErrorResult("Seçilen kafes bu projeye bağlı değildir.", "Seçilen kafes bu projeye bağlı değildir.", StatusCodes.Status400BadRequest);
            }

            var currentDirectionExists = await _unitOfWork.Db.CurrentDirections.AnyAsync(x => x.Id == dto.CurrentDirectionId && !x.IsDeleted);
            if (!currentDirectionExists)
            {
                return ApiResponse<CurrentDirectionMatchDto>.ErrorResult("Seçilen akıntı yönü bulunamadı.", "Seçilen akıntı yönü bulunamadı.", StatusCodes.Status400BadRequest);
            }

            var recordDate = dto.RecordDate.Date;
            var duplicateExists = await _unitOfWork.Db.CurrentDirectionMatches.AnyAsync(x =>
                x.ProjectId == dto.ProjectId &&
                x.ProjectCageId == dto.ProjectCageId &&
                x.RecordDate == recordDate &&
                !x.IsDeleted &&
                (!currentId.HasValue || x.Id != currentId.Value));

            if (duplicateExists)
            {
                return ApiResponse<CurrentDirectionMatchDto>.ErrorResult("Bu proje, kafes ve tarih için akıntı yönü kaydı zaten var.", "Bu proje, kafes ve tarih için akıntı yönü kaydı zaten var.", StatusCodes.Status409Conflict);
            }

            return ApiResponse<CurrentDirectionMatchDto>.SuccessResult(new CurrentDirectionMatchDto(), "Valid");
        }

        private static CurrentDirectionMatchDto Map(CurrentDirectionMatchEntity entity)
        {
            return new CurrentDirectionMatchDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                ProjectCode = entity.Project?.ProjectCode,
                ProjectName = entity.Project?.ProjectName,
                ProjectCageId = entity.ProjectCageId,
                CageId = entity.ProjectCage?.CageId,
                CageCode = entity.ProjectCage?.Cage?.CageCode,
                CageName = entity.ProjectCage?.Cage?.CageName,
                CurrentDirectionId = entity.CurrentDirectionId,
                CurrentDirectionName = entity.CurrentDirection?.Name,
                RecordDate = entity.RecordDate,
                Note = entity.Note
            };
        }
    }
}
