using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class BatchCageBalanceService : IBatchCageBalanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public BatchCageBalanceService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<BatchCageBalanceDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.BatchCageBalances
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<BatchCageBalanceDto>.ErrorResult(
                        _localizationService.GetLocalizedString("BatchCageBalanceService.NotFound"),
                        _localizationService.GetLocalizedString("BatchCageBalanceService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<BatchCageBalanceDto>(entity);
                return ApiResponse<BatchCageBalanceDto>.SuccessResult(dto, _localizationService.GetLocalizedString("BatchCageBalanceService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchCageBalanceDto>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchCageBalanceService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<BatchCageBalanceDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.BatchCageBalances
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(BatchCageBalance.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<BatchCageBalanceDto>(x)).ToList();

                var pagedResponse = new PagedResponse<BatchCageBalanceDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<BatchCageBalanceDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("BatchCageBalanceService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<BatchCageBalanceDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchCageBalanceService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BatchCageBalanceDto>> CreateAsync(CreateBatchCageBalanceDto dto)
        {
            try
            {
                var entity = _mapper.Map<BatchCageBalance>(dto);
                await _unitOfWork.BatchCageBalances.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<BatchCageBalanceDto>(entity);
                return ApiResponse<BatchCageBalanceDto>.SuccessResult(result, _localizationService.GetLocalizedString("BatchCageBalanceService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchCageBalanceDto>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchCageBalanceService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BatchCageBalanceDto>> UpdateAsync(long id, UpdateBatchCageBalanceDto dto)
        {
            try
            {
                var repo = _unitOfWork.BatchCageBalances;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<BatchCageBalanceDto>.ErrorResult(
                        _localizationService.GetLocalizedString("BatchCageBalanceService.NotFound"),
                        _localizationService.GetLocalizedString("BatchCageBalanceService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<BatchCageBalanceDto>(entity);
                return ApiResponse<BatchCageBalanceDto>.SuccessResult(result, _localizationService.GetLocalizedString("BatchCageBalanceService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchCageBalanceDto>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchCageBalanceService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.BatchCageBalances;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("BatchCageBalanceService.NotFound"),
                        _localizationService.GetLocalizedString("BatchCageBalanceService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("BatchCageBalanceService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchCageBalanceService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
