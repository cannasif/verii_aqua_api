using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace aqua_api.Shared.Host.WebApi.Filters
{
    /// <summary>
    /// Adds deprecation headers to API responses. Use on legacy controllers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class DeprecatedApiAttribute : ResultFilterAttribute
    {
        public string? Replacement { get; set; }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers.Append("X-API-Deprecated", "true");
            if (!string.IsNullOrEmpty(Replacement))
                context.HttpContext.Response.Headers.Append("X-API-Replacement", Replacement);
            base.OnResultExecuting(context);
        }
    }
}
