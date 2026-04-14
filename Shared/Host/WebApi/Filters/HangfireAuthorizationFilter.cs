using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

namespace aqua_api.Shared.Host.WebApi.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            var user = httpContext.User;
            if (user.IsInRole("Admin"))
            {
                return true;
            }

            return user.Claims.Any(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == "Admin");
        }
    }
}
