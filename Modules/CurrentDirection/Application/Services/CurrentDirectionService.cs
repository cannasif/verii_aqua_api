using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using CurrentDirectionEntity = aqua_api.Modules.CurrentDirection.Domain.Entities.CurrentDirection;

namespace aqua_api.Modules.CurrentDirection.Application.Services
{
    public class CurrentDirectionService : ICurrentDirectionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CurrentDirectionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<CurrentDirectionDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Db.CurrentDirections
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<CurrentDirectionDto>.ErrorResult("Akıntı yönü bulunamadı.", "Akıntı yönü bulunamadı.", StatusCodes.Status404NotFound);
                }

                return ApiResponse<CurrentDirectionDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<CurrentDirectionDto>.ErrorResult("Akıntı yönü getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<CurrentDirectionDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Db.CurrentDirections
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(CurrentDirectionEntity.Name) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                var response = new PagedResponse<CurrentDirectionDto>
                {
                    Items = entities.Select(Map).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<CurrentDirectionDto>>.SuccessResult(response, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<CurrentDirectionDto>>.ErrorResult("Akıntı yönleri getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<CurrentDirectionDto>> CreateAsync(CreateCurrentDirectionDto dto)
        {
            try
            {
                var validation = await ValidateAsync(dto);
                if (!validation.Success)
                {
                    return validation;
                }

                var entity = new CurrentDirectionEntity
                {
                    Name = dto.Name.Trim()
                };

                await _unitOfWork.Repository<CurrentDirectionEntity>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<CurrentDirectionDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<CurrentDirectionDto>.ErrorResult("Akıntı yönü oluşturulamadı.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<CurrentDirectionDto>> UpdateAsync(long id, UpdateCurrentDirectionDto dto)
        {
            try
            {
                var entity = await _unitOfWork.Db.CurrentDirections
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<CurrentDirectionDto>.ErrorResult("Akıntı yönü bulunamadı.", "Akıntı yönü bulunamadı.", StatusCodes.Status404NotFound);
                }

                var validation = await ValidateAsync(dto, id);
                if (!validation.Success)
                {
                    return validation;
                }

                entity.Name = dto.Name.Trim();

                await _unitOfWork.Repository<CurrentDirectionEntity>().UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<CurrentDirectionDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<CurrentDirectionDto>.ErrorResult("Akıntı yönü güncellenemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var used = await _unitOfWork.Db.CurrentDirectionMatches.AnyAsync(x => x.CurrentDirectionId == id && !x.IsDeleted);
                if (used)
                {
                    return ApiResponse<bool>.ErrorResult("Günlük kayıtlarda kullanılan akıntı yönü silinemez.", "Günlük kayıtlarda kullanılan akıntı yönü silinemez.", StatusCodes.Status409Conflict);
                }

                var deleted = await _unitOfWork.Repository<CurrentDirectionEntity>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult("Akıntı yönü bulunamadı.", "Akıntı yönü bulunamadı.", StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult("Akıntı yönü silinemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<ApiResponse<CurrentDirectionDto>> ValidateAsync(CreateCurrentDirectionDto dto, long? currentId = null)
        {
            var name = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return ApiResponse<CurrentDirectionDto>.ErrorResult("Akıntı yönü adı zorunludur.", "Akıntı yönü adı zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (name.Length > 50)
            {
                return ApiResponse<CurrentDirectionDto>.ErrorResult("Akıntı yönü adı en fazla 50 karakter olabilir.", "Akıntı yönü adı en fazla 50 karakter olabilir.", StatusCodes.Status400BadRequest);
            }

            var exists = await _unitOfWork.Db.CurrentDirections.AnyAsync(x =>
                x.Name == name &&
                !x.IsDeleted &&
                (!currentId.HasValue || x.Id != currentId.Value));

            if (exists)
            {
                return ApiResponse<CurrentDirectionDto>.ErrorResult("Bu akıntı yönü zaten tanımlı.", "Bu akıntı yönü zaten tanımlı.", StatusCodes.Status409Conflict);
            }

            return ApiResponse<CurrentDirectionDto>.SuccessResult(new CurrentDirectionDto(), "Valid");
        }

        private static CurrentDirectionDto Map(CurrentDirectionEntity entity)
        {
            return new CurrentDirectionDto
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }
    }
}
