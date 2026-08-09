using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace aqua_api.Tests;

public sealed class DashboardSoftDeleteGuardTests
{
    private static readonly Type[] DashboardEntityTypes =
    [
        typeof(Project),
        typeof(ProjectCage),
        typeof(Cage),
        typeof(Feeding),
        typeof(FeedingLine),
        typeof(FeedingDistribution),
        typeof(Mortality),
        typeof(MortalityLine),
        typeof(BatchCageBalance),
        typeof(BatchWarehouseBalance),
        typeof(DailyWeather),
        typeof(aqua_api.Modules.Weather.Domain.Entities.WeatherType),
        typeof(aqua_api.Modules.Weather.Domain.Entities.WeatherSeverity),
        typeof(NetOperation),
        typeof(NetOperationType),
        typeof(NetOperationLine),
        typeof(Transfer),
        typeof(TransferLine),
        typeof(Shipment),
        typeof(ShipmentLine),
        typeof(Weighing),
        typeof(WeighingLine),
        typeof(StockConvert),
        typeof(StockConvertLine),
        typeof(BatchMovement),
        typeof(FishBatch),
        typeof(aqua_api.Modules.Stock.Domain.Entities.Stock),
        typeof(aqua_api.Modules.Warehouse.Domain.Entities.Warehouse),
    ];

    [Fact]
    public void DashboardReadSources_MustKeepSoftDeleteQueryFilters()
    {
        var options = new DbContextOptionsBuilder<AquaDbContext>()
            .UseSqlServer("Server=(local);Database=DashboardModelOnly;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        using var db = new AquaDbContext(options);

        var missingFilters = DashboardEntityTypes
            .Where(entityType => db.Model.FindEntityType(entityType)?.GetQueryFilter() == null)
            .Select(entityType => entityType.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        Assert.True(
            missingFilters.Count == 0,
            "Every dashboard read source must exclude soft-deleted rows with an EF query filter. Missing: " +
            string.Join(", ", missingFilters));
    }
}
