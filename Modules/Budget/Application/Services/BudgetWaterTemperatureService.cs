using aqua_api.Modules.Budget.Domain.Entities;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Budget.Application.Services
{
    public class BudgetWaterTemperatureService : IBudgetWaterTemperatureService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BudgetWaterTemperatureService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<BudgetWaterTemperatureDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Db.BudgetWaterTemperatures
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<BudgetWaterTemperatureDto>.ErrorResult("Bütçe su sıcaklığı tanımı bulunamadı.", "Bütçe su sıcaklığı tanımı bulunamadı.", StatusCodes.Status404NotFound);
                }

                return ApiResponse<BudgetWaterTemperatureDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetWaterTemperatureDto>.ErrorResult("Bütçe su sıcaklığı tanımı getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<BudgetWaterTemperatureDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Db.BudgetWaterTemperatures
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(BudgetWaterTemperature.Year) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                var response = new PagedResponse<BudgetWaterTemperatureDto>
                {
                    Items = entities.Select(Map).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<BudgetWaterTemperatureDto>>.SuccessResult(response, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<BudgetWaterTemperatureDto>>.ErrorResult("Bütçe su sıcaklığı tanımları getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BudgetWaterTemperatureDto>> CreateAsync(CreateBudgetWaterTemperatureDto dto)
        {
            try
            {
                var validation = ValidateBaseFields(dto);
                if (!validation.Success)
                {
                    return validation;
                }

                var entity = await _unitOfWork.Db.BudgetWaterTemperatures
                    .FirstOrDefaultAsync(x => x.Year == dto.Year && x.Month == dto.Month && !x.IsDeleted);

                if (entity == null)
                {
                    entity = new BudgetWaterTemperature
                    {
                        Year = dto.Year,
                        Month = dto.Month,
                        WaterTemperatureCelsius = dto.WaterTemperatureCelsius,
                        Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
                    };

                    await _unitOfWork.Repository<BudgetWaterTemperature>().AddAsync(entity);
                }
                else
                {
                    entity.WaterTemperatureCelsius = dto.WaterTemperatureCelsius;
                    entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

                    await _unitOfWork.Repository<BudgetWaterTemperature>().UpdateAsync(entity);
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<BudgetWaterTemperatureDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetWaterTemperatureDto>.ErrorResult("Bütçe su sıcaklığı tanımı oluşturulamadı.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BudgetWaterTemperatureDto>> UpdateAsync(long id, UpdateBudgetWaterTemperatureDto dto)
        {
            try
            {
                var entity = await _unitOfWork.Db.BudgetWaterTemperatures
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<BudgetWaterTemperatureDto>.ErrorResult("Bütçe su sıcaklığı tanımı bulunamadı.", "Bütçe su sıcaklığı tanımı bulunamadı.", StatusCodes.Status404NotFound);
                }

                var validation = await ValidateAsync(dto, id);
                if (!validation.Success)
                {
                    return validation;
                }

                entity.Year = dto.Year;
                entity.Month = dto.Month;
                entity.WaterTemperatureCelsius = dto.WaterTemperatureCelsius;
                entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

                await _unitOfWork.Repository<BudgetWaterTemperature>().UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<BudgetWaterTemperatureDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetWaterTemperatureDto>.ErrorResult("Bütçe su sıcaklığı tanımı güncellenemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var deleted = await _unitOfWork.Repository<BudgetWaterTemperature>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult("Bütçe su sıcaklığı tanımı bulunamadı.", "Bütçe su sıcaklığı tanımı bulunamadı.", StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult("Bütçe su sıcaklığı tanımı silinemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<ApiResponse<BudgetWaterTemperatureDto>> ValidateAsync(CreateBudgetWaterTemperatureDto dto, long? currentId = null)
        {
            var baseValidation = ValidateBaseFields(dto);
            if (!baseValidation.Success)
            {
                return baseValidation;
            }

            var exists = await _unitOfWork.Db.BudgetWaterTemperatures.AnyAsync(x =>
                x.Year == dto.Year &&
                x.Month == dto.Month &&
                !x.IsDeleted &&
                (!currentId.HasValue || x.Id != currentId.Value));

            if (exists)
            {
                return ApiResponse<BudgetWaterTemperatureDto>.ErrorResult("Bu yıl ve ay için bütçe su sıcaklığı tanımı zaten var.", "Bu yıl ve ay için bütçe su sıcaklığı tanımı zaten var.", StatusCodes.Status409Conflict);
            }

            return ApiResponse<BudgetWaterTemperatureDto>.SuccessResult(new BudgetWaterTemperatureDto(), "Valid");
        }

        private static ApiResponse<BudgetWaterTemperatureDto> ValidateBaseFields(CreateBudgetWaterTemperatureDto dto)
        {
            if (dto.Year < 2000 || dto.Year > 2100)
            {
                return ApiResponse<BudgetWaterTemperatureDto>.ErrorResult("Yıl 2000 ile 2100 arasında olmalıdır.", "Yıl 2000 ile 2100 arasında olmalıdır.", StatusCodes.Status400BadRequest);
            }

            if (dto.Month < 1 || dto.Month > 12)
            {
                return ApiResponse<BudgetWaterTemperatureDto>.ErrorResult("Ay 1 ile 12 arasında olmalıdır.", "Ay 1 ile 12 arasında olmalıdır.", StatusCodes.Status400BadRequest);
            }

            if (dto.WaterTemperatureCelsius < -5 || dto.WaterTemperatureCelsius > 45)
            {
                return ApiResponse<BudgetWaterTemperatureDto>.ErrorResult("Su sıcaklığı -5 ile 45 derece arasında olmalıdır.", "Su sıcaklığı -5 ile 45 derece arasında olmalıdır.", StatusCodes.Status400BadRequest);
            }

            return ApiResponse<BudgetWaterTemperatureDto>.SuccessResult(new BudgetWaterTemperatureDto(), "Valid");
        }

        private static BudgetWaterTemperatureDto Map(BudgetWaterTemperature entity)
        {
            return new BudgetWaterTemperatureDto
            {
                Id = entity.Id,
                Year = entity.Year,
                Month = entity.Month,
                WaterTemperatureCelsius = entity.WaterTemperatureCelsius,
                Description = entity.Description
            };
        }
    }
}
