using System.Windows;
using System.Windows.Controls;

namespace Takvim
{
    public class SplitButton : Button
    {
        public static readonly DependencyProperty InternalContentProperty = DependencyProperty.Register("InternalContent", typeof(object), typeof(SplitButton), new PropertyMetadata(null));

        public object InternalContent
        {
            get => GetValue(InternalContentProperty);
            set => SetValue(InternalContentProperty, value);
        }
    }
}