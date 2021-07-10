﻿using System.Windows;
using System.Windows.Controls;

namespace Extensions
{
    public class ContentSlider : Slider
    {
        public static readonly DependencyProperty OverContentProperty = DependencyProperty.Register("OverContent", typeof(object), typeof(ContentSlider), new PropertyMetadata(null));

        static ContentSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentSlider), new FrameworkPropertyMetadata(typeof(ContentSlider)));
        }

        public object OverContent
        {
            get => GetValue(OverContentProperty);
            set => SetValue(OverContentProperty, value);
        }

        public override string ToString()
        {
            return OverContent.ToString();
        }
    }
}