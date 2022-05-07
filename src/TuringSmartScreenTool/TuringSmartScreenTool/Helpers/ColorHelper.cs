using System.Globalization;
using System.Windows.Media;

namespace TuringSmartScreenTool.Helpers
{
    public static class ColorHelper
    {
        public static string ToString(Color c)
        {
            return $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        public static Color FromString(string str)
        {
            var val = int.Parse(str.Replace("#", ""), NumberStyles.HexNumber);
            var c = System.Drawing.Color.FromArgb(val);
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }
    }
}
