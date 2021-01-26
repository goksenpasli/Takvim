using System.Windows;
using System.Windows.Controls;

namespace Takvim
{
    public class FocusWhenSelectedBehavior
    {
        public static readonly DependencyProperty FocusSelectedProperty = DependencyProperty.RegisterAttached("FocusSelected", typeof(bool), typeof(FocusWhenSelectedBehavior), new PropertyMetadata(false, new PropertyChangedCallback(OnFocusWhenSelectedChanged)));

        public static bool GetFocusSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(FocusSelectedProperty);
        }

        public static void SetFocusSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(FocusSelectedProperty, value);
        }
        private static void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            ((ListBoxItem)sender).Focus();
        }

        private static void OnFocusWhenSelectedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ListBoxItem listBoxItem)
            {
                if ((bool)args.NewValue)
                {
                    listBoxItem.Selected += ListBoxItem_Selected;
                }
                else
                {
                    listBoxItem.Selected -= ListBoxItem_Selected;
                }
            }
        }
    }
}
