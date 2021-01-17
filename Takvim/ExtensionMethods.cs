using System.Windows.Media;
namespace Takvim
{
    internal static class ExtensionMethods
    {
        public static Brush ConvertToBrush(this System.Drawing.Color color) => new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));

        public static System.Drawing.Color ConvertToColor(this Brush color)
        {
            var t = (SolidColorBrush)color;
            return System.Drawing.Color.FromArgb(t.Color.A, t.Color.R, t.Color.G, t.Color.B);
        }
    }
}
