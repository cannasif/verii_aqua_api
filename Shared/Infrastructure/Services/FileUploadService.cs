using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace aqua_api.Shared.Infrastructure.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILocalizationService _localizationService;
        private const string ProfilePicturesFolder = "user-profiles";
        private const string StockImagesFolder = "stock-images";
        private const string ActivityImagesFolder = "activity-images";
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp"
        };
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/pjpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        public FileUploadService(IWebHostEnvironment environment, ILocalizationService localizationService)
        {
            _environment = environment;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<string>> UploadProfilePictureAsync(IFormFile file, long userId)
        {
            try
            {
                var validationError = ValidateImageFile(file);
                if (validationError != null)
                {
                    return validationError;
                }
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                // Create directory in project root/uploads/user-profiles (not wwwroot)
                var uploadsBasePath = Path.Combine(_environment.ContentRootPath, "uploads");
                var profilePicturesPath = Path.Combine(uploadsBasePath, ProfilePicturesFolder);
                var userFolder = Path.Combine(profilePicturesPath, userId.ToString());
                
                if (!Directory.Exists(uploadsBasePath))
                {
                    Directory.CreateDirectory(uploadsBasePath);
                    SetDirectoryPermissions(uploadsBasePath);
                }
                
                if (!Directory.Exists(profilePicturesPath))
                {
                    Directory.CreateDirectory(profilePicturesPath);
                    SetDirectoryPermissions(profilePicturesPath);
                }
                
                if (!Directory.Exists(userFolder))
                {
                    Directory.CreateDirectory(userFolder);
                    SetDirectoryPermissions(userFolder);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(userFolder, fileName);

                // Save file using proper async/await with resource disposal
                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await using (var sourceStream = file.OpenReadStream())
                    {
                        await sourceStream.CopyToAsync(fileStream).ConfigureAwait(false);
                        await fileStream.FlushAsync().ConfigureAwait(false);
                    }
                }

                // Return URL
                var url = GetProfilePictureUrl(fileName, userId);
                return ApiResponse<string>.SuccessResult(url, _localizationService.GetLocalizedString("FileUploadService.FileUploadedSuccessfully"));
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResult(
                    _localizationService.GetLocalizedString("FileUploadService.FileUploadError"),
                    _localizationService.GetLocalizedString("FileUploadService.FileUploadExceptionMessage", ex.Message),
                    500);
            }
        }

        public Task<ApiResponse<bool>> DeleteProfilePictureAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                {
                    return Task.FromResult(ApiResponse<bool>.SuccessResult(
                        true,
                        _localizationService.GetLocalizedString("FileUploadService.NoFileToDelete")));
                }

                // Extract file path from URL
                // URL format: /uploads/user-profiles/{userId}/{fileName}
                // Handle both relative and absolute URLs
                string pathToParse = fileUrl.Trim();
                
                // Remove query string if exists
                if (pathToParse.Contains('?'))
                {
                    pathToParse = pathToParse.Substring(0, pathToParse.IndexOf('?'));
                }

                // Handle absolute URLs
                if (Uri.TryCreate(pathToParse, UriKind.Absolute, out Uri? absoluteUri))
                {
                    pathToParse = absoluteUri.AbsolutePath;
                }
                
                // Ensure path starts with /
                if (!pathToParse.StartsWith("/"))
                {
                    pathToParse = "/" + pathToParse;
                }

                var pathSegments = pathToParse.Split('/', StringSplitOptions.RemoveEmptyEntries);
                
                if (pathSegments.Length < 4 || pathSegments[0] != "uploads" || pathSegments[1] != "user-profiles")
                {
                    var expectedFormat = "/uploads/user-profiles/{userId}/{fileName}";
                    return Task.FromResult(ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("FileUploadService.InvalidFileUrl", expectedFormat, fileUrl),
                        null,
                        400));
                }

                var userId = pathSegments[2];
                var fileName = pathSegments[3];
                
                // Build file path using ContentRootPath (not WebRootPath)
                var filePath = Path.Combine(
                    _environment.ContentRootPath,
                    "uploads",
                    "user-profiles",
                    userId,
                    fileName);

                if (File.Exists(filePath))
                {
                    // Delete file synchronously (File.Delete is already synchronous and fast)
                    File.Delete(filePath);
                    return Task.FromResult(ApiResponse<bool>.SuccessResult(
                        true,
                        _localizationService.GetLocalizedString("FileUploadService.FileDeletedSuccessfully")));
                }
                else
                {
                    // File doesn't exist, but that's okay - it might have been already deleted
                    return Task.FromResult(ApiResponse<bool>.SuccessResult(
                        true,
                        _localizationService.GetLocalizedString("FileUploadService.FileDeletedSuccessfully")));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("FileUploadService.FileDeletionError"),
                    _localizationService.GetLocalizedString("FileUploadService.FileDeletionExceptionMessage", ex.Message),
                    500));
            }
        }

        public string GetProfilePictureUrl(string fileName, long userId)
        {
            // Return relative URL that will be served by static files middleware
            return $"/uploads/{ProfilePicturesFolder}/{userId}/{fileName}";
        }

        public async Task<ApiResponse<string>> UploadStockImageAsync(IFormFile file, long stockId)
        {
            try
            {
                var validationError = ValidateImageFile(file);
                if (validationError != null)
                {
                    return validationError;
                }
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                // Create directory in project root/uploads/stock-images
                var uploadsBasePath = Path.Combine(_environment.ContentRootPath, "uploads");
                var stockImagesPath = Path.Combine(uploadsBasePath, StockImagesFolder);
                var stockFolder = Path.Combine(stockImagesPath, stockId.ToString());
                
                if (!Directory.Exists(uploadsBasePath))
                {
                    Directory.CreateDirectory(uploadsBasePath);
                    SetDirectoryPermissions(uploadsBasePath);
                }
                
                if (!Directory.Exists(stockImagesPath))
                {
                    Directory.CreateDirectory(stockImagesPath);
                    SetDirectoryPermissions(stockImagesPath);
                }
                
                if (!Directory.Exists(stockFolder))
                {
                    Directory.CreateDirectory(stockFolder);
                    SetDirectoryPermissions(stockFolder);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(stockFolder, fileName);

                // Save file using proper async/await with resource disposal
                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await using (var sourceStream = file.OpenReadStream())
                    {
                        await sourceStream.CopyToAsync(fileStream).ConfigureAwait(false);
                        await fileStream.FlushAsync().ConfigureAwait(false);
                    }
                }

                // Return URL
                var url = GetStockImageUrl(fileName, stockId);
                return ApiResponse<string>.SuccessResult(url, _localizationService.GetLocalizedString("FileUploadService.FileUploadedSuccessfully"));
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResult(
                    _localizationService.GetLocalizedString("FileUploadService.FileUploadError"),
                    _localizationService.GetLocalizedString("FileUploadService.FileUploadExceptionMessage", ex.Message),
                    500);
            }
        }

        public Task<ApiResponse<bool>> DeleteStockImageAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                {
                    return Task.FromResult(ApiResponse<bool>.SuccessResult(
                        true,
                        _localizationService.GetLocalizedString("FileUploadService.NoFileToDelete")));
                }

                // Extract file path from URL
                // URL format: /uploads/stock-images/{stockId}/{fileName}
                string pathToParse = fileUrl.Trim();
                
                // Remove query string if exists
                if (pathToParse.Contains('?'))
                {
                    pathToParse = pathToParse.Substring(0, pathToParse.IndexOf('?'));
                }

                // Handle absolute URLs
                if (Uri.TryCreate(pathToParse, UriKind.Absolute, out Uri? absoluteUri))
                {
                    pathToParse = absoluteUri.AbsolutePath;
                }
                
                // Ensure path starts with /
                if (!pathToParse.StartsWith("/"))
                {
                    pathToParse = "/" + pathToParse;
                }

                var pathSegments = pathToParse.Split('/', StringSplitOptions.RemoveEmptyEntries);
                
                if (pathSegments.Length < 4 || pathSegments[0] != "uploads" || pathSegments[1] != StockImagesFolder)
                {
                    var expectedFormat = $"/uploads/{StockImagesFolder}/{{stockId}}/{{fileName}}";
                    return Task.FromResult(ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("FileUploadService.InvalidFileUrl", expectedFormat, fileUrl),
                        null,
                        400));
                }

                var stockId = pathSegments[2];
                var fileName = pathSegments[3];
                
                // Build file path using ContentRootPath
                var filePath = Path.Combine(
                    _environment.ContentRootPath,
                    "uploads",
                    StockImagesFolder,
                    stockId,
                    fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(ApiResponse<bool>.SuccessResult(
                        true,
                        _localizationService.GetLocalizedString("FileUploadService.FileDeletedSuccessfully")));
                }
                else
                {
                    return Task.FromResult(ApiResponse<bool>.SuccessResult(
                        true,
                        _localizationService.GetLocalizedString("FileUploadService.FileDeletedSuccessfully")));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("FileUploadService.FileDeletionError"),
                    _localizationService.GetLocalizedString("FileUploadService.FileDeletionExceptionMessage", ex.Message),
                    500));
            }
        }

        public string GetStockImageUrl(string fileName, long stockId)
        {
            // Return relative URL that will be served by static files middleware
            return $"/uploads/{StockImagesFolder}/{stockId}/{fileName}";
        }


        public async Task<ApiResponse<string>> UploadActivityImageAsync(IFormFile file, long activityId)
        {
            try
            {
                var validationError = ValidateImageFile(file);
                if (validationError != null)
                {
                    return validationError;
                }
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                var uploadsBasePath = Path.Combine(_environment.ContentRootPath, "uploads");
                var activityImagesPath = Path.Combine(uploadsBasePath, ActivityImagesFolder);
                var activityFolder = Path.Combine(activityImagesPath, activityId.ToString());

                if (!Directory.Exists(uploadsBasePath))
                {
                    Directory.CreateDirectory(uploadsBasePath);
                    SetDirectoryPermissions(uploadsBasePath);
                }

                if (!Directory.Exists(activityImagesPath))
                {
                    Directory.CreateDirectory(activityImagesPath);
                    SetDirectoryPermissions(activityImagesPath);
                }

                if (!Directory.Exists(activityFolder))
                {
                    Directory.CreateDirectory(activityFolder);
                    SetDirectoryPermissions(activityFolder);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(activityFolder, fileName);

                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await using (var sourceStream = file.OpenReadStream())
                    {
                        await sourceStream.CopyToAsync(fileStream).ConfigureAwait(false);
                        await fileStream.FlushAsync().ConfigureAwait(false);
                    }
                }

                var url = GetActivityImageUrl(fileName, activityId);
                return ApiResponse<string>.SuccessResult(url, _localizationService.GetLocalizedString("FileUploadService.FileUploadedSuccessfully"));
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResult(
                    _localizationService.GetLocalizedString("FileUploadService.FileUploadError"),
                    _localizationService.GetLocalizedString("FileUploadService.FileUploadExceptionMessage", ex.Message),
                    500);
            }
        }

        public Task<ApiResponse<bool>> DeleteActivityImageAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                {
                    return Task.FromResult(ApiResponse<bool>.SuccessResult(
                        true,
                        _localizationService.GetLocalizedString("FileUploadService.NoFileToDelete")));
                }

                string pathToParse = fileUrl.Trim();

                if (pathToParse.Contains('?'))
                {
                    pathToParse = pathToParse.Substring(0, pathToParse.IndexOf('?'));
                }

                if (Uri.TryCreate(pathToParse, UriKind.Absolute, out Uri? absoluteUri))
                {
                    pathToParse = absoluteUri.AbsolutePath;
                }

                if (!pathToParse.StartsWith("/"))
                {
                    pathToParse = "/" + pathToParse;
                }

                var pathSegments = pathToParse.Split('/', StringSplitOptions.RemoveEmptyEntries);

                if (pathSegments.Length < 4 || pathSegments[0] != "uploads" || pathSegments[1] != ActivityImagesFolder)
                {
                    var expectedFormat = $"/uploads/{ActivityImagesFolder}/{{activityId}}/{{fileName}}";
                    return Task.FromResult(ApiResponse<bool>.ErrorResult(
                        _localizationService.GetLocalizedString("FileUploadService.InvalidFileUrl", expectedFormat, fileUrl),
                        null,
                        400));
                }

                var activityId = pathSegments[2];
                var fileName = pathSegments[3];

                var filePath = Path.Combine(
                    _environment.ContentRootPath,
                    "uploads",
                    ActivityImagesFolder,
                    activityId,
                    fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return Task.FromResult(ApiResponse<bool>.SuccessResult(
                    true,
                    _localizationService.GetLocalizedString("FileUploadService.FileDeletedSuccessfully")));
            }
            catch (Exception ex)
            {
                return Task.FromResult(ApiResponse<bool>.ErrorResult(
                    _localizationService.GetLocalizedString("FileUploadService.FileDeletionError"),
                    _localizationService.GetLocalizedString("FileUploadService.FileDeletionExceptionMessage", ex.Message),
                    500));
            }
        }

        public string GetActivityImageUrl(string fileName, long activityId)
        {
            return $"/uploads/{ActivityImagesFolder}/{activityId}/{fileName}";
        }

        /// <summary>
        /// Sets read/write permissions for the directory (Windows only)
        /// </summary>
        private void SetDirectoryPermissions(string directoryPath)
        {
            // Only set permissions on Windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            try
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                var directorySecurity = directoryInfo.GetAccessControl();

                // Get current user
                var currentUser = WindowsIdentity.GetCurrent();
                if (currentUser != null && currentUser.User != null)
                {
                    // Add full control for current user
                    var accessRule = new FileSystemAccessRule(
                        currentUser.User,
                        FileSystemRights.FullControl,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow);

                    directorySecurity.AddAccessRule(accessRule);
                    directoryInfo.SetAccessControl(directorySecurity);
                }

                // Also add permissions for IIS_IUSRS (if running under IIS)
                try
                {
                    var iisUser = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                    var iisAccessRule = new FileSystemAccessRule(
                        iisUser,
                        FileSystemRights.ReadAndExecute | FileSystemRights.Write | FileSystemRights.Modify,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow);

                    directorySecurity.AddAccessRule(iisAccessRule);
                    directoryInfo.SetAccessControl(directorySecurity);
                }
                catch
                {
                    // Ignore if IIS_IUSRS doesn't exist (e.g., running outside IIS)
                }

                // Add permissions for Everyone (development only)
                if (_environment.IsDevelopment())
                {
                    try
                    {
                        var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                        var everyoneAccessRule = new FileSystemAccessRule(
                            everyone,
                            FileSystemRights.ReadAndExecute | FileSystemRights.Write,
                            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                            PropagationFlags.None,
                            AccessControlType.Allow);

                        directorySecurity.AddAccessRule(everyoneAccessRule);
                        directoryInfo.SetAccessControl(directorySecurity);
                    }
                    catch
                    {
                        // Ignore if setting permissions fails
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail if permission setting is not supported (e.g., on Linux)
                // The directory will still be created, but permissions might need to be set manually
            }
        }

        private ApiResponse<string>? ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return ApiResponse<string>.ErrorResult(
                    _localizationService.GetLocalizedString("FileUploadService.FileRequired"),
                    null,
                    400);
            }

            if (file.Length > MaxFileSize)
            {
                var maxSizeMb = MaxFileSize / (1024 * 1024);
                return ApiResponse<string>.ErrorResult(
                    _localizationService.GetLocalizedString("FileUploadService.FileSizeExceeded", maxSizeMb),
                    null,
                    400);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                var allowedFormats = string.Join(", ", AllowedExtensions);
                return ApiResponse<string>.ErrorResult(
                    _localizationService.GetLocalizedString("FileUploadService.InvalidFileFormat", allowedFormats),
                    null,
                    400);
            }

            var contentType = file.ContentType?.Trim();
            if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType))
            {
                var allowedFormats = string.Join(", ", AllowedExtensions);
                return ApiResponse<string>.ErrorResult(
                    _localizationService.GetLocalizedString("FileUploadService.InvalidFileFormat", allowedFormats),
                    null,
                    400);
            }

            return null;
        }
    }
}
