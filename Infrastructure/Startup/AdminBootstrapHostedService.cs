using aqua_api.Data;
using aqua_api.Infrastructure.Time;
using aqua_api.Models;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Infrastructure.Startup
{
    public class AdminBootstrapHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AdminBootstrapHostedService> _logger;

        public AdminBootstrapHostedService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<AdminBootstrapHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var email = FirstNonEmpty(
                _configuration["BootstrapAdmin:Email"],
                _configuration["AdminSettings:Email"],
                "admin@v3rii.com");

            var username = FirstNonEmpty(
                _configuration["BootstrapAdmin:Username"],
                _configuration["AdminSettings:Username"],
                "adminv3rii.com");

            var password = FirstNonEmpty(
                _configuration["BootstrapAdmin:Password"],
                _configuration["AdminSettings:Password"],
                "Veriipass123!");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            await using var scope = _serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();

            var roleId = 3L;
            var roleIdStr = _configuration["BootstrapAdmin:RoleId"];
            if (!string.IsNullOrWhiteSpace(roleIdStr) && long.TryParse(roleIdStr, out var parsed))
            {
                roleId = parsed;
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var normalizedUsername = username.Trim();

            var role = await db.Set<UserAuthority>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role == null)
            {
                role = new UserAuthority
                {
                    Id = roleId,
                    Title = roleId == 3 ? "Admin" : $"Role-{roleId}",
                    CreatedDate = DateTimeProvider.Now,
                    IsDeleted = false
                };

                db.Set<UserAuthority>().Add(role);
                await db.SaveChangesAsync(cancellationToken);
            }

            var existing = await db.Set<User>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    u => u.Email == normalizedEmail || u.Username == normalizedUsername,
                    cancellationToken);

            if (existing == null)
            {
                var user = new User
                {
                    Username = normalizedUsername,
                    Email = normalizedEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    FirstName = "Admin",
                    LastName = "User",
                    RoleId = roleId,
                    IsEmailConfirmed = true,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTimeProvider.Now
                };

                db.Set<User>().Add(user);
                await db.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Bootstrap admin user created in {Environment}.",
                    _environment.EnvironmentName);
                return;
            }

            var changed = false;

            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.DeletedDate = null;
                existing.DeletedBy = null;
                changed = true;
            }

            if (!existing.IsActive)
            {
                existing.IsActive = true;
                changed = true;
            }

            if (!existing.IsEmailConfirmed)
            {
                existing.IsEmailConfirmed = true;
                changed = true;
            }

            if (existing.RoleId != roleId)
            {
                existing.RoleId = roleId;
                changed = true;
            }

            if (!string.Equals(existing.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                existing.Email = normalizedEmail;
                changed = true;
            }

            if (!string.Equals(existing.Username, normalizedUsername, StringComparison.Ordinal))
            {
                existing.Username = normalizedUsername;
                changed = true;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, existing.PasswordHash))
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                changed = true;
            }

            if (changed)
            {
                existing.UpdatedDate = DateTimeProvider.Now;
                await db.SaveChangesAsync(cancellationToken);
                _logger.LogWarning(
                    "Bootstrap admin user synchronized in {Environment}.",
                    _environment.EnvironmentName);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static string FirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return string.Empty;
        }
    }
}
