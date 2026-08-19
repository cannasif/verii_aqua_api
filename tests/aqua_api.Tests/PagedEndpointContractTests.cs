using aqua_api.Shared.Common.Dtos;
using aqua_api.Modules.System.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xunit;

namespace aqua_api.Tests;

public sealed class PagedEndpointContractTests : IClassFixture<AquaHttpTestWebApplicationFactory>
{
    private static readonly HashSet<string> CanonicalPagedRouteManifest = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/aqua/BatchCageBalance/paged",
        "/api/aqua/BatchMovement/paged",
        "/api/aqua/BatchWarehouseBalance/paged",
        "/api/aqua/Cage/paged",
        "/api/aqua/CageWarehouseMapping/paged",
        "/api/aqua/CageWarehouseTransfer/paged",
        "/api/aqua/CageWarehouseTransferLine/paged",
        "/api/aqua/DailyWeather/paged",
        "/api/aqua/Feeding/paged",
        "/api/aqua/FeedingDistribution/paged",
        "/api/aqua/FeedingLine/paged",
        "/api/aqua/FishBatch/paged",
        "/api/aqua/FishGrowth/paged",
        "/api/aqua/GoodsReceipt/paged",
        "/api/aqua/GoodsReceiptFishDistribution/paged",
        "/api/aqua/GoodsReceiptLine/paged",
        "/api/aqua/Mortality/paged",
        "/api/aqua/MortalityLine/paged",
        "/api/aqua/NetOperation/paged",
        "/api/aqua/NetOperationLine/paged",
        "/api/aqua/NetOperationType/paged",
        "/api/aqua/Project/paged",
        "/api/aqua/ProjectCage/paged",
        "/api/aqua/ProjectMerge/paged",
        "/api/aqua/SeaWaterTemperature/paged",
        "/api/aqua/Shipment/paged",
        "/api/aqua/ShipmentLine/paged",
        "/api/aqua/StockConvert/paged",
        "/api/aqua/StockConvertLine/paged",
        "/api/aqua/Transfer/paged",
        "/api/aqua/TransferLine/paged",
        "/api/aqua/Warehouse/paged",
        "/api/aqua/WarehouseCageTransfer/paged",
        "/api/aqua/WarehouseCageTransferLine/paged",
        "/api/aqua/WarehouseTransfer/paged",
        "/api/aqua/WarehouseTransferLine/paged",
        "/api/aqua/WeatherSeverity/paged",
        "/api/aqua/WeatherType/paged",
        "/api/aqua/Weighing/paged",
        "/api/aqua/WeighingLine/paged",
        "/api/budget/CalibrationDefinition/paged",
        "/api/budget/FeedConsumptionRate/feed-stocks/paged",
        "/api/budget/FeedConsumptionRate/paged",
        "/api/budget/FeedMortalityRate/paged",
        "/api/budget/FishGrowthProfile/paged",
        "/api/budget/FishGrowthQuality/paged",
        "/api/budget/Planning/mortality-rates/paged",
        "/api/budget/Planning/paged",
        "/api/budget/WaterTemperature/paged",
        "/api/BudgetCalibrationDefinition/paged",
        "/api/BudgetFeedConsumptionRate/feed-stocks/paged",
        "/api/BudgetFeedConsumptionRate/paged",
        "/api/BudgetFishGrowthProfile/paged",
        "/api/BudgetWaterTemperature/paged",
        "/api/CurrentDirection/paged",
        "/api/CurrentDirectionMatch/paged",
        "/api/NetInventoryMovement/paged",
        "/api/erp-receipt-shipment-movements/paged",
        "/api/permission-definitions/paged",
        "/api/permission-groups/paged",
        "/api/SeaWaterTemperature/paged",
        "/api/Stock/paged",
        "/api/Stock/withImages/paged",
        "/api/StockDetail/paged",
        "/api/User/paged",
        "/api/UserAuthority/paged",
        "/api/UserDetail/paged",
        "/api/WindDirection/paged",
        "/api/WindDirectionMatch/paged",
    };

    private readonly AquaHttpTestWebApplicationFactory _factory;

    public PagedEndpointContractTests(AquaHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void NetsisFunctionReadController_DoesNotExposePagedActions()
    {
        _ = _factory.CreateClient();
        var pagedActions = _factory.Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>())
            .Where(action => action?.ControllerTypeInfo.AsType() == typeof(Modules.Integrations.Api.NetsisReadController))
            .Where(action => action!.Parameters.Any(parameter =>
                typeof(PagedRequest).IsAssignableFrom(parameter.ParameterType)))
            .ToList();

        Assert.Empty(pagedActions);
    }

    [Fact]
    public void EveryPagedAction_IsPostOnlyCanonicalRouteWithBodyBinding()
    {
        _ = _factory.CreateClient();
        var endpoints = _factory.Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Select(endpoint => new
            {
                Endpoint = endpoint,
                Action = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>(),
                Methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? []
            })
            .Where(item => item.Action?.Parameters.Any(parameter =>
                typeof(PagedRequest).IsAssignableFrom(parameter.ParameterType)) == true)
            .ToList();

        Assert.NotEmpty(endpoints);
        foreach (var endpoint in endpoints)
        {
            Assert.Contains(HttpMethods.Post, endpoint.Methods, StringComparer.OrdinalIgnoreCase);
            Assert.DoesNotContain(HttpMethods.Get, endpoint.Methods, StringComparer.OrdinalIgnoreCase);
            Assert.EndsWith(
                "/paged",
                endpoint.Endpoint.RoutePattern.RawText?.TrimEnd('/'),
                StringComparison.OrdinalIgnoreCase);

            var pagedParameters = endpoint.Action!.MethodInfo.GetParameters()
                .Where(parameter => typeof(PagedRequest).IsAssignableFrom(parameter.ParameterType));
            Assert.All(pagedParameters, parameter =>
                Assert.NotNull(parameter.GetCustomAttribute<FromBodyAttribute>()));
        }
    }

    [Fact]
    public void CanonicalPagedRouteManifest_CoversEveryPagedAction()
    {
        _ = _factory.CreateClient();
        var actual = _factory.Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods
                .Contains(HttpMethods.Post, StringComparer.OrdinalIgnoreCase) == true)
            .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.Parameters
                .Any(parameter => typeof(PagedRequest).IsAssignableFrom(parameter.ParameterType)) == true)
            .Select(endpoint => "/" + (endpoint.RoutePattern.RawText ?? string.Empty).Trim('/'))
            .Where(route => route.EndsWith("/paged", StringComparison.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = actual.Except(CanonicalPagedRouteManifest).OrderBy(route => route).ToArray();
        var stale = CanonicalPagedRouteManifest.Except(actual).OrderBy(route => route).ToArray();

        Assert.True(
            missing.Length == 0 && stale.Length == 0,
            $"Paged route manifest uyuşmuyor. Eksik:\n{string.Join("\n", missing)}\nEski/fazla:\n{string.Join("\n", stale)}");
    }

    [Fact]
    public void HangfirePagedEndpoints_ArePostOnlyBodyRoutes()
    {
        _ = _factory.CreateClient();
        var endpoints = _factory.Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Select(endpoint => new
            {
                Endpoint = endpoint,
                Action = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>(),
                Methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? []
            })
            .Where(item => item.Action?.Parameters.Any(parameter =>
                parameter.ParameterType == typeof(HangfirePagedRequest)) == true)
            .ToList();

        Assert.Equal(4, endpoints.Count);
        Assert.All(endpoints, endpoint =>
        {
            Assert.Contains(HttpMethods.Post, endpoint.Methods, StringComparer.OrdinalIgnoreCase);
            Assert.DoesNotContain(HttpMethods.Get, endpoint.Methods, StringComparer.OrdinalIgnoreCase);
            Assert.EndsWith(
                "/paged",
                endpoint.Endpoint.RoutePattern.RawText?.TrimEnd('/'),
                StringComparison.OrdinalIgnoreCase);
            Assert.NotNull(endpoint.Action!.MethodInfo.GetParameters()
                .Single(parameter => parameter.ParameterType == typeof(HangfirePagedRequest))
                .GetCustomAttribute<FromBodyAttribute>());
        });
    }
}
