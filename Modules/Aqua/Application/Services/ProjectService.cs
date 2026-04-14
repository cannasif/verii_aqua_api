using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public ProjectService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<ProjectDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _unitOfWork.Projects
                    .Query()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<ProjectDto>.ErrorResult(
                        _localizationService.GetLocalizedString("ProjectService.NotFound"),
                        _localizationService.GetLocalizedString("ProjectService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<ProjectDto>(entity);
                return ApiResponse<ProjectDto>.SuccessResult(dto, _localizationService.GetLocalizedString("ProjectService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<ProjectDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ProjectService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<ProjectDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = _unitOfWork.Projects
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(Project.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();

                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var items = entities.Select(x => _mapper.Map<ProjectDto>(x)).ToList();

                var pagedResponse = new PagedResponse<ProjectDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<ProjectDto>>.SuccessResult(
                    pagedResponse,
                    _localizationService.GetLocalizedString("ProjectService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<ProjectDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ProjectService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<ProjectDto>> CreateAsync(CreateProjectDto dto)
        {
            try
            {
                var normalizedCode = dto.ProjectCode?.Trim() ?? string.Empty;
                var normalizedName = dto.ProjectName?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(normalizedCode))
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectService.ProjectCodeRequired"));
                }

                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectService.ProjectNameRequired"));
                }

                var existingCode = await _unitOfWork.Projects
                    .Query()
                    .AnyAsync(x => !x.IsDeleted && x.ProjectCode == normalizedCode);
                if (existingCode)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectService.ProjectCodeAlreadyExists"));
                }

                var existingName = await _unitOfWork.Projects
                    .Query()
                    .AnyAsync(x => !x.IsDeleted && x.ProjectName == normalizedName);
                if (existingName)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectService.ProjectNameAlreadyExists"));
                }

                dto.ProjectCode = normalizedCode;
                dto.ProjectName = normalizedName;
                var entity = _mapper.Map<Project>(dto);
                await _unitOfWork.Projects.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<ProjectDto>(entity);
                return ApiResponse<ProjectDto>.SuccessResult(result, _localizationService.GetLocalizedString("ProjectService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<ProjectDto>.ErrorResult(
                    ex.Message,
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (DbUpdateException ex) when (IsKnownProjectConstraintError(ex, out var message))
            {
                return ApiResponse<ProjectDto>.ErrorResult(
                    message,
                    message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return ApiResponse<ProjectDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ProjectService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<ProjectDto>> UpdateAsync(long id, UpdateProjectDto dto)
        {
            try
            {
                var repo = _unitOfWork.Projects;
                var entity = await repo.GetByIdForUpdateAsync(id);

                if (entity == null)
                {
                    return ApiResponse<ProjectDto>.ErrorResult(
                        _localizationService.GetLocalizedString("ProjectService.NotFound"),
                        _localizationService.GetLocalizedString("ProjectService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                var normalizedCode = dto.ProjectCode?.Trim() ?? string.Empty;
                var normalizedName = dto.ProjectName?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(normalizedCode))
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectService.ProjectCodeRequired"));
                }

                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectService.ProjectNameRequired"));
                }

                var existingCode = await _unitOfWork.Projects
                    .Query()
                    .AnyAsync(x => !x.IsDeleted && x.Id != id && x.ProjectCode == normalizedCode);
                if (existingCode)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectService.ProjectCodeAlreadyExists"));
                }

                var existingName = await _unitOfWork.Projects
                    .Query()
                    .AnyAsync(x => !x.IsDeleted && x.Id != id && x.ProjectName == normalizedName);
                if (existingName)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectService.ProjectNameAlreadyExists"));
                }

                dto.ProjectCode = normalizedCode;
                dto.ProjectName = normalizedName;
                _mapper.Map(dto, entity);
                await repo.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<ProjectDto>(entity);
                return ApiResponse<ProjectDto>.SuccessResult(result, _localizationService.GetLocalizedString("ProjectService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<ProjectDto>.ErrorResult(
                    ex.Message,
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (DbUpdateException ex) when (IsKnownProjectConstraintError(ex, out var message))
            {
                return ApiResponse<ProjectDto>.ErrorResult(
                    message,
                    message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return ApiResponse<ProjectDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ProjectService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(long id)
        {
            try
            {
                var repo = _unitOfWork.Projects;
                var isDeleted = await repo.SoftDeleteAsync(id);

                if (!isDeleted)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("ProjectService.NotFound"),
                        _localizationService.GetLocalizedString("ProjectService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("ProjectService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("ProjectService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private bool IsKnownProjectConstraintError(DbUpdateException ex, out string message)
        {
            var allMessages = ex.ToString();

            if (allMessages.Contains("UX_RII_Project_ProjectCode_Active", StringComparison.OrdinalIgnoreCase))
            {
                message = _localizationService.GetLocalizedString("ProjectService.ProjectCodeAlreadyExists");
                return true;
            }

            message = string.Empty;
            return false;
        }
    }
}
