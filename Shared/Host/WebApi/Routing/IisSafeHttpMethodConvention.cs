using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;

namespace aqua_api.Shared.Host.WebApi.Routing;

/// <summary>
/// Adds POST-compatible aliases for PUT/DELETE endpoints so IIS deployments with
/// restrictive verb filtering can still reach the ASP.NET Core pipeline.
/// </summary>
public sealed class IisSafeHttpMethodConvention : IActionModelConvention
{
    private const string PostMethod = "POST";
    private const string PutMethod = "PUT";
    private const string DeleteMethod = "DELETE";

    public void Apply(ActionModel action)
    {
        var selectors = action.Selectors.ToArray();

        foreach (var selector in selectors)
        {
            var methods = GetHttpMethods(selector);
            if (methods.Contains(PutMethod, StringComparer.OrdinalIgnoreCase))
            {
                AddPostAliasIfMissing(action, selector, BuildPutAliasTemplate(selector));
            }

            if (methods.Contains(DeleteMethod, StringComparer.OrdinalIgnoreCase))
            {
                AddPostAliasIfMissing(action, selector, BuildDeleteAliasTemplate(selector));
            }
        }
    }

    private static IReadOnlyList<string> GetHttpMethods(SelectorModel selector)
    {
        return selector.ActionConstraints
            .OfType<HttpMethodActionConstraint>()
            .SelectMany(constraint => constraint.HttpMethods)
            .ToArray();
    }

    private static string? GetTemplate(SelectorModel selector)
    {
        return selector.AttributeRouteModel?.Template;
    }

    private static string? BuildPutAliasTemplate(SelectorModel selector)
    {
        var template = GetTemplate(selector)?.Trim('/');
        return string.IsNullOrWhiteSpace(template) ? "update" : $"{template}/update";
    }

    private static string BuildDeleteAliasTemplate(SelectorModel selector)
    {
        var template = GetTemplate(selector)?.Trim('/');
        return string.IsNullOrWhiteSpace(template) ? "delete" : $"{template}/delete";
    }

    private static void AddPostAliasIfMissing(ActionModel action, SelectorModel sourceSelector, string? aliasTemplate)
    {
        var aliasTemplateKey = aliasTemplate ?? string.Empty;
        var alreadyExists = action.Selectors.Any(selector =>
        {
            var template = GetTemplate(selector) ?? string.Empty;
            return string.Equals(template, aliasTemplateKey, StringComparison.OrdinalIgnoreCase) &&
                   GetHttpMethods(selector).Contains(PostMethod, StringComparer.OrdinalIgnoreCase);
        });

        if (alreadyExists)
        {
            return;
        }

        var aliasSelector = new SelectorModel
        {
            AttributeRouteModel = sourceSelector.AttributeRouteModel == null
                ? null
                : new AttributeRouteModel(sourceSelector.AttributeRouteModel)
                {
                    Template = aliasTemplate,
                    Name = null
                }
        };

        aliasSelector.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { PostMethod }));
        aliasSelector.EndpointMetadata.Add(new HttpMethodMetadata(new[] { PostMethod }));

        action.Selectors.Add(aliasSelector);
    }
}
