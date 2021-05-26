using System.Windows;
using System.Windows.Controls.Primitives;

namespace Takvim
{
    public class NumericUpDownControl : ScrollBar
    {
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(NumericUpDownControl), new PropertyMetadata(false));

        static NumericUpDownControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDownControl), new FrameworkPropertyMetadata(typeof(NumericUpDownControl)));
    }
}
