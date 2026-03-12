using AutoMapper;
using aqua_api.DTOs;
using aqua_api.Interfaces;
using aqua_api.Models;
using aqua_api.UnitOfWork;
using aqua_api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Services
{
    public class FishBatchService : IFishBatchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public FishBatchService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<FishBatchDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.FishBatches
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<FishBatchDto>.ErrorResult(
                        _localizationService.GetLocalizedString("FishBatchService.NotFound"),
                        _localizationService.GetLocalizedString("FishBatchService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<FishBatchDto>(entity);
                return ApiResponse<FishBatchDto>.SuccessResult(dto, _localizationService.GetLocalizedString("FishBatchService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<FishBatchDto>.ErrorResult(
                    _localizationService.GetLocalizedString("FishBatchService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<FishBatchDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.FishBatches
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(FishBatch.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<FishBatchDto>(x)).ToList();

                var pagedResponse = new PagedResponse<FishBatchDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<FishBatchDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("FishBatchService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<FishBatchDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("FishBatchService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<FishBatchDto>> CreateAsync(CreateFishBatchDto dto)
        {
            try
            {
                await NormalizeBatchCodeFromGoodsReceiptAsync(dto);
                var entity = _mapper.Map<FishBatch>(dto);
                await _unitOfWork.FishBatches.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<FishBatchDto>(entity);
                return ApiResponse<FishBatchDto>.SuccessResult(result, _localizationService.GetLocalizedString("FishBatchService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<FishBatchDto>.ErrorResult(
                    _localizationService.GetLocalizedString("FishBatchService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<FishBatchDto>> UpdateAsync(long id, UpdateFishBatchDto dto)
        {
            try
            {
                await NormalizeBatchCodeFromGoodsReceiptAsync(dto);
                var repo = _unitOfWork.FishBatches;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<FishBatchDto>.ErrorResult(
                        _localizationService.GetLocalizedString("FishBatchService.NotFound"),
                        _localizationService.GetLocalizedString("FishBatchService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<FishBatchDto>(entity);
                return ApiResponse<FishBatchDto>.SuccessResult(result, _localizationService.GetLocalizedString("FishBatchService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<FishBatchDto>.ErrorResult(
                    _localizationService.GetLocalizedString("FishBatchService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.FishBatches;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("FishBatchService.NotFound"),
                        _localizationService.GetLocalizedString("FishBatchService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("FishBatchService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("FishBatchService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task NormalizeBatchCodeFromGoodsReceiptAsync(CreateFishBatchDto dto)
        {
            if (!dto.SourceGoodsReceiptLineId.HasValue)
                return;

            var line = await _unitOfWork.GoodsReceiptLines
                .Query()
                .Where(x => !x.IsDeleted && x.Id == dto.SourceGoodsReceiptLineId.Value)
                .Select(x => new
                {
                    x.GoodsReceiptId,
                    ReceiptNo = x.GoodsReceipt != null ? x.GoodsReceipt.ReceiptNo : null
                })
                .FirstOrDefaultAsync();

            if (line == null || string.IsNullOrWhiteSpace(line.ReceiptNo))
                return;

            dto.BatchCode = line.ReceiptNo.Trim();
        }
    }
}
