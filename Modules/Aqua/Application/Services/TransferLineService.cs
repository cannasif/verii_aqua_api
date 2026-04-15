using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class TransferLineService : ITransferLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransferService _transferService;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public TransferLineService(
            IUnitOfWork unitOfWork,
            ITransferService transferService,
            IMapper mapper,
            ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _transferService = transferService;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<TransferLineDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.TransferLines
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<TransferLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<TransferLineDto>(entity);
                return ApiResponse<TransferLineDto>.SuccessResult(dto, _localizationService.GetLocalizedString("TransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<TransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<TransferLineDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.TransferLines
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(TransferLine.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<TransferLineDto>(x)).ToList();

                var pagedResponse = new PagedResponse<TransferLineDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<TransferLineDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("TransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<TransferLineDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<TransferLineDto>> CreateAsync(CreateTransferLineDto dto)
        {
            try
            {
                var entity = _mapper.Map<TransferLine>(dto);
                await _unitOfWork.TransferLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var transfer = await _unitOfWork.Transfers
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == entity.TransferId && !x.IsDeleted);
                if (transfer != null && transfer.Status == DocumentStatus.Draft)
                {
                    var userId = entity.CreatedBy ?? transfer.CreatedBy ?? 1L;
                    var postResult = await _transferService.Post(transfer.Id, userId);
                    if (!postResult.Success)
                    {
                        return ApiResponse<TransferLineDto>.ErrorResult(
                            postResult.Message,
                            postResult.ExceptionMessage,
                            postResult.StatusCode);
                    }
                }

                var result = _mapper.Map<TransferLineDto>(entity);
                return ApiResponse<TransferLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("TransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<TransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<TransferLineDto>> CreateWithAutoHeaderAsync(CreateTransferLineWithAutoHeaderDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var transfer = await _unitOfWork.Transfers
                    .Query()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProjectId == dto.ProjectId &&
                        x.Status == DocumentStatus.Draft &&
                        x.TransferDate.Date == dto.TransferDate.Date)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (transfer == null)
                {
                    var project = await _unitOfWork.Projects
                        .Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == dto.ProjectId && !x.IsDeleted);

                    if (project == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ApiResponse<TransferLineDto>.ErrorResult(
                            _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                            "Project not found.",
                            StatusCodes.Status404NotFound);
                    }

                    transfer = new Transfer
                    {
                        ProjectId = dto.ProjectId,
                        TransferDate = dto.TransferDate,
                        Status = DocumentStatus.Draft,
                        TransferNo = BuildDocumentNo(project.ProjectCode, project.ProjectName),
                    };

                    await _unitOfWork.Transfers.AddAsync(transfer);
                    await _unitOfWork.SaveChangesAsync();
                }

                var entity = new TransferLine
                {
                    TransferId = transfer.Id,
                    FishBatchId = dto.FishBatchId,
                    FromProjectCageId = dto.FromProjectCageId,
                    ToProjectCageId = dto.ToProjectCageId,
                    FishCount = dto.FishCount,
                    AverageGram = dto.AverageGram,
                    BiomassGram = dto.BiomassGram,
                };

                await _unitOfWork.TransferLines.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                if (transfer.Status == DocumentStatus.Draft)
                {
                    var userId = entity.CreatedBy ?? transfer.CreatedBy ?? 1L;
                    var postResult = await _transferService.Post(transfer.Id, userId);
                    if (!postResult.Success)
                    {
                        return ApiResponse<TransferLineDto>.ErrorResult(
                            postResult.Message,
                            postResult.ExceptionMessage,
                            postResult.StatusCode);
                    }
                }

                var result = _mapper.Map<TransferLineDto>(entity);
                return ApiResponse<TransferLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("TransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<TransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<TransferLineDto>> UpdateAsync(long id, UpdateTransferLineDto dto)
        {
            try
            {
                var repo = _unitOfWork.TransferLines;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<TransferLineDto>.ErrorResult(
                        _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<TransferLineDto>(entity);
                return ApiResponse<TransferLineDto>.SuccessResult(result, _localizationService.GetLocalizedString("TransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<TransferLineDto>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.TransferLines;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                        _localizationService.GetLocalizedString("TransferLineService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("TransferLineService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("TransferLineService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static string BuildDocumentNo(string? projectCode, string? projectName)
        {
            var baseValue = !string.IsNullOrWhiteSpace(projectCode) ? projectCode : projectName;
            var normalized = string.IsNullOrWhiteSpace(baseValue) ? "DOC" : baseValue.Trim();
            return $"{normalized}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
    }
}
