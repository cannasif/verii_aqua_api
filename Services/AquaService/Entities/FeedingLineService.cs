using AutoMapper;
using aqua_api.DTOs;
using aqua_api.Interfaces;
using aqua_api.Models;
using aqua_api.UnitOfWork;
using aqua_api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Services
{
    public class FeedingLineService : IFeedingLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public FeedingLineService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<FeedingLineDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.FeedingLines
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<FeedingLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingLineService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<FeedingLineDto>(entity);
                return ApiResponse<FeedingLineDto>.SuccessResult(dto, _localizationService.GetLocalizedString("FeedingLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedingLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<FeedingLineDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.FeedingLines
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(FeedingLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<FeedingLineDto>(x)).ToList();

                var pagedResponse = new PagedResponse<FeedingLineDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<FeedingLineDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("FeedingLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<FeedingLineDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<FeedingLineDto>> CreateAsync(CreateFeedingLineDto dto)
        {
            try
            {
                if (dto.QtyUnit <= 0)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("FeedingLineService.QuantityMustBeGreaterThanZero"));
                }

                dto.FeedingId = await EnsureFeedingIdAsync(dto);

                if (dto.GramPerUnit <= 0)
                {
                    dto.GramPerUnit = dto.TotalGram > 0
                        ? Math.Round(dto.TotalGram / dto.QtyUnit, 3, MidpointRounding.AwayFromZero)
                        : 1;
                }

                if (dto.TotalGram <= 0)
                {
                    dto.TotalGram = Math.Round(dto.QtyUnit * dto.GramPerUnit, 3, MidpointRounding.AwayFromZero);
                }

                var entity = _mapper.Map<FeedingLine>(dto);
                await _unitOfWork.FeedingLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<FeedingLineDto>(entity);
                return ApiResponse<FeedingLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("FeedingLineService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<FeedingLineDto>.ErrorResult(
                    ex.Message,
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (DbUpdateException ex)
            {
                var businessMessage = MapDbError(ex);
                return ApiResponse<FeedingLineDto>.ErrorResult(
                    businessMessage,
                    businessMessage,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedingLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<FeedingLineDto>> UpdateAsync(long id, UpdateFeedingLineDto dto)
        {
            try
            {
                var repo = _unitOfWork.FeedingLines;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<FeedingLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingLineService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<FeedingLineDto>(entity);
                return ApiResponse<FeedingLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("FeedingLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedingLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.FeedingLines;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("FeedingLineService.NotFound"),
                        _localizationService.GetLocalizedString("FeedingLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("FeedingLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("FeedingLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<long> EnsureFeedingIdAsync(CreateFeedingLineDto dto)
        {
            if (dto.FeedingId > 0)
            {
                var current = await _unitOfWork.Feedings
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == dto.FeedingId && !x.IsDeleted);

                if (current != null)
                {
                    return current.Id;
                }
            }

            if (!dto.ProjectId.HasValue || dto.ProjectId.Value <= 0)
            {
                throw new InvalidOperationException(_localizationService.GetLocalizedString("FeedingLineService.HeaderNotFoundRetryWithProject"));
            }

            var targetDate = (dto.FeedingDate ?? DateTime.UtcNow).Date;

            var existingHeader = await _unitOfWork.Feedings
                .Query()
                .Where(x => !x.IsDeleted
                    && x.ProjectId == dto.ProjectId.Value
                    && x.FeedingDate.Date == targetDate
                    && x.Status != DocumentStatus.Cancelled)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync();

            if (existingHeader != null)
            {
                return existingHeader.Id;
            }

            var generatedFeedingNo = string.IsNullOrWhiteSpace(dto.FeedingNo)
                ? $"FD-{dto.ProjectId.Value}-{targetDate:yyyyMMdd}-{Guid.NewGuid():N}"[..32]
                : dto.FeedingNo.Trim();

            var newHeader = new Feeding
            {
                ProjectId = dto.ProjectId.Value,
                FeedingNo = generatedFeedingNo,
                FeedingDate = targetDate,
                FeedingSlot = dto.FeedingSlot ?? FeedingSlot.Morning,
                SourceType = dto.SourceType ?? FeedingSourceType.Manual,
                Status = dto.Status ?? DocumentStatus.Posted,
                Note = dto.Note
            };

            await _unitOfWork.Feedings.AddAsync(newHeader);
            await _unitOfWork.SaveChangesAsync();

            return newHeader.Id;
        }

        private string MapDbError(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            if (message.Contains("CK_RII_FeedingLine_Positive", StringComparison.OrdinalIgnoreCase))
            {
                return _localizationService.GetLocalizedString("FeedingLineService.PositiveValuesRequired");
            }

            if (message.Contains("FK_RII_FeedingLine_Feeding", StringComparison.OrdinalIgnoreCase))
            {
                return _localizationService.GetLocalizedString("FeedingLineService.HeaderCreateRetryForToday");
            }

            if (message.Contains("FK_RII_FeedingLine_Stock", StringComparison.OrdinalIgnoreCase))
            {
                return _localizationService.GetLocalizedString("FeedingLineService.InvalidStockSelection");
            }

            return _localizationService.GetLocalizedString("FeedingLineService.SaveFailed");
        }
    }
}
