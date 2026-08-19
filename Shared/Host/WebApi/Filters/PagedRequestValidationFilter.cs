using aqua_api.Shared.Common.Exceptions;
using aqua_api.Shared.Common.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace aqua_api.Shared.Host.WebApi.Filters;

public sealed class PagedRequestValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var requests = context.ActionArguments.Values.OfType<PagedRequest>().ToArray();
        foreach (var request in requests)
        {
            request.Normalize();
            QueryHelper.ValidateRequestContract(request);
        }

        await next();

        foreach (var request in requests)
        {
            if (!string.IsNullOrWhiteSpace(request.Search) && !request.SearchApplied)
            {
                throw new PagedQueryValidationException("Genel arama bu endpoint sorgusuna uygulanmadı.");
            }
        }
    }
}
