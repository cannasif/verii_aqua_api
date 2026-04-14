using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class NetOperationTypeService : INetOperationTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public NetOperationTypeService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<NetOperationTypeDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.NetOperationTypes
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<NetOperationTypeDto>.ErrorResult(
                        _localizationService.GetLocalizedString("NetOperationTypeService.NotFound"),
                        _localizationService.GetLocalizedString("NetOperationTypeService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<NetOperationTypeDto>(entity);
                return ApiResponse<NetOperationTypeDto>.SuccessResult(dto, _localizationService.GetLocalizedString("NetOperationTypeService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<NetOperationTypeDto>.ErrorResult(
                    _localizationService.GetLocalizedString("NetOperationTypeService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<NetOperationTypeDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.NetOperationTypes
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(NetOperationType.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<NetOperationTypeDto>(x)).ToList();

                var pagedResponse = new PagedResponse<NetOperationTypeDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<NetOperationTypeDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("NetOperationTypeService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<NetOperationTypeDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("NetOperationTypeService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<NetOperationTypeDto>> CreateAsync(CreateNetOperationTypeDto dto)
        {
            try
            {
                var entity = _mapper.Map<NetOperationType>(dto);
                await _unitOfWork.NetOperationTypes.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<NetOperationTypeDto>(entity);
                return ApiResponse<NetOperationTypeDto>.SuccessResult(result, _localizationService.GetLocalizedString("NetOperationTypeService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<NetOperationTypeDto>.ErrorResult(
                    _localizationService.GetLocalizedString("NetOperationTypeService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<NetOperationTypeDto>> UpdateAsync(long id, UpdateNetOperationTypeDto dto)
        {
            try
            {
                var repo = _unitOfWork.NetOperationTypes;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<NetOperationTypeDto>.ErrorResult(
                        _localizationService.GetLocalizedString("NetOperationTypeService.NotFound"),
                        _localizationService.GetLocalizedString("NetOperationTypeService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<NetOperationTypeDto>(entity);
                return ApiResponse<NetOperationTypeDto>.SuccessResult(result, _localizationService.GetLocalizedString("NetOperationTypeService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<NetOperationTypeDto>.ErrorResult(
                    _localizationService.GetLocalizedString("NetOperationTypeService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.NetOperationTypes;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("NetOperationTypeService.NotFound"),
                        _localizationService.GetLocalizedString("NetOperationTypeService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("NetOperationTypeService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("NetOperationTypeService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
