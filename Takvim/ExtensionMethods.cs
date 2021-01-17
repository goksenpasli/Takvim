using System.Windows.Media;
namespace Takvim
{
    internal static class ExtensionMethods
    {
        public static Brush ConvertToBrush(this System.Drawing.Color color) => new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));

        public static System.Drawing.Color ConvertToColor(this Brush color)
        {
            var sb = (SolidColorBrush)color;
            return System.Drawing.Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B);
        }
    }
}
