using System.Windows;
using System.Windows.Controls.Primitives;

namespace Takvim
{
    public class ContentToggleButton : ToggleButton
    {
        public object OverContent
        {
            get => GetValue(OverContentProperty);
            set => SetValue(OverContentProperty, value);
        }

        public static readonly DependencyProperty OverContentProperty = DependencyProperty.Register("OverContent", typeof(object), typeof(ContentToggleButton), new PropertyMetadata(null));

        public override string ToString() => OverContent.ToString();
    }
}