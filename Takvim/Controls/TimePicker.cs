using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Takvim
{
    public class TimePicker : ComboBox
    {
        public static readonly DependencyProperty ClearButtonVisibleProperty = DependencyProperty.Register("ClearButtonVisible", typeof(Visibility), typeof(TimePicker), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register("Interval", typeof(int), typeof(TimePicker), new PropertyMetadata(30, IntervalChanged), new ValidateValueCallback(IsValidInterval));

        public static readonly DependencyProperty TimeValueProperty = DependencyProperty.Register("TimeValue", typeof(string), typeof(TimePicker), new PropertyMetadata(null));

        static TimePicker() => DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));

        public TimePicker() => GenerateTime(Interval);

        public static IList<string> Saatler { get; set; }

        public Visibility ClearButtonVisible
        {
            get => (Visibility)GetValue(ClearButtonVisibleProperty);
            set => SetValue(ClearButtonVisibleProperty, value);
        }

        public int Interval
        {
            get => (int)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
        }

        public string TimeValue
        {
            get => (string)GetValue(TimeValueProperty);
            set => SetValue(TimeValueProperty, value);
        }

        private static void GenerateTime(int interval)
        {
            Saatler = new List<string>();
            for (DateTime i = DateTime.Today; i < DateTime.Today.AddDays(1); i = i.AddMinutes(interval))
            {
                Saatler.Add(i.ToShortTimeString());
            }
        }

        private static void IntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => GenerateTime(Convert.ToInt32(e.NewValue));

        private static bool IsValidInterval(object value)
        {
            int v = (int)value;
            return v is > 0 and <= 1440;
        }
    }
}