namespace aqua_api.Shared.Common.Helpers;

public sealed record AquaPricingNormalizationResult(
    string CurrencyCode,
    decimal ExchangeRate,
    decimal? UnitPrice,
    decimal? LocalUnitPrice,
    decimal? LineAmount,
    decimal? LocalLineAmount
);

public static class AquaLinePricingMath
{
    private static decimal RoundFinancial(decimal value) => Math.Round(value, 6, MidpointRounding.AwayFromZero);

    public static AquaPricingNormalizationResult NormalizeGoodsReceiptLine(
        byte itemType,
        decimal? qtyUnit,
        decimal? totalGram,
        decimal? fishTotalGram,
        string? currencyCode,
        decimal? exchangeRate,
        decimal? unitPrice
    )
    {
        var normalizedCurrency = string.IsNullOrWhiteSpace(currencyCode) ? "TRY" : currencyCode.Trim().ToUpperInvariant();
        var normalizedExchangeRate = normalizedCurrency == "TRY" ? 1m : Math.Max(exchangeRate ?? 0, 0);
        normalizedExchangeRate = normalizedExchangeRate <= 0 ? 1m : RoundFinancial(normalizedExchangeRate);

        var normalizedUnitPrice = Math.Max(unitPrice ?? 0, 0);
        decimal? roundedUnitPrice = normalizedUnitPrice > 0 ? RoundFinancial(normalizedUnitPrice) : null;
        decimal? localUnitPrice = roundedUnitPrice.HasValue ? RoundFinancial(roundedUnitPrice.Value * normalizedExchangeRate) : null;

        var quantityKg = ResolveGoodsReceiptQuantityKg(itemType, qtyUnit, totalGram, fishTotalGram);
        decimal? lineAmount = roundedUnitPrice.HasValue && quantityKg > 0 ? RoundFinancial(roundedUnitPrice.Value * quantityKg) : null;
        decimal? localLineAmount = lineAmount.HasValue ? RoundFinancial(lineAmount.Value * normalizedExchangeRate) : null;

        return new AquaPricingNormalizationResult(
            normalizedCurrency,
            normalizedExchangeRate,
            roundedUnitPrice,
            localUnitPrice,
            lineAmount,
            localLineAmount
        );
    }

    public static AquaPricingNormalizationResult NormalizeShipmentLine(
        decimal biomassGram,
        string? currencyCode,
        decimal? exchangeRate,
        decimal? unitPrice
    )
    {
        var normalizedCurrency = string.IsNullOrWhiteSpace(currencyCode) ? "TRY" : currencyCode.Trim().ToUpperInvariant();
        var normalizedExchangeRate = normalizedCurrency == "TRY" ? 1m : Math.Max(exchangeRate ?? 0, 0);
        normalizedExchangeRate = normalizedExchangeRate <= 0 ? 1m : RoundFinancial(normalizedExchangeRate);

        var normalizedUnitPrice = Math.Max(unitPrice ?? 0, 0);
        decimal? roundedUnitPrice = normalizedUnitPrice > 0 ? RoundFinancial(normalizedUnitPrice) : null;
        decimal? localUnitPrice = roundedUnitPrice.HasValue ? RoundFinancial(roundedUnitPrice.Value * normalizedExchangeRate) : null;

        var quantityKg = biomassGram > 0 ? biomassGram / 1000m : 0m;
        decimal? lineAmount = roundedUnitPrice.HasValue && quantityKg > 0 ? RoundFinancial(roundedUnitPrice.Value * quantityKg) : null;
        decimal? localLineAmount = lineAmount.HasValue ? RoundFinancial(lineAmount.Value * normalizedExchangeRate) : null;

        return new AquaPricingNormalizationResult(
            normalizedCurrency,
            normalizedExchangeRate,
            roundedUnitPrice,
            localUnitPrice,
            lineAmount,
            localLineAmount
        );
    }

    private static decimal ResolveGoodsReceiptQuantityKg(
        byte itemType,
        decimal? qtyUnit,
        decimal? totalGram,
        decimal? fishTotalGram
    )
    {
        const byte feedItemType = 0;

        if (itemType == feedItemType)
        {
            if ((qtyUnit ?? 0) > 0) return qtyUnit ?? 0;
            if ((totalGram ?? 0) > 0) return (totalGram ?? 0) / 1000m;
            return 0;
        }

        if ((fishTotalGram ?? 0) > 0) return (fishTotalGram ?? 0) / 1000m;
        if ((totalGram ?? 0) > 0) return (totalGram ?? 0) / 1000m;
        return 0;
    }
}
