using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Hangfire;
using aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces;

namespace aqua_api.Modules.Identity.Application.Services
{
    public class UserService : IUserService
    {
        private const long UserRoleId = 1;
        private const long AdminRoleId = 3;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _loc;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public UserService(IUnitOfWork uow, IMapper mapper, ILocalizationService loc, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _uow = uow; _mapper = mapper; _loc = loc;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public Task<ApiResponse<long>> GetCurrentUserIdAsync()
        {
            try
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                {
                    return Task.FromResult(ApiResponse<long>.ErrorResult(
                        _loc.GetLocalizedString("UserService.InvalidUserId"),
                        _loc.GetLocalizedString("UserService.InvalidUserId"),
                        StatusCodes.Status400BadRequest));
                }

                return Task.FromResult(ApiResponse<long>.SuccessResult(
                    userId,
                    _loc.GetLocalizedString("UserService.UserIdRetrieved")));
            }
            catch (Exception ex)
            {
                return Task.FromResult(ApiResponse<long>.ErrorResult(
                    _loc.GetLocalizedString("UserService.InternalServerError"),
                    _loc.GetLocalizedString("UserService.GetCurrentUserIdExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError));
            }
        }

        public async Task<ApiResponse<PagedResponse<UserDto>>> GetAllUsersAsync(PagedRequest request)
        {
            try
            {
                if (request == null)
                {
                    request = new PagedRequest();
                }

                if (request.Filters == null)
                {
                    request.Filters = new List<Filter>();
                }

                var columnMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "role", "RoleNavigation.Title" }
                };

                var query = _uow.Users.Query()
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted)
                    .Include(u => u.RoleNavigation)
                    .Include(u => u.CreatedByUser)
                    .Include(u => u.UpdatedByUser)
                    .Include(u => u.DeletedByUser)
                    .ApplyFilters(request.Filters, request.FilterLogic, columnMapping);

                var sortBy = request.SortBy ?? nameof(User.Id);

                query = query.ApplySorting(sortBy, request.SortDirection, columnMapping);

                var totalCount = await query.CountAsync();

                var items = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .ToListAsync();

                var dtos = items.Select(x => _mapper.Map<UserDto>(x)).ToList();

                var pagedResponse = new PagedResponse<UserDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<UserDto>>.SuccessResult(pagedResponse, _loc.GetLocalizedString("UserService.UsersRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<UserDto>>.ErrorResult(
                    _loc.GetLocalizedString("UserService.InternalServerError"),
                    _loc.GetLocalizedString("UserService.GetAllUsersExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(long id)
        {
            try
            {
                var user = await _uow.Users.GetByIdAsync(id);
                if (user == null) return ApiResponse<UserDto>.ErrorResult(
                    _loc.GetLocalizedString("UserService.UserNotFound"),
                    _loc.GetLocalizedString("UserService.UserNotFound"),
                    StatusCodes.Status404NotFound);

                // Reload with navigation properties for mapping
                var userWithNav = await _uow.Users.Query()
                    .AsNoTracking()
                    .Include(u => u.RoleNavigation)
                    .Include(u => u.CreatedByUser)
                    .Include(u => u.UpdatedByUser)
                    .Include(u => u.DeletedByUser)
                    .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

                var dto = _mapper.Map<UserDto>(userWithNav ?? user);
                return ApiResponse<UserDto>.SuccessResult(dto, _loc.GetLocalizedString("UserService.UserRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResult(
                    _loc.GetLocalizedString("UserService.InternalServerError"),
                    _loc.GetLocalizedString("UserService.GetUserByIdExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Email))
                {
                    return ApiResponse<UserDto>.ErrorResult(
                        _loc.GetLocalizedString("General.ValidationError"),
                        _loc.GetLocalizedString("General.ValidationError"),
                        StatusCodes.Status400BadRequest);
                }

                var existsByEmail = await _uow.Users.Query()
                    .AsNoTracking()
                    .AnyAsync(x => !x.IsDeleted && x.Email == dto.Email);

                if (existsByEmail)
                {
                    return ApiResponse<UserDto>.ErrorResult(
                        _loc.GetLocalizedString("UserService.UserAlreadyExists"),
                        _loc.GetLocalizedString("UserService.UserAlreadyExists"),
                        StatusCodes.Status400BadRequest);
                }

                var existsByUsername = await _uow.Users.Query()
                    .AsNoTracking()
                    .AnyAsync(x => !x.IsDeleted && x.Username == dto.Username);

                if (existsByUsername)
                {
                    return ApiResponse<UserDto>.ErrorResult(
                        _loc.GetLocalizedString("UserService.UserAlreadyExists"),
                        _loc.GetLocalizedString("UserService.UserAlreadyExists"),
                        StatusCodes.Status400BadRequest);
                }

                var normalizedCreate = await NormalizeRoleAndPermissionGroupsAsync(
                    dto.RoleId,
                    dto.PermissionGroupIds);

                if (!normalizedCreate.Success)
                {
                    return ApiResponse<UserDto>.ErrorResult(
                        normalizedCreate.Message,
                        normalizedCreate.ExceptionMessage,
                        normalizedCreate.StatusCode);
                }

                dto.RoleId = normalizedCreate.Data.RoleId;
                dto.PermissionGroupIds = normalizedCreate.Data.PermissionGroupIds;

                var plainPassword = string.IsNullOrWhiteSpace(dto.Password)
                    ? GenerateTemporaryPassword()
                    : dto.Password;

                var entity = _mapper.Map<User>(dto);
                entity.IsEmailConfirmed = true;
                entity.IsActive = dto.IsActive ?? true;
                entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
                await _uow.Users.AddAsync(entity);
                await _uow.SaveChangesAsync();

                if (dto.PermissionGroupIds != null)
                {
                    var syncResult = await SyncUserPermissionGroupsAsync(entity.Id, dto.PermissionGroupIds);
                    if (!syncResult.Success)
                    {
                        return ApiResponse<UserDto>.ErrorResult(syncResult.Message, syncResult.ExceptionMessage, syncResult.StatusCode);
                    }
                }

                // Reload with navigation properties for mapping
                var userWithNav = await _uow.Users.Query()
                    .AsNoTracking()
                    .Include(u => u.RoleNavigation)
                    .Include(u => u.CreatedByUser)
                    .Include(u => u.UpdatedByUser)
                    .Include(u => u.DeletedByUser)
                    .FirstOrDefaultAsync(u => u.Id == entity.Id && !u.IsDeleted);

                var outDto = _mapper.Map<UserDto>(userWithNav ?? entity);
                
                    var baseUrl = _configuration["FrontendSettings:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5173";
                    BackgroundJob.Enqueue<IMailJob>(job =>
                        job.SendUserCreatedEmailAsync(dto.Email, dto.Username, plainPassword, dto.FirstName, dto.LastName, baseUrl));

                return ApiResponse<UserDto>.SuccessResult(outDto, _loc.GetLocalizedString("UserService.UserCreated"));
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;

                if (innerMessage.Contains("IX_Users_Email", StringComparison.OrdinalIgnoreCase) ||
                    innerMessage.Contains("IX_Users_Username", StringComparison.OrdinalIgnoreCase) ||
                    innerMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<UserDto>.ErrorResult(
                        _loc.GetLocalizedString("UserService.UserAlreadyExists"),
                        _loc.GetLocalizedString("UserService.UserAlreadyExists"),
                        StatusCodes.Status400BadRequest);
                }

                if (innerMessage.Contains("RII_USER_AUTHORITY", StringComparison.OrdinalIgnoreCase) ||
                    innerMessage.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<UserDto>.ErrorResult(
                        _loc.GetLocalizedString("General.ValidationError"),
                        _loc.GetLocalizedString("General.ValidationError"),
                        StatusCodes.Status400BadRequest);
                }

                return ApiResponse<UserDto>.ErrorResult(
                    _loc.GetLocalizedString("UserService.InternalServerError"),
                    _loc.GetLocalizedString("UserService.CreateUserExceptionMessage", innerMessage),
                    StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResult(
                    _loc.GetLocalizedString("UserService.InternalServerError"),
                    _loc.GetLocalizedString("UserService.CreateUserExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateUserAsync(long id, UpdateUserDto dto)
        {
            try
            {
                var entity = await _uow.Users.GetByIdForUpdateAsync(id);
                if (entity == null) return ApiResponse<UserDto>.ErrorResult(
                    _loc.GetLocalizedString("UserService.UserNotFound"),
                    null,
                    StatusCodes.Status404NotFound);

                if (dto.Email != null && string.IsNullOrWhiteSpace(dto.Email))
                {
                    return ApiResponse<UserDto>.ErrorResult(
                        _loc.GetLocalizedString("General.ValidationError"),
                        _loc.GetLocalizedString("General.ValidationError"),
                        StatusCodes.Status400BadRequest);
                }

                if (!string.IsNullOrWhiteSpace(dto.Email) &&
                    !dto.Email.Equals(entity.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _uow.Users.Query()
                        .AsNoTracking()
                        .AnyAsync(x => !x.IsDeleted && x.Id != id && x.Email == dto.Email);

                    if (emailExists)
                    {
                        return ApiResponse<UserDto>.ErrorResult(
                            _loc.GetLocalizedString("UserService.UserAlreadyExists"),
                            _loc.GetLocalizedString("UserService.UserAlreadyExists"),
                            StatusCodes.Status400BadRequest);
                    }
                }

                var currentPermissionGroupIds = await _uow.UserPermissionGroups.Query()
                    .AsNoTracking()
                    .Where(x => x.UserId == entity.Id && !x.IsDeleted)
                    .Select(x => x.PermissionGroupId)
                    .ToListAsync();

                var requestedRoleId = dto.RoleId.HasValue && dto.RoleId.Value > 0
                    ? dto.RoleId.Value
                    : entity.RoleId;

                var requestedPermissionGroupIds = dto.PermissionGroupIds ?? currentPermissionGroupIds;

                var normalizedUpdate = await NormalizeRoleAndPermissionGroupsAsync(
                    requestedRoleId,
                    requestedPermissionGroupIds);

                if (!normalizedUpdate.Success)
                {
                    return ApiResponse<UserDto>.ErrorResult(
                        normalizedUpdate.Message,
                        normalizedUpdate.ExceptionMessage,
                        normalizedUpdate.StatusCode);
                }

                dto.RoleId = normalizedUpdate.Data.RoleId;

                _mapper.Map(dto, entity);
                await _uow.Users.UpdateAsync(entity);
                await _uow.SaveChangesAsync();

                var syncResult = await SyncUserPermissionGroupsAsync(entity.Id, normalizedUpdate.Data.PermissionGroupIds);
                if (!syncResult.Success)
                {
                    return ApiResponse<UserDto>.ErrorResult(syncResult.Message, syncResult.ExceptionMessage, syncResult.StatusCode);
                }

                // Reload with navigation properties for mapping
                var userWithNav = await _uow.Users.Query()
                    .AsNoTracking()
                    .Include(u => u.RoleNavigation)
                    .Include(u => u.CreatedByUser)
                    .Include(u => u.UpdatedByUser)
                    .Include(u => u.DeletedByUser)
                    .FirstOrDefaultAsync(u => u.Id == entity.Id && !u.IsDeleted);

                var outDto = _mapper.Map<UserDto>(userWithNav ?? entity);
                return ApiResponse<UserDto>.SuccessResult(outDto, _loc.GetLocalizedString("UserService.UserUpdated"));
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;

                if (innerMessage.Contains("IX_Users_Email", StringComparison.OrdinalIgnoreCase) ||
                    innerMessage.Contains("IX_Users_Username", StringComparison.OrdinalIgnoreCase) ||
                    innerMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<UserDto>.ErrorResult(
                        _loc.GetLocalizedString("UserService.UserAlreadyExists"),
                        _loc.GetLocalizedString("UserService.UserAlreadyExists"),
                        StatusCodes.Status400BadRequest);
                }

                if (innerMessage.Contains("RII_USER_AUTHORITY", StringComparison.OrdinalIgnoreCase) ||
                    innerMessage.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<UserDto>.ErrorResult(
                        _loc.GetLocalizedString("General.ValidationError"),
                        _loc.GetLocalizedString("General.ValidationError"),
                        StatusCodes.Status400BadRequest);
                }

                return ApiResponse<UserDto>.ErrorResult(
                    _loc.GetLocalizedString("UserService.InternalServerError"),
                    _loc.GetLocalizedString("UserService.UpdateUserExceptionMessage", innerMessage),
                    StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResult(
                    _loc.GetLocalizedString("UserService.InternalServerError"),
                    _loc.GetLocalizedString("UserService.UpdateUserExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<object>> DeleteUserAsync(long id)
        {
            try
            {
                var entity = await _uow.Users.GetByIdAsync(id);
                if (entity == null) return ApiResponse<object>.ErrorResult(
                    _loc.GetLocalizedString("UserService.UserNotFound"),
                    _loc.GetLocalizedString("UserService.UserNotFound"),
                    StatusCodes.Status404NotFound);
                await _uow.Users.SoftDeleteAsync(id);
                await _uow.SaveChangesAsync();
                return ApiResponse<object>.SuccessResult(null, _loc.GetLocalizedString("UserService.UserDeleted"));
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResult(
                    _loc.GetLocalizedString("UserService.InternalServerError"),
                    _loc.GetLocalizedString("UserService.DeleteUserExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserProfileAsync(string userId)
        {
            try
            {
                if (!long.TryParse(userId, out var userIdLong))
                {
                    return ApiResponse<UserDto>.ErrorResult(
                        _loc.GetLocalizedString("UserService.InvalidUserId"),
                        null,
                        StatusCodes.Status400BadRequest);
                }

                var user = await _uow.Users.GetByIdAsync(userIdLong);
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResult(
                        _loc.GetLocalizedString("UserService.UserNotFound"),
                        _loc.GetLocalizedString("UserService.UserNotFound"),
                        StatusCodes.Status404NotFound);
                }

                var dto = _mapper.Map<UserDto>(user);
                return ApiResponse<UserDto>.SuccessResult(dto, _loc.GetLocalizedString("UserService.UserRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResult(
                    _loc.GetLocalizedString("UserService.InternalServerError"),
                    _loc.GetLocalizedString("UserService.GetUserProfileExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        private string GenerateTemporaryPassword()
        {
            var seed = Guid.NewGuid().ToString("N")[..10];
            return $"V3r!{seed}";
        }

        private async Task<ApiResponse<(long RoleId, List<long> PermissionGroupIds)>> NormalizeRoleAndPermissionGroupsAsync(
            long requestedRoleId,
            IEnumerable<long>? permissionGroupIds)
        {
            try
            {
                var normalizedRoleId = requestedRoleId == AdminRoleId
                    ? AdminRoleId
                    : UserRoleId;

                if (normalizedRoleId != UserRoleId && normalizedRoleId != AdminRoleId)
                {
                    return ApiResponse<(long RoleId, List<long> PermissionGroupIds)>.ErrorResult(
                        _loc.GetLocalizedString("General.ValidationError"),
                        _loc.GetLocalizedString("General.ValidationError"),
                        StatusCodes.Status400BadRequest);
                }

                var roleExists = await _uow.UserAuthorities.Query()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == normalizedRoleId && !x.IsDeleted);

                if (!roleExists)
                {
                    return ApiResponse<(long RoleId, List<long> PermissionGroupIds)>.ErrorResult(
                        _loc.GetLocalizedString("General.ValidationError"),
                        _loc.GetLocalizedString("General.ValidationError"),
                        StatusCodes.Status400BadRequest);
                }

                var normalizedPermissionGroupIds = permissionGroupIds?
                    .Distinct()
                    .ToList() ?? new List<long>();

                var validateGroups = await ValidatePermissionGroupIdsAsync(normalizedPermissionGroupIds);
                if (!validateGroups.Success)
                {
                    return ApiResponse<(long RoleId, List<long> PermissionGroupIds)>.ErrorResult(
                        validateGroups.Message,
                        validateGroups.ExceptionMessage,
                        validateGroups.StatusCode);
                }

                var systemAdminGroupIds = await _uow.PermissionGroups.Query()
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.IsActive && x.IsSystemAdmin)
                    .Select(x => x.Id)
                    .ToListAsync();

                var hasSystemAdminGroup = normalizedPermissionGroupIds.Any(groupId => systemAdminGroupIds.Contains(groupId));

                if (hasSystemAdminGroup)
                {
                    normalizedRoleId = AdminRoleId;
                }

                if (normalizedRoleId == AdminRoleId)
                {
                    normalizedPermissionGroupIds = normalizedPermissionGroupIds
                        .Union(systemAdminGroupIds)
                        .Distinct()
                        .ToList();
                }
                else
                {
                    normalizedPermissionGroupIds = normalizedPermissionGroupIds
                        .Where(groupId => !systemAdminGroupIds.Contains(groupId))
                        .ToList();
                }

                return ApiResponse<(long RoleId, List<long> PermissionGroupIds)>.SuccessResult(
                    (normalizedRoleId, normalizedPermissionGroupIds),
                    _loc.GetLocalizedString("General.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<(long RoleId, List<long> PermissionGroupIds)>.ErrorResult(
                    _loc.GetLocalizedString("General.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<ApiResponse<bool>> ValidatePermissionGroupIdsAsync(IEnumerable<long> permissionGroupIds)
        {
            try
            {
                var distinctGroupIds = permissionGroupIds.Distinct().ToList();
                if (distinctGroupIds.Count == 0)
                {
                    return ApiResponse<bool>.SuccessResult(true, _loc.GetLocalizedString("General.OperationSuccessful"));
                }

                var validCount = await _uow.PermissionGroups.Query()
                    .AsNoTracking()
                    .CountAsync(x => !x.IsDeleted && distinctGroupIds.Contains(x.Id));

                if (validCount != distinctGroupIds.Count)
                {
                    return ApiResponse<bool>.ErrorResult(
                        _loc.GetLocalizedString("General.ValidationError"),
                        _loc.GetLocalizedString("General.ValidationError"),
                        StatusCodes.Status400BadRequest);
                }

                return ApiResponse<bool>.SuccessResult(true, _loc.GetLocalizedString("General.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _loc.GetLocalizedString("General.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<ApiResponse<bool>> SyncUserPermissionGroupsAsync(long userId, IEnumerable<long> permissionGroupIds)
        {
            try
            {
                var distinctGroupIds = permissionGroupIds.Distinct().ToList();

                var validate = await ValidatePermissionGroupIdsAsync(distinctGroupIds);
                if (!validate.Success)
                {
                    return validate;
                }

                var allLinks = await _uow.UserPermissionGroups
                    .Query(tracking: true, ignoreQueryFilters: true)
                    .Where(x => x.UserId == userId)
                    .ToListAsync();

                // Soft-delete links not desired anymore
                foreach (var link in allLinks.Where(x => !x.IsDeleted && !distinctGroupIds.Contains(x.PermissionGroupId)))
                {
                    await _uow.UserPermissionGroups.SoftDeleteAsync(link.Id);
                }

                // Ensure each desired groupId is active; revive if previously soft-deleted
                foreach (var groupId in distinctGroupIds)
                {
                    var existing = allLinks.FirstOrDefault(x => x.PermissionGroupId == groupId);
                    if (existing == null)
                    {
                        await _uow.UserPermissionGroups.AddAsync(new UserPermissionGroup
                        {
                            UserId = userId,
                            PermissionGroupId = groupId
                        });
                        continue;
                    }

                    if (existing.IsDeleted)
                    {
                        existing.IsDeleted = false;
                        existing.DeletedDate = null;
                        existing.DeletedBy = null;
                        await _uow.UserPermissionGroups.UpdateAsync(existing);
                    }
                }

                await _uow.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, _loc.GetLocalizedString("General.OperationSuccessful"));
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    _loc.GetLocalizedString("General.InternalServerError"),
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
