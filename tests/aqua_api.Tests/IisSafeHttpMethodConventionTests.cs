using aqua_api.Shared.Host.WebApi.Routing;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using Xunit;

namespace aqua_api.Tests;

public sealed class IisSafeHttpMethodConventionTests
{
    [Fact]
    public void Apply_AddsPostUpdateAliasForPutRoutes()
    {
        var action = CreateAction("{id:long}", "PUT");

        new IisSafeHttpMethodConvention().Apply(action);

        Assert.Contains(action.Selectors, selector =>
            string.Equals(selector.AttributeRouteModel?.Template, "{id:long}/update", StringComparison.OrdinalIgnoreCase) &&
            HasMethod(selector, "POST"));
    }

    [Fact]
    public void Apply_AddsPostDeleteAliasForDeleteRoutes()
    {
        var action = CreateAction("{id:long}", "DELETE");

        new IisSafeHttpMethodConvention().Apply(action);

        Assert.Contains(action.Selectors, selector =>
            string.Equals(selector.AttributeRouteModel?.Template, "{id:long}/delete", StringComparison.OrdinalIgnoreCase) &&
            HasMethod(selector, "POST"));
    }

    private static ActionModel CreateAction(string template, string method)
    {
        var selector = new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel
            {
                Template = template
            }
        };
        selector.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { method }));

        var dummyAction = typeof(IisSafeHttpMethodConventionTests)
            .GetMethod(nameof(DummyAction), BindingFlags.Instance | BindingFlags.NonPublic)!;
        var action = new ActionModel(dummyAction, Array.Empty<object>())
        {
            ActionName = nameof(DummyAction)
        };
        action.Selectors.Add(selector);

        return action;
    }

    private static bool HasMethod(SelectorModel selector, string method)
    {
        return selector.ActionConstraints
            .OfType<HttpMethodActionConstraint>()
            .SelectMany(constraint => constraint.HttpMethods)
            .Contains(method, StringComparer.OrdinalIgnoreCase);
    }

    private void DummyAction()
    {
    }
}
