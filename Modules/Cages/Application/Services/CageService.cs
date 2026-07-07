using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Cages.Application.Services
{
    public class CageService : ICageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public CageService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<CageDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Cages
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<CageDto>.ErrorResult(
                        _localizationService.GetLocalizedString("CageService.NotFound"),
                        _localizationService.GetLocalizedString("CageService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<CageDto>(entity);
                return ApiResponse<CageDto>.SuccessResult(dto, _localizationService.GetLocalizedString("CageService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<CageDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<CageDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Cages
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(Cage.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<CageDto>(x)).ToList();

                var pagedResponse = new PagedResponse<CageDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<CageDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("CageService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<CageDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("CageService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<CageDto>> CreateAsync(CreateCageDto dto)
        {
            try
            {
                var validation = await ValidateAsync(dto.CageCode, dto.CageName, null);
                if (!validation.Success)
                {
                    return validation;
                }

                dto.CageCode = dto.CageCode.Trim();
                dto.CageName = dto.CageName.Trim();

                var entity = _mapper.Map<Cage>(dto);
                await _unitOfWork.Cages.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<CageDto>(entity);
                return ApiResponse<CageDto>.SuccessResult(result, _localizationService.GetLocalizedString("CageService.OperationSuccessful"));
            }
            catch (DbUpdateException ex) when (DbUpdateExceptionHelper.TryGetUniqueViolation(ex, out _))
            {
                return DuplicateCageCode();
            }
            catch (Exception ex)
            {
                return ApiResponse<CageDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<CageDto>> UpdateAsync(long id, UpdateCageDto dto)
        {
            try
            {
                var repo = _unitOfWork.Cages;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<CageDto>.ErrorResult(
                        _localizationService.GetLocalizedString("CageService.NotFound"),
                        _localizationService.GetLocalizedString("CageService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var validation = await ValidateAsync(dto.CageCode, dto.CageName, id);
                if (!validation.Success)
                {
                    return validation;
                }

                dto.CageCode = dto.CageCode.Trim();
                dto.CageName = dto.CageName.Trim();

                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<CageDto>(entity);
                return ApiResponse<CageDto>.SuccessResult(result, _localizationService.GetLocalizedString("CageService.OperationSuccessful"));
            }
            catch (DbUpdateException ex) when (DbUpdateExceptionHelper.TryGetUniqueViolation(ex, out _))
            {
                return DuplicateCageCode();
            }
            catch (Exception ex)
            {
                return ApiResponse<CageDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.Cages;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("CageService.NotFound"),
                        _localizationService.GetLocalizedString("CageService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("CageService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("CageService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<ApiResponse<CageDto>> ValidateAsync(string cageCode, string cageName, long? currentId)
        {
            var normalizedCode = cageCode?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedCode))
            {
                return ApiResponse<CageDto>.ErrorResult(
                    _localizationService.GetLocalizedString("CageService.CageCodeRequired"),
                    _localizationService.GetLocalizedString("CageService.CageCodeRequired"),
                    StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(cageName))
            {
                var message = _localizationService.GetLocalizedString("CageService.CageNameRequired");
                return ApiResponse<CageDto>.ErrorResult(message, message, StatusCodes.Status400BadRequest);
            }

            var duplicateExists = await _unitOfWork.Cages
                .Query()
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.CageCode == normalizedCode &&
                    (!currentId.HasValue || x.Id != currentId.Value));

            return duplicateExists
                ? DuplicateCageCode()
                : ApiResponse<CageDto>.SuccessResult(new CageDto(), string.Empty);
        }

        private ApiResponse<CageDto> DuplicateCageCode()
        {
            var message = _localizationService.GetLocalizedString("CageService.CageCodeAlreadyExists");
            return ApiResponse<CageDto>.ErrorResult(message, message, StatusCodes.Status409Conflict);
        }
    }
}
