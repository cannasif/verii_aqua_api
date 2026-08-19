using aqua_api.Shared.Common.Dtos;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace aqua_api.Shared.Host.WebApi.Routing;

public sealed class PagedPostRouteConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                if (!action.Parameters.Any(parameter =>
                        typeof(PagedRequest).IsAssignableFrom(parameter.ParameterInfo.ParameterType)))
                {
                    continue;
                }

                var getSelectors = action.Selectors
                    .Where(IsGetSelector)
                    .ToArray();
                foreach (var selector in getSelectors)
                {
                    var postSelector = new SelectorModel(selector);
                    for (var index = postSelector.ActionConstraints.Count - 1; index >= 0; index--)
                    {
                        if (postSelector.ActionConstraints[index] is HttpMethodActionConstraint)
                        {
                            postSelector.ActionConstraints.RemoveAt(index);
                        }
                    }

                    for (var index = postSelector.EndpointMetadata.Count - 1; index >= 0; index--)
                    {
                        if (postSelector.EndpointMetadata[index] is IActionHttpMethodProvider)
                        {
                            postSelector.EndpointMetadata.RemoveAt(index);
                        }
                    }

                    postSelector.ActionConstraints.Add(new HttpMethodActionConstraint([HttpMethods.Post]));
                    postSelector.EndpointMetadata.Add(new HttpMethodMetadata([HttpMethods.Post]));
                    var template = selector.AttributeRouteModel?.Template?.Trim('/');
                    if (string.IsNullOrWhiteSpace(template))
                    {
                        template = "paged";
                    }
                    else if (!template.EndsWith("/paged", StringComparison.OrdinalIgnoreCase)
                             && !template.Equals("paged", StringComparison.OrdinalIgnoreCase))
                    {
                        template = $"{template}/paged";
                    }

                    postSelector.AttributeRouteModel = new AttributeRouteModel
                    {
                        Template = template,
                        Name = selector.AttributeRouteModel?.Name,
                        Order = selector.AttributeRouteModel?.Order,
                        SuppressLinkGeneration = selector.AttributeRouteModel?.SuppressLinkGeneration ?? false,
                        SuppressPathMatching = selector.AttributeRouteModel?.SuppressPathMatching ?? false
                    };
                    action.Selectors.Add(postSelector);
                }
            }
        }
    }

    private static bool IsGetSelector(SelectorModel selector) =>
        selector.ActionConstraints
            .OfType<HttpMethodActionConstraint>()
            .Any(constraint => constraint.HttpMethods.Contains(HttpMethods.Get, StringComparer.OrdinalIgnoreCase));
}
