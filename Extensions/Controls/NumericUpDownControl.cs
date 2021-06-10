using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Extensions
{
    public class NumericUpDownControl : ScrollBar
    {
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(NumericUpDownControl), new PropertyMetadata(false));

        public static readonly DependencyProperty NumericUpDownButtonsVisibilityProperty = DependencyProperty.Register("NumericUpDownButtonsVisibility", typeof(Visibility), typeof(NumericUpDownControl), new PropertyMetadata(Visibility.Visible));

        static NumericUpDownControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDownControl), new FrameworkPropertyMetadata(typeof(NumericUpDownControl)));
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public Visibility NumericUpDownButtonsVisibility
        {
            get => (Visibility)GetValue(NumericUpDownButtonsVisibilityProperty);
            set => SetValue(NumericUpDownButtonsVisibilityProperty, value);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                Value++;
            }
            else if (e.Key == Key.Down)
            {
                Value--;
            }
            base.OnKeyDown(e);
        }
    }
}