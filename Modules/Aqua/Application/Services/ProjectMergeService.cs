using AutoMapper;
using aqua_api.Shared.Infrastructure.Time;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class ProjectMergeService : IProjectMergeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public ProjectMergeService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<ProjectMergeDto>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await QueryProjectMerges()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    return ApiResponse<ProjectMergeDto>.ErrorResult(
                        _localizationService.GetLocalizedString("ProjectMergeService.NotFound"),
                        _localizationService.GetLocalizedString("ProjectMergeService.NotFound"),
                        StatusCodes.Status404NotFound);
                }

                return ApiResponse<ProjectMergeDto>.SuccessResult(
                    MapDto(entity),
                    _localizationService.GetLocalizedString("ProjectMergeService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<ProjectMergeDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ProjectMergeService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<PagedResponse<ProjectMergeDto>>> GetAllAsync(PagedRequest request)
        {
            try
            {
                request ??= new PagedRequest();
                request.Filters ??= new List<Filter>();

                var query = QueryProjectMerges()
                    .Where(x => !x.IsDeleted)
                    .ApplyFilters(request.Filters, request.FilterLogic);

                var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(ProjectMerge.Id) : request.SortBy;
                query = query.ApplySorting(sortBy, request.SortDirection);

                var totalCount = await query.CountAsync();
                var entities = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var response = new PagedResponse<ProjectMergeDto>
                {
                    Items = entities.Select(MapDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<ProjectMergeDto>>.SuccessResult(
                    response,
                    _localizationService.GetLocalizedString("ProjectMergeService.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<ProjectMergeDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("ProjectMergeService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<ProjectMergeDto>> CreateAsync(CreateProjectMergeDto dto, long userId)
        {
            try
            {
                await _unitOfWork.BeginTransaction();

                var settings = await _unitOfWork.AquaSettings
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.Id)
                    .FirstOrDefaultAsync();

                if (!(settings?.AllowProjectMerge ?? false))
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectMergeService.MergeDisabled"));
                }

                var sourceProjectIds = dto.SourceProjectIds
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                if (dto.TargetProjectId <= 0)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectMergeService.TargetProjectRequired"));
                }

                if (sourceProjectIds.Count == 0)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectMergeService.SourceProjectsRequired"));
                }

                if (sourceProjectIds.Contains(dto.TargetProjectId))
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectMergeService.TargetProjectCannotBeSource"));
                }

                var allProjectIds = sourceProjectIds
                    .Append(dto.TargetProjectId)
                    .Distinct()
                    .ToList();

                var projects = await _unitOfWork.Db.Projects
                    .Where(x => allProjectIds.Contains(x.Id) && !x.IsDeleted)
                    .ToListAsync();

                var targetProject = projects.FirstOrDefault(x => x.Id == dto.TargetProjectId);
                if (targetProject == null)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectMergeService.TargetProjectNotFound"));
                }

                var sourceProjects = projects
                    .Where(x => sourceProjectIds.Contains(x.Id))
                    .OrderBy(x => x.ProjectCode)
                    .ToList();

                if (sourceProjects.Count != sourceProjectIds.Count)
                {
                    throw new InvalidOperationException(_localizationService.GetLocalizedString("ProjectMergeService.SourceProjectNotFound"));
                }

                var activeProjectCages = await _unitOfWork.Db.ProjectCages
                    .Include(x => x.Cage)
                    .Where(x =>
                        sourceProjectIds.Contains(x.ProjectId) &&
                        !x.IsDeleted &&
                        x.ReleasedDate == null)
                    .OrderBy(x => x.ProjectId)
                    .ThenBy(x => x.CageId)
                    .ToListAsync();

                var movedProjectCageIds = activeProjectCages.Select(x => x.Id).ToList();
                var activeFishBatchIds = movedProjectCageIds.Count == 0
                    ? new List<long>()
                    : await _unitOfWork.Db.BatchCageBalances
                        .Where(x =>
                            movedProjectCageIds.Contains(x.ProjectCageId) &&
                            !x.IsDeleted &&
                            x.LiveCount > 0)
                        .Select(x => x.FishBatchId)
                        .Distinct()
                        .ToListAsync();

                var entity = new ProjectMerge
                {
                    TargetProjectId = targetProject.Id,
                    TargetProjectCode = targetProject.ProjectCode,
                    TargetProjectName = targetProject.ProjectName,
                    MergeDate = dto.MergeDate.Date,
                    Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                    SourceProjectStateAfterMerge = dto.SourceProjectStateAfterMerge,
                    CreatedBy = userId,
                    CreatedDate = DateTimeProvider.UtcNow,
                    IsDeleted = false,
                };

                foreach (var sourceProject in sourceProjects)
                {
                    entity.SourceProjects.Add(new ProjectMergeSource
                    {
                        SourceProjectId = sourceProject.Id,
                        SourceProjectCode = sourceProject.ProjectCode,
                        SourceProjectName = sourceProject.ProjectName,
                        CreatedBy = userId,
                        CreatedDate = DateTimeProvider.UtcNow,
                        IsDeleted = false,
                    });
                }

                foreach (var projectCage in activeProjectCages)
                {
                    entity.Cages.Add(new ProjectMergeCage
                    {
                        SourceProjectId = projectCage.ProjectId,
                        ProjectCageId = projectCage.Id,
                        CageId = projectCage.CageId,
                        CageCode = projectCage.Cage?.CageCode ?? string.Empty,
                        CageName = projectCage.Cage?.CageName ?? string.Empty,
                        CreatedBy = userId,
                        CreatedDate = DateTimeProvider.UtcNow,
                        IsDeleted = false,
                    });

                    projectCage.ProjectId = targetProject.Id;
                    projectCage.UpdatedBy = userId;
                    projectCage.UpdatedDate = DateTimeProvider.UtcNow;
                }

                if (activeFishBatchIds.Count > 0)
                {
                    var fishBatches = await _unitOfWork.Db.FishBatches
                        .Where(x => activeFishBatchIds.Contains(x.Id) && !x.IsDeleted)
                        .ToListAsync();

                    foreach (var fishBatch in fishBatches)
                    {
                        fishBatch.ProjectId = targetProject.Id;
                        fishBatch.UpdatedBy = userId;
                        fishBatch.UpdatedDate = DateTimeProvider.UtcNow;
                    }
                }

                foreach (var sourceProject in sourceProjects)
                {
                    sourceProject.Status = DocumentStatus.Cancelled;
                    if (sourceProject.EndDate == null || sourceProject.EndDate > dto.MergeDate.Date)
                    {
                        sourceProject.EndDate = dto.MergeDate.Date;
                    }

                    var mergeStateLabel = dto.SourceProjectStateAfterMerge == ProjectMergeSourceState.Passive
                        ? "Passive"
                        : "Archived";
                    var mergeNote = $"Merged into {targetProject.ProjectCode} on {dto.MergeDate:yyyy-MM-dd} ({mergeStateLabel}).";
                    sourceProject.Note = string.IsNullOrWhiteSpace(sourceProject.Note)
                        ? mergeNote
                        : $"{sourceProject.Note} {mergeNote}".Trim();
                    sourceProject.UpdatedBy = userId;
                    sourceProject.UpdatedDate = DateTimeProvider.UtcNow;
                }

                await _unitOfWork.Db.ProjectMerges.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.Commit();

                return ApiResponse<ProjectMergeDto>.SuccessResult(
                    MapDto(entity),
                    _localizationService.GetLocalizedString("ProjectMergeService.OperationSuccessful"));
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<ProjectMergeDto>.ErrorResult(
                    ex.Message,
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.Rollback();
                return ApiResponse<ProjectMergeDto>.ErrorResult(
                    _localizationService.GetLocalizedString("ProjectMergeService.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private IQueryable<ProjectMerge> QueryProjectMerges()
        {
            return _unitOfWork.Db.ProjectMerges
                .Include(x => x.SourceProjects)
                .Include(x => x.Cages)
                .AsQueryable();
        }

        private ProjectMergeDto MapDto(ProjectMerge entity)
        {
            return new ProjectMergeDto
            {
                Id = entity.Id,
                TargetProjectId = entity.TargetProjectId,
                TargetProjectCode = entity.TargetProjectCode,
                TargetProjectName = entity.TargetProjectName,
                MergeDate = entity.MergeDate,
                Description = entity.Description,
                SourceProjectStateAfterMerge = entity.SourceProjectStateAfterMerge,
                SourceProjects = entity.SourceProjects
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.SourceProjectCode)
                    .Select(x => new ProjectMergeSourceDto
                    {
                        Id = x.Id,
                        SourceProjectId = x.SourceProjectId,
                        SourceProjectCode = x.SourceProjectCode,
                        SourceProjectName = x.SourceProjectName,
                    })
                    .ToList(),
                Cages = entity.Cages
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.SourceProjectId)
                    .ThenBy(x => x.CageCode)
                    .Select(x => new ProjectMergeCageDto
                    {
                        Id = x.Id,
                        SourceProjectId = x.SourceProjectId,
                        ProjectCageId = x.ProjectCageId,
                        CageId = x.CageId,
                        CageCode = x.CageCode,
                        CageName = x.CageName,
                    })
                    .ToList(),
            };
        }
    }
}
