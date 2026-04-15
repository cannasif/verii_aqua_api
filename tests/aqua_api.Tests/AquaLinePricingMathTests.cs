using aqua_api.Shared.Common.Helpers;
using Xunit;

namespace aqua_api.Tests;

public class AquaLinePricingMathTests
{
    [Fact]
    public void NormalizeGoodsReceiptLine_FeedUsdScenario_ComputesLocalAndLineTotals()
    {
        var result = AquaLinePricingMath.NormalizeGoodsReceiptLine(
            itemType: 0,
            qtyUnit: 100m,
            totalGram: null,
            fishTotalGram: null,
            currencyCode: "usd",
            exchangeRate: 38.5m,
            unitPrice: 1.75m
        );

        Assert.Equal("USD", result.CurrencyCode);
        Assert.Equal(38.5m, result.ExchangeRate);
        Assert.Equal(1.75m, result.UnitPrice);
        Assert.Equal(67.375m, result.LocalUnitPrice);
        Assert.Equal(175m, result.LineAmount);
        Assert.Equal(6737.5m, result.LocalLineAmount);
    }

    [Fact]
    public void NormalizeGoodsReceiptLine_FishTryScenario_UsesFishTotalGramAsQuantity()
    {
        var result = AquaLinePricingMath.NormalizeGoodsReceiptLine(
            itemType: 1,
            qtyUnit: null,
            totalGram: null,
            fishTotalGram: 250000m,
            currencyCode: "try",
            exchangeRate: 999m,
            unitPrice: 92m
        );

        Assert.Equal("TRY", result.CurrencyCode);
        Assert.Equal(1m, result.ExchangeRate);
        Assert.Equal(92m, result.UnitPrice);
        Assert.Equal(92m, result.LocalUnitPrice);
        Assert.Equal(23000m, result.LineAmount);
        Assert.Equal(23000m, result.LocalLineAmount);
    }

    [Fact]
    public void NormalizeShipmentLine_EurScenario_ComputesShipmentTotals()
    {
        var result = AquaLinePricingMath.NormalizeShipmentLine(
            biomassGram: 120000m,
            currencyCode: "eur",
            exchangeRate: 42m,
            unitPrice: 2.4m
        );

        Assert.Equal("EUR", result.CurrencyCode);
        Assert.Equal(42m, result.ExchangeRate);
        Assert.Equal(2.4m, result.UnitPrice);
        Assert.Equal(100.8m, result.LocalUnitPrice);
        Assert.Equal(288m, result.LineAmount);
        Assert.Equal(12096m, result.LocalLineAmount);
    }
}
