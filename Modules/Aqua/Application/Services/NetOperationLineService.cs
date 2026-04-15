using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class NetOperationLineService : INetOperationLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INetOperationService _netOperationService;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public NetOperationLineService(
            IUnitOfWork unitOfWork,
            INetOperationService netOperationService,
            IMapper mapper,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _netOperationService = netOperationService;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<NetOperationLineDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.NetOperationLines
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<NetOperationLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("NetOperationLineService.NotFound"),
                        _localizationService.GetLocalizedString("NetOperationLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<NetOperationLineDto>(entity);
                return ApiResponse<NetOperationLineDto>.SuccessResult(dto, _localizationService.GetLocalizedString("NetOperationLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<NetOperationLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("NetOperationLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<NetOperationLineDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.NetOperationLines
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(NetOperationLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<NetOperationLineDto>(x)).ToList();

                var pagedResponse = new PagedResponse<NetOperationLineDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<NetOperationLineDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("NetOperationLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<NetOperationLineDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("NetOperationLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<NetOperationLineDto>> CreateAsync(CreateNetOperationLineDto dto)
        {
            try
            {
                var entity = _mapper.Map<NetOperationLine>(dto);
                await _unitOfWork.NetOperationLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var netOperation = await _unitOfWork.NetOperations
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == entity.NetOperationId && !x.IsDeleted);
                if (netOperation != null && netOperation.Status == DocumentStatus.Draft)
                {
                    var userId = entity.CreatedBy ?? netOperation.CreatedBy ?? 1L;
                    var postResult = await _netOperationService.Post(netOperation.Id, userId);
                    if (!postResult.Success)
                    {
                        return ApiResponse<NetOperationLineDto>.ErrorResult(
                            postResult.Message,
                            postResult.ExceptionMessage,
                            postResult.StatusCode);
                    }
                }

                var result = _mapper.Map<NetOperationLineDto>(entity);
                return ApiResponse<NetOperationLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("NetOperationLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<NetOperationLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("NetOperationLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<NetOperationLineDto>> CreateWithAutoHeaderAsync(CreateNetOperationLineWithAutoHeaderDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var netOperation = await _unitOfWork.NetOperations
                    .Query()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProjectId == dto.ProjectId &&
                        x.Status == DocumentStatus.Draft &&
                        x.OperationDate.Date == dto.OperationDate.Date &&
                        x.OperationTypeId == dto.OperationTypeId)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (netOperation == null)
                {
                    netOperation = new NetOperation
                    {
                        ProjectId = dto.ProjectId,
                        OperationTypeId = dto.OperationTypeId,
                        OperationDate = dto.OperationDate.Date,
                        OperationNo = BuildDocumentNo(dto.ProjectId, dto.OperationDate),
                        Status = DocumentStatus.Draft,
                        Note = dto.Note,
                    };

                    await _unitOfWork.NetOperations.AddAsync(netOperation);
                    await _unitOfWork.SaveChangesAsync();
                }

                var entity = new NetOperationLine
                {
                    NetOperationId = netOperation.Id,
                    ProjectCageId = dto.ProjectCageId,
                    FishBatchId = dto.FishBatchId,
                    Note = dto.Note,
                };

                await _unitOfWork.NetOperationLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                if (netOperation.Status == DocumentStatus.Draft)
                {
                    var userId = entity.CreatedBy ?? netOperation.CreatedBy ?? 1L;
                    var postResult = await _netOperationService.Post(netOperation.Id, userId);
                    if (!postResult.Success)
                    {
                        return ApiResponse<NetOperationLineDto>.ErrorResult(
                            postResult.Message,
                            postResult.ExceptionMessage,
                            postResult.StatusCode);
                    }
                }

                var result = _mapper.Map<NetOperationLineDto>(entity);
                return ApiResponse<NetOperationLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("NetOperationLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<NetOperationLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("NetOperationLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<NetOperationLineDto>> UpdateAsync(long id, UpdateNetOperationLineDto dto)
        {
            try
            {
                var repo = _unitOfWork.NetOperationLines;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<NetOperationLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("NetOperationLineService.NotFound"),
                        _localizationService.GetLocalizedString("NetOperationLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<NetOperationLineDto>(entity);
                return ApiResponse<NetOperationLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("NetOperationLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<NetOperationLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("NetOperationLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.NetOperationLines;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("NetOperationLineService.NotFound"),
                        _localizationService.GetLocalizedString("NetOperationLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("NetOperationLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("NetOperationLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static string BuildDocumentNo(long projectId, DateTime operationDate)
        {
            return $"NO-{projectId}-{operationDate:yyyyMMdd}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
    }
}
