using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Takvim
{
    internal class ShadowedImage : Image
    {
        public static readonly DependencyProperty ShowShadowProperty = DependencyProperty.Register("ShowShadow", typeof(bool), typeof(ShadowedImage), new PropertyMetadata(false));

        private static readonly SolidColorBrush brush = new(Color.FromArgb(70, 128, 128, 128));

        public bool ShowShadow
        {
            get => (bool)GetValue(ShowShadowProperty);
            set => SetValue(ShowShadowProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (ShowShadow)
            {
                dc.DrawRectangle(brush, null, new Rect(new Point(2.5, 2.5), new Size(ActualWidth, ActualHeight)));
            }
            base.OnRender(dc);
        }
    }
}