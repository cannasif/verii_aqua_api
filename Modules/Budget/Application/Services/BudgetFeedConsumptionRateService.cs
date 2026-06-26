using aqua_api.Modules.Budget.Domain.Entities;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Budget.Application.Services
{
    public class BudgetFeedConsumptionRateService : IBudgetFeedConsumptionRateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly IReadOnlyDictionary<string, string> ColumnMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["waterTemperatureYear"] = "WaterTemperature.Year",
            ["waterTemperatureMonth"] = "WaterTemperature.Month",
            ["waterTemperatureCelsius"] = "WaterTemperature.WaterTemperatureCelsius",
            ["calibrationCode"] = "CalibrationDefinition.CalibrationCode",
            ["calibrationInfo"] = "CalibrationDefinition.CalibrationInfo",
            ["feedStockCode"] = "FeedStock.ErpStockCode",
            ["feedStockName"] = "FeedStock.StockName"
        };

        public BudgetFeedConsumptionRateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<BudgetFeedConsumptionRateDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await BaseQuery()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Yem tüketim oranı tanımı bulunamadı.", "Yem tüketim oranı tanımı bulunamadı.", StatusCodes.Status404NotFound);
                }

                return ApiResponse<BudgetFeedConsumptionRateDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Yem tüketim oranı tanımı getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<BudgetFeedConsumptionRateDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = BaseQuery()
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic, ColumnMapping);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(BudgetFeedConsumptionRate.WaterTemperatureId) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection, ColumnMapping);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                var response = new PagedResponse<BudgetFeedConsumptionRateDto>
                {
                    Items = entities.Select(Map).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<BudgetFeedConsumptionRateDto>>.SuccessResult(response, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<BudgetFeedConsumptionRateDto>>.ErrorResult("Yem tüketim oranı tanımları getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<StockGetDto>>> GetFeedStocksAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Db.Stocks
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.GrupKodu != null && x.GrupKodu.ToUpper() == "YEM")
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "ErpStockCode" : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                var response = new PagedResponse<StockGetDto>
                {
                    Items = entities.Select(MapStock).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<StockGetDto>>.SuccessResult(response, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<StockGetDto>>.ErrorResult("Yem stokları getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BudgetFeedConsumptionRateDto>> CreateAsync(CreateBudgetFeedConsumptionRateDto dto)
        {
            try
            {
                var validation = await ValidateAsync(dto);
                if (!validation.Success)
                {
                    return validation;
                }

                var entity = await _unitOfWork.Db.BudgetFeedConsumptionRates
                    .FirstOrDefaultAsync(x =>
                        x.WaterTemperatureId == dto.WaterTemperatureId &&
                        x.CalibrationDefinitionId == dto.CalibrationDefinitionId &&
                        x.FeedStockId == dto.FeedStockId &&
                        !x.IsDeleted);

                if (entity == null)
                {
                    entity = new BudgetFeedConsumptionRate
                    {
                        WaterTemperatureId = dto.WaterTemperatureId,
                        CalibrationDefinitionId = dto.CalibrationDefinitionId,
                        FeedStockId = dto.FeedStockId,
                        FeedAmount = dto.FeedAmount,
                        Description = NormalizeOptional(dto.Description)
                    };

                    await _unitOfWork.Repository<BudgetFeedConsumptionRate>().AddAsync(entity);
                }
                else
                {
                    entity.FeedAmount = dto.FeedAmount;
                    entity.Description = NormalizeOptional(dto.Description);
                    await _unitOfWork.Repository<BudgetFeedConsumptionRate>().UpdateAsync(entity);
                }

                await _unitOfWork.SaveChangesAsync();

                var saved = await BaseQuery().AsNoTracking().FirstAsync(x => x.Id == entity.Id);
                return ApiResponse<BudgetFeedConsumptionRateDto>.SuccessResult(Map(saved), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Yem tüketim oranı tanımı kaydedilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BudgetFeedConsumptionRateDto>> UpdateAsync(long id, UpdateBudgetFeedConsumptionRateDto dto)
        {
            try
            {
                var entity = await _unitOfWork.Db.BudgetFeedConsumptionRates
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Yem tüketim oranı tanımı bulunamadı.", "Yem tüketim oranı tanımı bulunamadı.", StatusCodes.Status404NotFound);
                }

                var validation = await ValidateAsync(dto, id);
                if (!validation.Success)
                {
                    return validation;
                }

                entity.WaterTemperatureId = dto.WaterTemperatureId;
                entity.CalibrationDefinitionId = dto.CalibrationDefinitionId;
                entity.FeedStockId = dto.FeedStockId;
                entity.FeedAmount = dto.FeedAmount;
                entity.Description = NormalizeOptional(dto.Description);

                await _unitOfWork.Repository<BudgetFeedConsumptionRate>().UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var saved = await BaseQuery().AsNoTracking().FirstAsync(x => x.Id == entity.Id);
                return ApiResponse<BudgetFeedConsumptionRateDto>.SuccessResult(Map(saved), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Yem tüketim oranı tanımı güncellenemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var deleted = await _unitOfWork.Repository<BudgetFeedConsumptionRate>().SoftDeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.ErrorResult("Yem tüketim oranı tanımı bulunamadı.", "Yem tüketim oranı tanımı bulunamadı.", StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult("Yem tüketim oranı tanımı silinemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        private IQueryable<BudgetFeedConsumptionRate> BaseQuery()
        {
            return _unitOfWork.Db.BudgetFeedConsumptionRates
                .Include(x => x.WaterTemperature)
                .Include(x => x.CalibrationDefinition)
                .Include(x => x.FeedStock);
        }

        private async Task<ApiResponse<BudgetFeedConsumptionRateDto>> ValidateAsync(CreateBudgetFeedConsumptionRateDto dto, long? currentId = null)
        {
            if (dto.WaterTemperatureId <= 0)
            {
                return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Su sıcaklığı seçimi zorunludur.", "Su sıcaklığı seçimi zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (dto.CalibrationDefinitionId <= 0)
            {
                return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Kalibrasyon seçimi zorunludur.", "Kalibrasyon seçimi zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (dto.FeedStockId <= 0)
            {
                return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Yem seçimi zorunludur.", "Yem seçimi zorunludur.", StatusCodes.Status400BadRequest);
            }

            if (dto.FeedAmount <= 0)
            {
                return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Yem miktarı sıfırdan büyük olmalıdır.", "Yem miktarı sıfırdan büyük olmalıdır.", StatusCodes.Status400BadRequest);
            }

            if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description.Trim().Length > 500)
            {
                return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Açıklama en fazla 500 karakter olabilir.", "Açıklama en fazla 500 karakter olabilir.", StatusCodes.Status400BadRequest);
            }

            var waterTemperatureExists = await _unitOfWork.Db.BudgetWaterTemperatures
                .AnyAsync(x => x.Id == dto.WaterTemperatureId && !x.IsDeleted);
            if (!waterTemperatureExists)
            {
                return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Seçilen su sıcaklığı tanımı bulunamadı.", "Seçilen su sıcaklığı tanımı bulunamadı.", StatusCodes.Status400BadRequest);
            }

            var calibrationExists = await _unitOfWork.Db.BudgetCalibrationDefinitions
                .AnyAsync(x => x.Id == dto.CalibrationDefinitionId && !x.IsDeleted);
            if (!calibrationExists)
            {
                return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Seçilen kalibrasyon tanımı bulunamadı.", "Seçilen kalibrasyon tanımı bulunamadı.", StatusCodes.Status400BadRequest);
            }

            var feedStockExists = await _unitOfWork.Db.Stocks
                .AnyAsync(x => x.Id == dto.FeedStockId && !x.IsDeleted && x.GrupKodu != null && x.GrupKodu.ToUpper() == "YEM");
            if (!feedStockExists)
            {
                return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Seçilen stok yem grubu içinde değildir.", "Seçilen stok yem grubu içinde değildir.", StatusCodes.Status400BadRequest);
            }

            var duplicateExists = await _unitOfWork.Db.BudgetFeedConsumptionRates.AnyAsync(x =>
                x.WaterTemperatureId == dto.WaterTemperatureId &&
                x.CalibrationDefinitionId == dto.CalibrationDefinitionId &&
                x.FeedStockId == dto.FeedStockId &&
                !x.IsDeleted &&
                currentId.HasValue &&
                x.Id != currentId.Value);

            if (duplicateExists)
            {
                return ApiResponse<BudgetFeedConsumptionRateDto>.ErrorResult("Bu su sıcaklığı, kalibrasyon ve yem kombinasyonu için tanım zaten var.", "Bu su sıcaklığı, kalibrasyon ve yem kombinasyonu için tanım zaten var.", StatusCodes.Status409Conflict);
            }

            return ApiResponse<BudgetFeedConsumptionRateDto>.SuccessResult(new BudgetFeedConsumptionRateDto(), "Valid");
        }

        private static BudgetFeedConsumptionRateDto Map(BudgetFeedConsumptionRate entity)
        {
            return new BudgetFeedConsumptionRateDto
            {
                Id = entity.Id,
                WaterTemperatureId = entity.WaterTemperatureId,
                WaterTemperatureYear = entity.WaterTemperature?.Year,
                WaterTemperatureMonth = entity.WaterTemperature?.Month,
                WaterTemperatureCelsius = entity.WaterTemperature?.WaterTemperatureCelsius,
                CalibrationDefinitionId = entity.CalibrationDefinitionId,
                CalibrationCode = entity.CalibrationDefinition?.CalibrationCode,
                CalibrationInfo = entity.CalibrationDefinition?.CalibrationInfo,
                FeedStockId = entity.FeedStockId,
                FeedStockCode = entity.FeedStock?.ErpStockCode,
                FeedStockName = entity.FeedStock?.StockName,
                FeedAmount = entity.FeedAmount,
                Description = entity.Description
            };
        }

        private static StockGetDto MapStock(aqua_api.Modules.Stock.Domain.Entities.Stock stock)
        {
            return new StockGetDto
            {
                Id = stock.Id,
                ErpStockCode = stock.ErpStockCode,
                StockName = stock.StockName,
                Unit = stock.Unit,
                UreticiKodu = stock.UreticiKodu,
                GrupKodu = stock.GrupKodu,
                GrupAdi = stock.GrupAdi,
                Kod1 = stock.Kod1,
                Kod1Adi = stock.Kod1Adi,
                Kod2 = stock.Kod2,
                Kod2Adi = stock.Kod2Adi,
                Kod3 = stock.Kod3,
                Kod3Adi = stock.Kod3Adi,
                Kod4 = stock.Kod4,
                Kod4Adi = stock.Kod4Adi,
                Kod5 = stock.Kod5,
                Kod5Adi = stock.Kod5Adi,
                BranchCode = stock.BranchCode,
                IsERPIntegrated = stock.IsERPIntegrated,
                ERPIntegrationNumber = stock.ERPIntegrationNumber,
                LastSyncDate = stock.LastSyncDate,
                CountTriedBy = stock.CountTriedBy
            };
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
