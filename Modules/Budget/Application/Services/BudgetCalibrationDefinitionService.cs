using aqua_api.Modules.Budget.Domain.Entities;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Budget.Application.Services
{
    public class BudgetCalibrationDefinitionService : IBudgetCalibrationDefinitionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BudgetCalibrationDefinitionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<BudgetCalibrationDefinitionDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Db.BudgetCalibrationDefinitions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<BudgetCalibrationDefinitionDto>.ErrorResult("Kalibrasyon tanımı bulunamadı.", "Kalibrasyon tanımı bulunamadı.", StatusCodes.Status404NotFound);
                }

                return ApiResponse<BudgetCalibrationDefinitionDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetCalibrationDefinitionDto>.ErrorResult("Kalibrasyon tanımı getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<BudgetCalibrationDefinitionDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Db.BudgetCalibrationDefinitions
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(BudgetCalibrationDefinition.CalibrationCode) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                var response = new PagedResponse<BudgetCalibrationDefinitionDto>
                {
                    Items = entities.Select(Map).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<BudgetCalibrationDefinitionDto>>.SuccessResult(response, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<BudgetCalibrationDefinitionDto>>.ErrorResult("Kalibrasyon tanımları getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BudgetCalibrationDefinitionDto>> CreateAsync(CreateBudgetCalibrationDefinitionDto dto)
        {
            try
            {
                var validation = await ValidateAsync(dto);
                if (!validation.Success)
                {
                    return validation;
                }

                var entity = new BudgetCalibrationDefinition
                {
                    CalibrationCode = NormalizeRequired(dto.CalibrationCode),
                    CalibrationInfo = NormalizeRequired(dto.CalibrationInfo),
                    Description = NormalizeOptional(dto.Description)
                };

                await _unitOfWork.Repository<BudgetCalibrationDefinition>().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<BudgetCalibrationDefinitionDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetCalibrationDefinitionDto>.ErrorResult("Kalibrasyon tanımı oluşturulamadı.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BudgetCalibrationDefinitionDto>> UpdateAsync(long id, UpdateBudgetCalibrationDefinitionDto dto)
        {
            try
            {
                var entity = await _unitOfWork.Db.BudgetCalibrationDefinitions
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<BudgetCalibrationDefinitionDto>.ErrorResult("Kalibrasyon tanımı bulunamadı.", "Kalibrasyon tanımı bulunamadı.", StatusCodes.Status404NotFound);
                }

                var validation = await ValidateAsync(dto, id);
                if (!validation.Success)
                {
                    return validation;
                }

                entity.CalibrationCode = NormalizeRequired(dto.CalibrationCode);
                entity.CalibrationInfo = NormalizeRequired(dto.CalibrationInfo);
                entity.Description = NormalizeOptional(dto.Description);

                await _unitOfWork.Repository<BudgetCalibrationDefinition>().UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<BudgetCalibrationDefinitionDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetCalibrationDefinitionDto>.ErrorResult("Kalibrasyon tanımı güncellenemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var deleted = await _unitOfWork.Repository<BudgetCalibrationDefinition>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult("Kalibrasyon tanımı bulunamadı.", "Kalibrasyon tanımı bulunamadı.", StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult("Kalibrasyon tanımı silinemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<ApiResponse<BudgetCalibrationDefinitionDto>> ValidateAsync(CreateBudgetCalibrationDefinitionDto dto, long? currentId = null)
        {
            if (string.IsNullOrWhiteSpace(dto.CalibrationCode))
            {
                return ApiResponse<BudgetCalibrationDefinitionDto>.ErrorResult("Kalibrasyon kodu zorunludur.", "Kalibrasyon kodu zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (dto.CalibrationCode.Trim().Length > 50)
            {
                return ApiResponse<BudgetCalibrationDefinitionDto>.ErrorResult("Kalibrasyon kodu en fazla 50 karakter olabilir.", "Kalibrasyon kodu en fazla 50 karakter olabilir.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(dto.CalibrationInfo))
            {
                return ApiResponse<BudgetCalibrationDefinitionDto>.ErrorResult("Kalibrasyon bilgisi zorunludur.", "Kalibrasyon bilgisi zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (dto.CalibrationInfo.Trim().Length > 250)
            {
                return ApiResponse<BudgetCalibrationDefinitionDto>.ErrorResult("Kalibrasyon bilgisi en fazla 250 karakter olabilir.", "Kalibrasyon bilgisi en fazla 250 karakter olabilir.", StatusCodes.Status400BadRequest);
            }

            if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description.Trim().Length > 500)
            {
                return ApiResponse<BudgetCalibrationDefinitionDto>.ErrorResult("Açıklama en fazla 500 karakter olabilir.", "Açıklama en fazla 500 karakter olabilir.", StatusCodes.Status400BadRequest);
            }

            var normalizedCode = NormalizeRequired(dto.CalibrationCode);
            var exists = await _unitOfWork.Db.BudgetCalibrationDefinitions.AnyAsync(x =>
                x.CalibrationCode == normalizedCode &&
                !x.IsDeleted &&
                (!currentId.HasValue || x.Id != currentId.Value));

            if (exists)
            {
                return ApiResponse<BudgetCalibrationDefinitionDto>.ErrorResult("Bu kalibrasyon kodu ile tanım zaten var.", "Bu kalibrasyon kodu ile tanım zaten var.", StatusCodes.Status409Conflict);
            }

            return ApiResponse<BudgetCalibrationDefinitionDto>.SuccessResult(new BudgetCalibrationDefinitionDto(), "Valid");
        }

        private static BudgetCalibrationDefinitionDto Map(BudgetCalibrationDefinition entity)
        {
            return new BudgetCalibrationDefinitionDto
            {
                Id = entity.Id,
                CalibrationCode = entity.CalibrationCode,
                CalibrationInfo = entity.CalibrationInfo,
                Description = entity.Description
            };
        }

        private static string NormalizeRequired(string value)
        {
            return value.Trim();
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
