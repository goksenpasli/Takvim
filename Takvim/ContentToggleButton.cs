using System.Windows;
using System.Windows.Controls.Primitives;

namespace Takvim
{
    public class ContentToggleButton : ToggleButton
    {
        public static readonly DependencyProperty StaysOpenProperty = DependencyProperty.Register("StaysOpen", typeof(bool), typeof(ContentToggleButton), new PropertyMetadata(false));

        public static readonly DependencyProperty OverContentProperty = DependencyProperty.Register("OverContent", typeof(object), typeof(ContentToggleButton), new PropertyMetadata(null));

        public bool StaysOpen
        {
            get { return (bool)GetValue(StaysOpenProperty); }
            set { SetValue(StaysOpenProperty, value); }
        }

        public object OverContent
        {
            get => GetValue(OverContentProperty);
            set => SetValue(OverContentProperty, value);
        }

        public override string ToString() => OverContent.ToString();
    }
}