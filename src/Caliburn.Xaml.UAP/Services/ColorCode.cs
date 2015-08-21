using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Windows.UI;

namespace Caliburn.Light
{
    /// <summary>
    /// Helper to convert a color code to a <see cref="Color"/>.
    /// </summary>
    public static class ColorCode
    {
        /// <summary>
        /// Converts a HEX color code to a <see cref="Color"/>.
        /// </summary>
        /// <param name="hexValue">The hexadecimal value.</param>
        /// <returns>The color.</returns>
        public static Color ToColor(string hexValue)
        {
            if (!hexValue.Contains("#")) // might be a named color
            {
                var colorProperty =
                    typeof (Colors).GetRuntimeProperties()
                        .FirstOrDefault(p => string.Compare(p.Name, hexValue, StringComparison.OrdinalIgnoreCase) == 0);
                if (colorProperty == null)
                    throw new ArgumentException("This is not a known color name. Use a proper hex color number.", "hexValue");

                return (Color)colorProperty.GetValue(null);
            }

            hexValue = hexValue.Replace("#", string.Empty);
            if (hexValue.Length < 6)
                throw new ArgumentException("This does not appear to be a proper hex color number", "hexValue");

            byte a = 255;
            var startPosition = 0;

            // the case where alpha is provided
            if (hexValue.Length == 8)
            {
                a = byte.Parse(hexValue.Substring(0, 2), NumberStyles.HexNumber);
                startPosition = 2;
            }

            var r = byte.Parse(hexValue.Substring(startPosition, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hexValue.Substring(startPosition + 2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hexValue.Substring(startPosition + 4, 2), NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }
    }
}
