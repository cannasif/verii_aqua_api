using System;
using System.Globalization;

namespace aqua_api.Shared.Common.Helpers
{
    /// <summary>
    /// Single place for PDF unit conversion (e.g. px to pt for 96 DPI to 72 DPI).
    /// </summary>
    public static class PdfUnitConversion
    {
        /// <summary>
        /// Converts a value to points (72 DPI). px is assumed 96 DPI.
        /// </summary>
        public static decimal ToPoints(decimal value, string unit)
        {
            if (value == 0) return 0;
            return (unit ?? "px").ToLowerInvariant() switch
            {
                "px" => value * 72m / 96m,
                "pt" => value,
                "in" => value * 72m,
                "mm" => value * 72m / 25.4m,
                _ => value
            };
        }

        public static float ToPointsFloat(decimal value, string unit)
        {
            return (float)ToPoints(value, unit);
        }
    }
}
