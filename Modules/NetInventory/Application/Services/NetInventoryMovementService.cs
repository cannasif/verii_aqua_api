using aqua_api.Modules.NetInventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.NetInventory.Application.Services;

public class NetInventoryMovementService : INetInventoryMovementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;

    public NetInventoryMovementService(IUnitOfWork unitOfWork, ILocalizationService localizationService)
    {
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<PagedResponse<NetInventoryMovementDto>>> GetAllAsync(PagedRequest request)
    {
        try
        {
            request ??= new PagedRequest();
            request.Filters ??= new List<Filter>();

            var query = BuildQuery()
                .ApplyFilters(request.Filters, request.FilterLogic);

            var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(NetInventoryMovement.MovementDate) : request.SortBy;
            query = query.ApplySorting(sortBy, request.SortDirection);

            var totalCount = await query.CountAsync();
            var entities = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync();

            var response = new PagedResponse<NetInventoryMovementDto>
            {
                Items = entities.Select(Map).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return ApiResponse<PagedResponse<NetInventoryMovementDto>>.SuccessResult(
                response,
                L("NetInventoryMovementService.OperationSuccessful"));
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResponse<NetInventoryMovementDto>>.ErrorResult(
                L("NetInventoryMovementService.InternalServerError"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<ApiResponse<NetInventoryMovementDto>> GetByIdAsync(long id)
    {
        try
        {
            var entity = await BuildQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return ApiResponse<NetInventoryMovementDto>.ErrorResult(
                    L("NetInventoryMovementService.NotFound"),
                    L("NetInventoryMovementService.NotFound"),
                    StatusCodes.Status404NotFound);
            }

            return ApiResponse<NetInventoryMovementDto>.SuccessResult(Map(entity), L("NetInventoryMovementService.OperationSuccessful"));
        }
        catch (Exception ex)
        {
            return ApiResponse<NetInventoryMovementDto>.ErrorResult(
                L("NetInventoryMovementService.InternalServerError"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<ApiResponse<NetInventoryMovementDto>> CreateAsync(CreateNetInventoryMovementDto dto)
    {
        try
        {
            var validation = await ValidateAsync(dto);
            if (!validation.Success)
            {
                return validation;
            }

            var entity = new NetInventoryMovement
            {
                MovementNo = string.IsNullOrWhiteSpace(dto.MovementNo)
                    ? await BuildMovementNoAsync(dto.MovementDate)
                    : dto.MovementNo.Trim(),
                NetType = dto.NetType,
                MovementType = dto.MovementType,
                MovementDate = dto.MovementDate.Date,
                StockId = dto.StockId,
                ProjectId = dto.ProjectId,
                SourceWarehouseId = dto.SourceWarehouseId,
                TargetWarehouseId = dto.TargetWarehouseId,
                SourceProjectCageId = dto.SourceProjectCageId,
                TargetProjectCageId = dto.TargetProjectCageId,
                Quantity = dto.Quantity,
                Note = NormalizeNote(dto.Note)
            };

            await _unitOfWork.Repository<NetInventoryMovement>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var created = await BuildQuery().FirstAsync(x => x.Id == entity.Id);
            return ApiResponse<NetInventoryMovementDto>.SuccessResult(Map(created), L("NetInventoryMovementService.OperationSuccessful"));
        }
        catch (Exception ex)
        {
            return ApiResponse<NetInventoryMovementDto>.ErrorResult(
                L("NetInventoryMovementService.CreateFailed"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<ApiResponse<NetInventoryMovementDto>> UpdateAsync(long id, UpdateNetInventoryMovementDto dto)
    {
        try
        {
            var entity = await _unitOfWork.Db.NetInventoryMovements.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (entity == null)
            {
                return ApiResponse<NetInventoryMovementDto>.ErrorResult(
                    L("NetInventoryMovementService.NotFound"),
                    L("NetInventoryMovementService.NotFound"),
                    StatusCodes.Status404NotFound);
            }

            var validation = await ValidateAsync(dto, id);
            if (!validation.Success)
            {
                return validation;
            }

            entity.MovementNo = string.IsNullOrWhiteSpace(dto.MovementNo) ? entity.MovementNo : dto.MovementNo.Trim();
            entity.NetType = dto.NetType;
            entity.MovementType = dto.MovementType;
            entity.MovementDate = dto.MovementDate.Date;
            entity.StockId = dto.StockId;
            entity.ProjectId = dto.ProjectId;
            entity.SourceWarehouseId = dto.SourceWarehouseId;
            entity.TargetWarehouseId = dto.TargetWarehouseId;
            entity.SourceProjectCageId = dto.SourceProjectCageId;
            entity.TargetProjectCageId = dto.TargetProjectCageId;
            entity.Quantity = dto.Quantity;
            entity.Note = NormalizeNote(dto.Note);

            await _unitOfWork.Repository<NetInventoryMovement>().UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var updated = await BuildQuery().FirstAsync(x => x.Id == entity.Id);
            return ApiResponse<NetInventoryMovementDto>.SuccessResult(Map(updated), L("NetInventoryMovementService.OperationSuccessful"));
        }
        catch (Exception ex)
        {
            return ApiResponse<NetInventoryMovementDto>.ErrorResult(
                L("NetInventoryMovementService.UpdateFailed"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
    {
        try
        {
            var deleted = await _unitOfWork.Repository<NetInventoryMovement>().SoftDeleteAsync(id);
            if (!deleted)
            {
                return ApiResponse<bool>.ErrorResult(
                    L("NetInventoryMovementService.NotFound"),
                    L("NetInventoryMovementService.NotFound"),
                    StatusCodes.Status404NotFound);
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResult(true, L("NetInventoryMovementService.OperationSuccessful"));
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResult(
                L("NetInventoryMovementService.DeleteFailed"),
                ex.Message,
                StatusCodes.Status500InternalServerError);
        }
    }

    private IQueryable<NetInventoryMovement> BuildQuery()
    {
        return _unitOfWork.Db.NetInventoryMovements
            .AsNoTracking()
            .Include(x => x.Stock)
            .Include(x => x.Project)
            .Include(x => x.SourceWarehouse)
            .Include(x => x.TargetWarehouse)
            .Include(x => x.SourceProjectCage)
                .ThenInclude(x => x!.Cage)
            .Include(x => x.TargetProjectCage)
                .ThenInclude(x => x!.Cage)
            .Where(x => !x.IsDeleted);
    }

    private async Task<ApiResponse<NetInventoryMovementDto>> ValidateAsync(CreateNetInventoryMovementDto dto, long? currentId = null)
    {
        if (!Enum.IsDefined(typeof(NetType), dto.NetType))
        {
            return ValidationError("NetInventoryMovementService.InvalidNetType");
        }

        if (!Enum.IsDefined(typeof(NetInventoryMovementType), dto.MovementType))
        {
            return ValidationError("NetInventoryMovementService.InvalidMovementType");
        }

        if (dto.Quantity <= 0)
        {
            return ValidationError("NetInventoryMovementService.QuantityRequired");
        }

        if (dto.MovementDate.Year < 1900)
        {
            return ValidationError("NetInventoryMovementService.MovementDateRequired");
        }

        var movementNo = dto.MovementNo?.Trim();
        if (!string.IsNullOrWhiteSpace(movementNo))
        {
            if (movementNo.Length > 50)
            {
                return ValidationError("NetInventoryMovementService.MovementNoTooLong");
            }

            var exists = await _unitOfWork.Db.NetInventoryMovements.AnyAsync(x =>
                x.MovementNo == movementNo &&
                !x.IsDeleted &&
                (!currentId.HasValue || x.Id != currentId.Value));

            if (exists)
            {
                return ValidationError("NetInventoryMovementService.MovementNoAlreadyExists");
            }
        }

        if (dto.MovementType == NetInventoryMovementType.CagePlacement && !dto.TargetProjectCageId.HasValue)
        {
            return ValidationError("NetInventoryMovementService.TargetCageRequired");
        }

        if (dto.MovementType == NetInventoryMovementType.CageRemoval && !dto.SourceProjectCageId.HasValue)
        {
            return ValidationError("NetInventoryMovementService.SourceCageRequired");
        }

        if (dto.MovementType == NetInventoryMovementType.WarehouseTransfer &&
            (!dto.SourceWarehouseId.HasValue || !dto.TargetWarehouseId.HasValue))
        {
            return ValidationError("NetInventoryMovementService.WarehouseTransferWarehousesRequired");
        }

        if (!string.IsNullOrWhiteSpace(dto.Note) && dto.Note.Trim().Length > 500)
        {
            return ValidationError("NetInventoryMovementService.NoteTooLong");
        }

        return ApiResponse<NetInventoryMovementDto>.SuccessResult(new NetInventoryMovementDto(), "Valid");
    }

    private async Task<string> BuildMovementNoAsync(DateTime movementDate)
    {
        var prefix = $"NET-{movementDate:yyyyMMdd}";
        var count = await _unitOfWork.Db.NetInventoryMovements
            .IgnoreQueryFilters()
            .CountAsync(x => x.MovementNo.StartsWith(prefix));
        return $"{prefix}-{count + 1:0000}";
    }

    private ApiResponse<NetInventoryMovementDto> ValidationError(string key)
    {
        var message = L(key);
        return ApiResponse<NetInventoryMovementDto>.ErrorResult(message, message, StatusCodes.Status400BadRequest);
    }

    private NetInventoryMovementDto Map(NetInventoryMovement entity)
    {
        return new NetInventoryMovementDto
        {
            Id = entity.Id,
            MovementNo = entity.MovementNo,
            NetType = entity.NetType,
            NetTypeName = L($"NetInventory.NetType.{(int)entity.NetType}"),
            MovementType = entity.MovementType,
            MovementTypeName = L($"NetInventory.MovementType.{(int)entity.MovementType}"),
            MovementDate = entity.MovementDate,
            StockId = entity.StockId,
            StockCode = entity.Stock?.ErpStockCode,
            StockName = entity.Stock?.StockName,
            ProjectId = entity.ProjectId,
            ProjectCode = entity.Project?.ProjectCode,
            ProjectName = entity.Project?.ProjectName,
            SourceWarehouseId = entity.SourceWarehouseId,
            SourceWarehouseCode = entity.SourceWarehouse?.ErpWarehouseCode.ToString(),
            SourceWarehouseName = entity.SourceWarehouse?.WarehouseName,
            TargetWarehouseId = entity.TargetWarehouseId,
            TargetWarehouseCode = entity.TargetWarehouse?.ErpWarehouseCode.ToString(),
            TargetWarehouseName = entity.TargetWarehouse?.WarehouseName,
            SourceProjectCageId = entity.SourceProjectCageId,
            SourceCageCode = entity.SourceProjectCage?.Cage?.CageCode,
            SourceCageName = entity.SourceProjectCage?.Cage?.CageName,
            TargetProjectCageId = entity.TargetProjectCageId,
            TargetCageCode = entity.TargetProjectCage?.Cage?.CageCode,
            TargetCageName = entity.TargetProjectCage?.Cage?.CageName,
            Quantity = entity.Quantity,
            Note = entity.Note
        };
    }

    private static string? NormalizeNote(string? note)
    {
        var normalized = note?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private string L(string key) => _localizationService.GetLocalizedString(key);
}
