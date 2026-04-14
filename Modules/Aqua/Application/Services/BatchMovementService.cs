using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class BatchMovementService : IBatchMovementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public BatchMovementService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<BatchMovementDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.BatchMovements
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<BatchMovementDto>.ErrorResult(
                        _localizationService.GetLocalizedString("BatchMovementService.NotFound"),
                        _localizationService.GetLocalizedString("BatchMovementService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<BatchMovementDto>(entity);
                return ApiResponse<BatchMovementDto>.SuccessResult(dto, _localizationService.GetLocalizedString("BatchMovementService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchMovementDto>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchMovementService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<BatchMovementDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.BatchMovements
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(BatchMovement.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<BatchMovementDto>(x)).ToList();

                var pagedResponse = new PagedResponse<BatchMovementDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<BatchMovementDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("BatchMovementService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<BatchMovementDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchMovementService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BatchMovementDto>> CreateAsync(CreateBatchMovementDto dto)
        {
            try
            {
                var entity = _mapper.Map<BatchMovement>(dto);
                await _unitOfWork.BatchMovements.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<BatchMovementDto>(entity);
                return ApiResponse<BatchMovementDto>.SuccessResult(result, _localizationService.GetLocalizedString("BatchMovementService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchMovementDto>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchMovementService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<BatchMovementDto>> UpdateAsync(long id, UpdateBatchMovementDto dto)
        {
            try
            {
                var repo = _unitOfWork.BatchMovements;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<BatchMovementDto>.ErrorResult(
                        _localizationService.GetLocalizedString("BatchMovementService.NotFound"),
                        _localizationService.GetLocalizedString("BatchMovementService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<BatchMovementDto>(entity);
                return ApiResponse<BatchMovementDto>.SuccessResult(result, _localizationService.GetLocalizedString("BatchMovementService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchMovementDto>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchMovementService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.BatchMovements;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("BatchMovementService.NotFound"),
                        _localizationService.GetLocalizedString("BatchMovementService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("BatchMovementService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("BatchMovementService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
