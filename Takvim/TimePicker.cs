using System.Windows;
using System.Windows.Controls;

namespace Takvim
{
    public class TimePicker : ComboBox
    {
        public static readonly DependencyProperty TimeValueProperty = DependencyProperty.Register("TimeValue", typeof(string), typeof(TimePicker), new PropertyMetadata(null));

        public Visibility ClearButtonVisible
        {
            get => (Visibility)GetValue(ClearButtonVisibleProperty);
            set => SetValue(ClearButtonVisibleProperty, value);
        }

        public static readonly DependencyProperty ClearButtonVisibleProperty = DependencyProperty.Register("ClearButtonVisible", typeof(Visibility), typeof(TimePicker), new PropertyMetadata(Visibility.Collapsed));

        static TimePicker() => DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));

        public string TimeValue
        {
            get => (string)GetValue(TimeValueProperty);
            set => SetValue(TimeValueProperty, value);
        }
    }
}
