using System;
using System.Globalization;
using EVEMon.Common.Constants;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;

namespace EVEMon.Common.Helpers
{
    public static class FormatHelper
    {

        /// <summary>
        /// Formats the given value into an abbreviated format string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <param name="truncated">if set to <c>true</c> [truncated].</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public static string Format(double value, AbbreviationFormat format, bool truncated = true, CultureInfo culture = null)
            => Format(Convert.ToDecimal(value), format, truncated, culture);

        /// <summary>
        /// Formats the given value into an abbreviated format string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <param name="truncated">if set to <c>true</c> [truncated].</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public static string Format(int value, AbbreviationFormat format, bool truncated = true, CultureInfo culture = null)
            => Format(Convert.ToDecimal(value), format, truncated, culture);

        /// <summary>
        /// Formats the given value into an abbreviated format string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <param name="truncated">if set to <c>true</c> [truncated].</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public static string Format(long value, AbbreviationFormat format, bool truncated = true, CultureInfo culture = null)
            => Format(Convert.ToDecimal(value), format, truncated, culture);

        /// <summary>
        /// Formats the given value into an abbreviated format string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <param name="truncated">if set to <c>true</c> [truncated].</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public static string Format(decimal value, AbbreviationFormat format, bool truncated = true, CultureInfo culture = null)
        {
            if (culture == null)
                culture = CultureConstants.DefaultCulture;

            decimal abs = Math.Abs(value);
            if (format == AbbreviationFormat.AbbreviationWords)
            {
                if (abs >= 1E9M)
                    return Format(" Billions", value / 1E9M, truncated, culture);
                if (abs >= 1E6M)
                    return Format(" Millions", value / 1E6M, truncated, culture);

                return abs >= 1E3M
                    ? Format(" Thousands", value / 1E3M, truncated, culture)
                    : Format(String.Empty, value, truncated, culture);
            }

            if (abs >= 1E9M)
                return Format(" B", value / 1E9M, truncated, culture);
            if (abs >= 1E6M)
                return Format(" M", value / 1E6M, truncated, culture);

            return abs >= 1E3M ? Format(" K", value / 1E3M, truncated, culture) : Format(String.Empty, value, truncated, culture);
        }

        /// <summary>
        /// Formats the given value and suffix the way we want.
        /// </summary>
        /// <param name="suffix">The suffix.</param>
        /// <param name="value">The value.</param>
        /// <param name="truncated">if set to <c>true</c> [truncated].</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        private static string Format(string suffix, decimal value, bool truncated, CultureInfo culture)
        {
            if (!truncated)
                return value.ToNumericString(2, culture) + suffix;

            // Explanations : 999.99 was displayed as 1000 because only three digits were required
            // So we do the truncation at hand for the number of digits we exactly request

            decimal abs = Math.Abs(value);
            if (abs < 1.0M)
                return ((int)value * 100 / 100M).ToString("0.##", culture) + suffix;
            if (abs < 10.0M)
                return ((int)value * 1000 / 1000M).ToString("#.##", culture) + suffix;
            if (abs < 100.0M)
                return ((int)value * 1000 / 1000M).ToString("##.#", culture) + suffix;

            return ((int)value * 1000 / 1000M).ToString("###", culture) + suffix;
        }

        /// <summary>
        /// Formats the given value into an abbreviated format string.
        /// </summary>
        /// <param name="direct">If true, ToNumericString is used instead of Format.</param>
        /// <param name="places">If direct is true, the number of decimal places used for ToNumericString.</param>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <param name="truncated">if set to <c>true</c> [truncated].</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public static string FormatIf(bool direct, int places, decimal value,
            AbbreviationFormat format, bool truncated = true, CultureInfo culture = null)
        {
            return direct ? Format(value, format, truncated, culture) : value.ToNumericString(
                places);
        }

        /// <summary>
        /// Formats the given value into an abbreviated format string.
        /// </summary>
        /// <param name="direct">If true, ToNumericString is used instead of Format.</param>
        /// <param name="places">If direct is true, the number of decimal places used for ToNumericString.</param>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <param name="truncated">if set to <c>true</c> [truncated].</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public static string FormatIf(bool direct, long value, AbbreviationFormat format,
            bool truncated = true, CultureInfo culture = null)
        {
            return direct ? Format(value, format, truncated, culture) : value.
                ToNumericString(0);
        }
    }
}
