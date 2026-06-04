using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WindDirectionEntity = aqua_api.Modules.WindDirection.Domain.Entities.WindDirection;

namespace aqua_api.Modules.WindDirection.Application.Services
{
    public class WindDirectionService : IWindDirectionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WindDirectionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<WindDirectionDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Db.WindDirections
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<WindDirectionDto>.ErrorResult("Rüzgar yönü bulunamadı.", "Rüzgar yönü bulunamadı.", StatusCodes.Status404NotFound);
                }

                return ApiResponse<WindDirectionDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<WindDirectionDto>.ErrorResult("Rüzgar yönü getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<WindDirectionDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Db.WindDirections
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(WindDirectionEntity.Name) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                var response = new PagedResponse<WindDirectionDto>
                {
                    Items = entities.Select(Map).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<WindDirectionDto>>.SuccessResult(response, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<WindDirectionDto>>.ErrorResult("Rüzgar yönleri getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WindDirectionDto>> CreateAsync(CreateWindDirectionDto dto)
        {
            try
            {
                var validation = await ValidateAsync(dto);
                if (!validation.Success)
                {
                    return validation;
                }

                var entity = new WindDirectionEntity
                {
                    Name = dto.Name.Trim()
                };

                await _unitOfWork.Repository<WindDirectionEntity>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<WindDirectionDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<WindDirectionDto>.ErrorResult("Rüzgar yönü oluşturulamadı.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<WindDirectionDto>> UpdateAsync(long id, UpdateWindDirectionDto dto)
        {
            try
            {
                var entity = await _unitOfWork.Db.WindDirections
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<WindDirectionDto>.ErrorResult("Rüzgar yönü bulunamadı.", "Rüzgar yönü bulunamadı.", StatusCodes.Status404NotFound);
                }

                var validation = await ValidateAsync(dto, id);
                if (!validation.Success)
                {
                    return validation;
                }

                entity.Name = dto.Name.Trim();

                await _unitOfWork.Repository<WindDirectionEntity>().UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<WindDirectionDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<WindDirectionDto>.ErrorResult("Rüzgar yönü güncellenemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var used = await _unitOfWork.Db.WindDirectionMatches.AnyAsync(x => x.WindDirectionId == id && !x.IsDeleted);
                if (used)
                {
                    return ApiResponse<bool>.ErrorResult("Günlük kayıtlarda kullanılan rüzgar yönü silinemez.", "Günlük kayıtlarda kullanılan rüzgar yönü silinemez.", StatusCodes.Status409Conflict);
                }

                var deleted = await _unitOfWork.Repository<WindDirectionEntity>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult("Rüzgar yönü bulunamadı.", "Rüzgar yönü bulunamadı.", StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult("Rüzgar yönü silinemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<ApiResponse<WindDirectionDto>> ValidateAsync(CreateWindDirectionDto dto, long? currentId = null)
        {
            var name = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return ApiResponse<WindDirectionDto>.ErrorResult("Rüzgar yönü adı zorunludur.", "Rüzgar yönü adı zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (name.Length > 50)
            {
                return ApiResponse<WindDirectionDto>.ErrorResult("Rüzgar yönü adı en fazla 50 karakter olabilir.", "Rüzgar yönü adı en fazla 50 karakter olabilir.", StatusCodes.Status400BadRequest);
            }

            var exists = await _unitOfWork.Db.WindDirections.AnyAsync(x =>
                x.Name == name &&
                !x.IsDeleted &&
                (!currentId.HasValue || x.Id != currentId.Value));

            if (exists)
            {
                return ApiResponse<WindDirectionDto>.ErrorResult("Bu rüzgar yönü zaten tanımlı.", "Bu rüzgar yönü zaten tanımlı.", StatusCodes.Status409Conflict);
            }

            return ApiResponse<WindDirectionDto>.SuccessResult(new WindDirectionDto(), "Valid");
        }

        private static WindDirectionDto Map(WindDirectionEntity entity)
        {
            return new WindDirectionDto
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }
    }
}
