using System.Reflection;
using aqua_api.Modules.Budget.Domain.Entities;
using aqua_api.Modules.BudgetPlanning.Application.Services;
using Xunit;

namespace aqua_api.Tests;

public class BudgetPlanningCalculationTests
{
    [Fact]
    public void CalibrationSelection_UsesCalibrationInfoBeforeCode()
    {
        var calibrations = new List<BudgetCalibrationDefinition>
        {
            new() { Id = 1, CalibrationCode = "K-1", CalibrationInfo = "0 - 200 gr" },
            new() { Id = 2, CalibrationCode = "K-2", CalibrationInfo = "200 - 500 gr" },
            new() { Id = 3, CalibrationCode = "K-3", CalibrationInfo = "500 gr ve üzeri" }
        };

        var selected = InvokePrivateStatic<BudgetCalibrationDefinition?>(
            "FindCalibration",
            calibrations,
            674.796m);

        Assert.NotNull(selected);
        Assert.Equal(3, selected!.Id);
    }

    [Fact]
    public void FeedRateSelection_RequiresExactTemperatureAndCalibration()
    {
        var rates = new List<BudgetFeedConsumptionRate>
        {
            new() { Id = 1, WaterTemperatureId = 1, CalibrationDefinitionId = 1, FeedStockId = 10, FeedAmount = 0.8m },
            new() { Id = 2, WaterTemperatureId = 1, CalibrationDefinitionId = 3, FeedStockId = 10, FeedAmount = 1.6m }
        };

        var missingCalibration = InvokePrivateStatic<BudgetFeedConsumptionRate?>(
            "FindFeedRate",
            rates,
            1L,
            null);
        var exactRate = InvokePrivateStatic<BudgetFeedConsumptionRate?>(
            "FindFeedRate",
            rates,
            1L,
            3L);

        Assert.Null(missingCalibration);
        Assert.NotNull(exactRate);
        Assert.Equal(2, exactRate!.Id);
    }

    [Fact]
    public void WaterTemperatureSelection_FallsBackToMonthWhenYearSpecificValueIsMissing()
    {
        var temperatures = new List<BudgetWaterTemperature>
        {
            new() { Id = 1, Year = 2026, Month = 1, WaterTemperatureCelsius = 12m },
            new() { Id = 2, Year = 2028, Month = 1, WaterTemperatureCelsius = 14m }
        };

        var fallback = InvokePrivateStatic<BudgetWaterTemperature?>(
            "FindWaterTemperature",
            temperatures,
            2027,
            1);
        var exact = InvokePrivateStatic<BudgetWaterTemperature?>(
            "FindWaterTemperature",
            temperatures,
            2028,
            1);

        Assert.NotNull(fallback);
        Assert.Equal(1, fallback!.Id);
        Assert.NotNull(exact);
        Assert.Equal(2, exact!.Id);
    }

    private static T InvokePrivateStatic<T>(string methodName, params object?[] args)
    {
        var method = typeof(BudgetPlanningService).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (T)method!.Invoke(null, args)!;
    }
}
