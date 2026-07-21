using aqua_api.Modules.Budget.Domain.Entities;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using StockEntity = aqua_api.Modules.Stock.Domain.Entities.Stock;

namespace aqua_api.Modules.Budget.Application.Services
{
    public class BudgetFishGrowthProfileService : IBudgetFishGrowthProfileService
    {
        private const int MaxGrowthMonthNo = 100;
        private readonly IUnitOfWork _unitOfWork;

        public BudgetFishGrowthProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<PagedResponse<BudgetFishGrowthProfileSummaryDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Db.BudgetFishGrowthProfiles
                    .AsNoTracking()
                    .Include(x => x.Stock)
                    .Include(x => x.Lines)
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(BudgetFishGrowthProfile.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

                var response = new PagedResponse<BudgetFishGrowthProfileSummaryDto>
                {
                    Items = entities.Select(MapSummary).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<BudgetFishGrowthProfileSummaryDto>>.SuccessResult(response, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<BudgetFishGrowthProfileSummaryDto>>.ErrorResult("Balık büyüme parametreleri getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BudgetFishGrowthProfileDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await GetProfileQuery()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<BudgetFishGrowthProfileDto>.ErrorResult("Balık büyüme parametresi bulunamadı.", "Balık büyüme parametresi bulunamadı.", StatusCodes.Status404NotFound);
                }

                return ApiResponse<BudgetFishGrowthProfileDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetFishGrowthProfileDto>.ErrorResult("Balık büyüme parametresi getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BudgetFishGrowthProfileDto>> GetByStockAndStartMonthAsync(long stockId, int startMonth)
        {
            try
            {
                if (stockId <= 0)
                {
                    return ApiResponse<BudgetFishGrowthProfileDto>.ErrorResult("Stok seçimi zorunludur.", "Stok seçimi zorunludur.", StatusCodes.Status400BadRequest);
                }

                if (!IsValidMonth(startMonth))
                {
                    return ApiResponse<BudgetFishGrowthProfileDto>.ErrorResult("Başlangıç ayı 1 ile 12 arasında olmalıdır.", "Başlangıç ayı 1 ile 12 arasında olmalıdır.", StatusCodes.Status400BadRequest);
                }

                var entity = await GetProfileQuery()
                    .FirstOrDefaultAsync(x => x.StockId == stockId && x.StartMonth == startMonth && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<BudgetFishGrowthProfileDto>.SuccessResult(
                        CreateEmptyProfile(stockId, startMonth),
                        "Balık büyüme parametresi henüz oluşturulmamış.");
                }

                return ApiResponse<BudgetFishGrowthProfileDto>.SuccessResult(Map(entity), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetFishGrowthProfileDto>.ErrorResult("Balık büyüme parametresi getirilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BudgetFishGrowthProfileDto>> UpsertAsync(UpsertBudgetFishGrowthProfileDto dto)
        {
            try
            {
                var validation = await ValidateAsync(dto);
                if (!validation.Success)
                {
                    return validation;
                }

                await _unitOfWork.BeginTransactionAsync();

                var stock = await _unitOfWork.Db.Stocks
                    .FirstAsync(x => x.Id == dto.StockId && !x.IsDeleted);
                var profile = await UpsertProfileEntityAsync(dto, stock);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var result = await GetProfileQuery()
                    .FirstAsync(x => x.Id == profile.Id);

                return ApiResponse<BudgetFishGrowthProfileDto>.SuccessResult(Map(result), "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<BudgetFishGrowthProfileDto>.ErrorResult("Balık büyüme parametresi kaydedilemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<ImportBudgetFishGrowthProfilesResultDto>> ImportAsync(
            ImportBudgetFishGrowthProfilesDto dto)
        {
            try
            {
                if (dto?.Rows == null || dto.Rows.Count == 0)
                {
                    return ApiResponse<ImportBudgetFishGrowthProfilesResultDto>.ErrorResult(
                        "İçe aktarılacak büyüme satırı bulunamadı.",
                        "İçe aktarılacak büyüme satırı bulunamadı.",
                        StatusCodes.Status400BadRequest);
                }

                if (dto.Rows.Count > 10000)
                {
                    return ApiResponse<ImportBudgetFishGrowthProfilesResultDto>.ErrorResult(
                        "Tek seferde en fazla 10.000 büyüme satırı içe aktarılabilir.",
                        "Tek seferde en fazla 10.000 büyüme satırı içe aktarılabilir.",
                        StatusCodes.Status400BadRequest);
                }

                var normalizedRows = dto.Rows.Select(row => new ImportBudgetFishGrowthProfileRowDto
                {
                    StockCode = row.StockCode?.Trim() ?? string.Empty,
                    StartMonth = row.StartMonth,
                    GrowthMonthNo = row.GrowthMonthNo,
                    MonthlyGrowthGram = row.MonthlyGrowthGram
                }).ToList();

                if (normalizedRows.Any(row => string.IsNullOrWhiteSpace(row.StockCode)))
                {
                    return ImportValidationError("Stok kodu boş olamaz.");
                }

                if (normalizedRows.Any(row => !IsValidMonth(row.StartMonth)))
                {
                    return ImportValidationError("Başlangıç büyütme ayı 1 ile 12 arasında olmalıdır.");
                }

                if (normalizedRows.Any(row => row.GrowthMonthNo < 1 || row.GrowthMonthNo > MaxGrowthMonthNo))
                {
                    return ImportValidationError("Geçen ay 1 ile 100 arasında olmalıdır.");
                }

                if (normalizedRows.Any(row => row.MonthlyGrowthGram < 0))
                {
                    return ImportValidationError("Aylık artış gramı negatif olamaz.");
                }

                var duplicate = normalizedRows
                    .GroupBy(row => new
                    {
                        StockCode = row.StockCode.ToUpperInvariant(),
                        row.StartMonth,
                        row.GrowthMonthNo
                    })
                    .FirstOrDefault(group => group.Count() > 1);
                if (duplicate != null)
                {
                    return ImportValidationError(
                        $"{duplicate.Key.StockCode} stok kodu, {duplicate.Key.StartMonth} başlangıç ayı ve {duplicate.Key.GrowthMonthNo} geçen ay için tekrar eden satır var.");
                }

                var requestedStockCodes = normalizedRows
                    .Select(row => row.StockCode.ToUpperInvariant())
                    .Distinct()
                    .ToList();
                var stocks = await _unitOfWork.Db.Stocks
                    .Where(stock => !stock.IsDeleted && requestedStockCodes.Contains(stock.ErpStockCode.ToUpper()))
                    .ToListAsync();

                var duplicateStockCode = stocks
                    .GroupBy(stock => stock.ErpStockCode, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault(group => group.Count() > 1);
                if (duplicateStockCode != null)
                {
                    return ImportValidationError(
                        $"{duplicateStockCode.Key} stok kodu birden fazla aktif stok kaydıyla eşleşiyor.");
                }

                var stocksByCode = stocks.ToDictionary(
                    stock => stock.ErpStockCode,
                    StringComparer.OrdinalIgnoreCase);
                var missingStockCode = requestedStockCodes.FirstOrDefault(code => !stocksByCode.ContainsKey(code));
                if (missingStockCode != null)
                {
                    return ImportValidationError($"{missingStockCode} stok kodu sistemde bulunamadı.", StatusCodes.Status404NotFound);
                }

                var profileGroups = normalizedRows
                    .GroupBy(row => new
                    {
                        StockCode = row.StockCode.ToUpperInvariant(),
                        row.StartMonth
                    })
                    .ToList();

                await _unitOfWork.BeginTransactionAsync();

                foreach (var group in profileGroups)
                {
                    var stock = stocksByCode[group.Key.StockCode];
                    var upsert = new UpsertBudgetFishGrowthProfileDto
                    {
                        StockId = stock.Id,
                        StartMonth = group.Key.StartMonth,
                        Lines = group
                            .OrderBy(row => row.GrowthMonthNo)
                            .Select(row => new UpsertBudgetFishGrowthProfileLineDto
                            {
                                GrowthMonthNo = row.GrowthMonthNo,
                                MonthlyGrowthGram = row.MonthlyGrowthGram
                            })
                            .ToList()
                    };

                    await UpsertProfileEntityAsync(upsert, stock, preserveExistingMetadata: true);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<ImportBudgetFishGrowthProfilesResultDto>.SuccessResult(
                    new ImportBudgetFishGrowthProfilesResultDto
                    {
                        ProfileCount = profileGroups.Count,
                        RowCount = normalizedRows.Count
                    },
                    "Balık büyüme parametreleri içe aktarıldı.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<ImportBudgetFishGrowthProfilesResultDto>.ErrorResult(
                    "Balık büyüme parametreleri içe aktarılamadı.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Db.BudgetFishGrowthProfiles
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<bool>.ErrorResult("Balık büyüme parametresi bulunamadı.", "Balık büyüme parametresi bulunamadı.", StatusCodes.Status404NotFound);
                }

                entity.IsDeleted = true;
                entity.DeletedDate = DateTime.Now;

                foreach (var line in entity.Lines.Where(x => !x.IsDeleted))
                {
                    line.IsDeleted = true;
                    line.DeletedDate = DateTime.Now;
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "İşlem başarılı.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult("Balık büyüme parametresi silinemedi.", ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<ApiResponse<BudgetFishGrowthProfileDto>> ValidateAsync(UpsertBudgetFishGrowthProfileDto dto)
        {
            if (dto.StockId <= 0)
            {
                return ApiResponse<BudgetFishGrowthProfileDto>.ErrorResult("Stok seçimi zorunludur.", "Stok seçimi zorunludur.", StatusCodes.Status400BadRequest);
            }

            var stockExists = await _unitOfWork.Db.Stocks.AnyAsync(x => x.Id == dto.StockId && !x.IsDeleted);
            if (!stockExists)
            {
                return ApiResponse<BudgetFishGrowthProfileDto>.ErrorResult("Seçilen stok bulunamadı.", "Seçilen stok bulunamadı.", StatusCodes.Status404NotFound);
            }

            if (!IsValidMonth(dto.StartMonth))
            {
                return ApiResponse<BudgetFishGrowthProfileDto>.ErrorResult("Başlangıç ayı 1 ile 12 arasında olmalıdır.", "Başlangıç ayı 1 ile 12 arasında olmalıdır.", StatusCodes.Status400BadRequest);
            }

            if (dto.Lines.Any(x => x.GrowthMonthNo < 1 || x.GrowthMonthNo > MaxGrowthMonthNo))
            {
                return ApiResponse<BudgetFishGrowthProfileDto>.ErrorResult("Büyütme ayı 1 ile 100 arasında olmalıdır.", "Büyütme ayı 1 ile 100 arasında olmalıdır.", StatusCodes.Status400BadRequest);
            }

            if (dto.Lines.Any(x => x.MonthlyGrowthGram < 0))
            {
                return ApiResponse<BudgetFishGrowthProfileDto>.ErrorResult("Aylık büyüme gramı negatif olamaz.", "Aylık büyüme gramı negatif olamaz.", StatusCodes.Status400BadRequest);
            }

            return ApiResponse<BudgetFishGrowthProfileDto>.SuccessResult(new BudgetFishGrowthProfileDto(), "Valid");
        }

        private async Task<BudgetFishGrowthProfile> UpsertProfileEntityAsync(
            UpsertBudgetFishGrowthProfileDto dto,
            StockEntity stock,
            bool preserveExistingMetadata = false)
        {
            var profile = await _unitOfWork.Db.BudgetFishGrowthProfiles
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x =>
                    x.StockId == dto.StockId &&
                    x.StartMonth == dto.StartMonth &&
                    !x.IsDeleted);

            if (profile == null)
            {
                profile = new BudgetFishGrowthProfile
                {
                    StockId = dto.StockId,
                    StartMonth = dto.StartMonth,
                    Name = BuildProfileName(dto.Name, stock.StockName, dto.StartMonth),
                    Description = NormalizeDescription(dto.Description)
                };

                await _unitOfWork.Repository<BudgetFishGrowthProfile>().AddAsync(profile);
                await _unitOfWork.SaveChangesAsync();
            }
            else if (!preserveExistingMetadata)
            {
                profile.Name = BuildProfileName(dto.Name, stock.StockName, dto.StartMonth);
                profile.Description = NormalizeDescription(dto.Description);
                await _unitOfWork.Repository<BudgetFishGrowthProfile>().UpdateAsync(profile);
            }

            var normalizedLines = dto.Lines
                .Where(x => x.GrowthMonthNo >= 1 && x.GrowthMonthNo <= MaxGrowthMonthNo)
                .GroupBy(x => x.GrowthMonthNo)
                .Select(x => x.Last())
                .ToDictionary(x => x.GrowthMonthNo);

            var runningTotal = 0m;
            for (var monthNo = 1; monthNo <= MaxGrowthMonthNo; monthNo++)
            {
                var monthlyGrowth = normalizedLines.TryGetValue(monthNo, out var input)
                    ? input.MonthlyGrowthGram
                    : 0m;
                runningTotal += monthlyGrowth;

                var line = profile.Lines.FirstOrDefault(x => x.GrowthMonthNo == monthNo && !x.IsDeleted);
                if (line == null)
                {
                    line = new BudgetFishGrowthProfileLine
                    {
                        BudgetFishGrowthProfileId = profile.Id,
                        GrowthMonthNo = monthNo
                    };

                    await _unitOfWork.Repository<BudgetFishGrowthProfileLine>().AddAsync(line);
                    profile.Lines.Add(line);
                }

                line.CalendarMonth = CalculateCalendarMonth(profile.StartMonth, monthNo);
                line.MonthlyGrowthGram = monthlyGrowth;
                line.TotalGram = runningTotal;
            }

            return profile;
        }

        private static ApiResponse<ImportBudgetFishGrowthProfilesResultDto> ImportValidationError(
            string message,
            int statusCode = StatusCodes.Status400BadRequest)
        {
            return ApiResponse<ImportBudgetFishGrowthProfilesResultDto>.ErrorResult(
                message,
                message,
                statusCode);
        }

        private IQueryable<BudgetFishGrowthProfile> GetProfileQuery()
        {
            return _unitOfWork.Db.BudgetFishGrowthProfiles
                .AsNoTracking()
                .Include(x => x.Stock)
                .Include(x => x.Lines.Where(line => !line.IsDeleted));
        }

        private static BudgetFishGrowthProfileDto Map(BudgetFishGrowthProfile entity)
        {
            return new BudgetFishGrowthProfileDto
            {
                Id = entity.Id,
                StockId = entity.StockId,
                StockCode = entity.Stock?.ErpStockCode ?? string.Empty,
                StockName = entity.Stock?.StockName ?? string.Empty,
                StartMonth = entity.StartMonth,
                Name = entity.Name,
                Description = entity.Description,
                Lines = entity.Lines
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.GrowthMonthNo)
                    .Select(MapLine)
                    .ToList()
            };
        }

        private static BudgetFishGrowthProfileSummaryDto MapSummary(BudgetFishGrowthProfile entity)
        {
            var activeLines = entity.Lines.Where(x => !x.IsDeleted).ToList();
            return new BudgetFishGrowthProfileSummaryDto
            {
                Id = entity.Id,
                StockId = entity.StockId,
                StockCode = entity.Stock?.ErpStockCode ?? string.Empty,
                StockName = entity.Stock?.StockName ?? string.Empty,
                StartMonth = entity.StartMonth,
                Name = entity.Name,
                Description = entity.Description,
                LineCount = activeLines.Count,
                FinalTotalGram = activeLines.Count == 0 ? 0 : activeLines.MaxBy(x => x.GrowthMonthNo)?.TotalGram ?? 0
            };
        }

        private static BudgetFishGrowthProfileLineDto MapLine(BudgetFishGrowthProfileLine line)
        {
            return new BudgetFishGrowthProfileLineDto
            {
                Id = line.Id,
                BudgetFishGrowthProfileId = line.BudgetFishGrowthProfileId,
                GrowthMonthNo = line.GrowthMonthNo,
                CalendarMonth = line.CalendarMonth,
                MonthlyGrowthGram = line.MonthlyGrowthGram,
                TotalGram = line.TotalGram
            };
        }

        private static BudgetFishGrowthProfileDto CreateEmptyProfile(long stockId, int startMonth)
        {
            var lines = Enumerable.Range(1, MaxGrowthMonthNo)
                .Select(monthNo => new BudgetFishGrowthProfileLineDto
                {
                    GrowthMonthNo = monthNo,
                    CalendarMonth = CalculateCalendarMonth(startMonth, monthNo),
                    MonthlyGrowthGram = 0,
                    TotalGram = 0
                })
                .ToList();

            return new BudgetFishGrowthProfileDto
            {
                StockId = stockId,
                StartMonth = startMonth,
                Lines = lines
            };
        }

        private static int CalculateCalendarMonth(int startMonth, int growthMonthNo)
        {
            return ((startMonth - 1 + growthMonthNo - 1) % 12) + 1;
        }

        private static bool IsValidMonth(int month)
        {
            return month >= 1 && month <= 12;
        }

        private static string BuildProfileName(string? requestedName, string stockName, int startMonth)
        {
            if (!string.IsNullOrWhiteSpace(requestedName))
            {
                return requestedName.Trim();
            }

            return $"{stockName} - {GetTurkishMonthName(startMonth)} Başlangıç";
        }

        private static string? NormalizeDescription(string? description)
        {
            return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

        private static string GetTurkishMonthName(int month)
        {
            return month switch
            {
                1 => "Ocak",
                2 => "Şubat",
                3 => "Mart",
                4 => "Nisan",
                5 => "Mayıs",
                6 => "Haziran",
                7 => "Temmuz",
                8 => "Ağustos",
                9 => "Eylül",
                10 => "Ekim",
                11 => "Kasım",
                12 => "Aralık",
                _ => month.ToString()
            };
        }
    }
}
