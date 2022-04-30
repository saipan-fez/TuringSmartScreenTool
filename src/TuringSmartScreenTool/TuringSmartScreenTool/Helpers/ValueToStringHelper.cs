using System;

namespace TuringSmartScreenTool.Helpers
{
    public enum ThermalUnit
    {
        Celsius,
        Fahrenheit
    }

    public static class ValueToStringHelper
    {
        public static string ToFrequencyString(double hz, string format, bool includeUnitString)
        {
            string[] prefixeSI = { "", "k", "M", "G", "T", "P", "E", "Z", "Y" };

            var log10 = (int)Math.Log10(Math.Abs(hz));
            if (log10 < -27)
                return "";
            if (log10 % -3 < 0)
                log10 -= 3;
            var log1000 = log10 / 3;
            var v = (double)hz / Math.Pow(10, log1000 * 3);
            var prefix = prefixeSI[log1000];

            var unit = includeUnitString ? "Hz" : "";

            var str = string.Format($"{{0:{format}}}", v) + $" {prefix}{unit}";
            return str.TrimEnd();
        }

        public static string CelsiusToThermalString(double celsius, ThermalUnit thermalUnit, string format, bool includeUnitString)
        {
            var unit = includeUnitString ?
                thermalUnit switch
                {
                    ThermalUnit.Celsius => "℃",
                    ThermalUnit.Fahrenheit => "℉",
                    _ => throw new InvalidOperationException()
                } :
                "";
            var v = thermalUnit switch
            {
                ThermalUnit.Celsius => celsius,
                ThermalUnit.Fahrenheit => ((celsius * 9) / 5) + 32,
                _ => throw new InvalidOperationException()
            };

            var str = string.Format($"{{0:{format}}}", v) + $" {unit}";
            return str.TrimEnd();
        }
    }
}
