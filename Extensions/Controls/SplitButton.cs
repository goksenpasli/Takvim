using System.Windows;
using System.Windows.Controls;

namespace Extensions
{
    public class SplitButton : Button
    {
        public static readonly DependencyProperty InternalContentProperty = DependencyProperty.Register("InternalContent", typeof(object), typeof(SplitButton), new PropertyMetadata(null));

        public static readonly DependencyProperty StayOpenProperty = DependencyProperty.Register("StayOpen", typeof(bool), typeof(SplitButton), new PropertyMetadata(false));

        public object InternalContent
        {
            get => GetValue(InternalContentProperty);
            set => SetValue(InternalContentProperty, value);
        }

        public bool StayOpen
        {
            get => (bool)GetValue(StayOpenProperty);
            set => SetValue(StayOpenProperty, value);
        }

        static SplitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
        }
    }
}