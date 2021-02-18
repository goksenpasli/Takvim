using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Takvim
{
    public class TimePicker : Control
    {
        public List<string> Saatler { get; set; } = new List<string>();

        public TimePicker()
        {
            for (DateTime i = DateTime.Today; i < DateTime.Today.AddDays(1); i = i.AddMinutes(30))
            {
                Saatler.Add(i.ToShortTimeString());
            }
        }

        public string TimeValue
        {
            get { return (string)GetValue(TimeValueProperty); }
            set { SetValue(TimeValueProperty, value); }
        }
        public static readonly DependencyProperty TimeValueProperty = DependencyProperty.Register("TimeValue", typeof(string), typeof(TimePicker), new PropertyMetadata(null));

        static TimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
        }
    }
}
