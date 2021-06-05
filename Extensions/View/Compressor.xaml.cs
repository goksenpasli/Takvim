using System.Windows;
using System.Windows.Controls;

namespace Extensions
{
    /// <summary>
    /// Interaction logic for Compressor.xaml
    /// </summary>
    public partial class Compressor : UserControl
    {
        public static readonly DependencyProperty MiniViewProperty = DependencyProperty.Register("MiniView", typeof(bool), typeof(Compressor), new PropertyMetadata(false, Changed));

        public Compressor()
        {
            InitializeComponent();
            DataContext = new CompressorViewModel();
        }

        public bool MiniView
        {
            get => (bool)GetValue(MiniViewProperty);
            set => SetValue(MiniViewProperty, value);
        }

        private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Compressor compressor && (bool)e.NewValue && compressor.DataContext is CompressorViewModel compressorViewModel)
            {
                compressorViewModel.ElementVisible = Visibility.Collapsed;
                compressor.Width = 90;
            }
        }
    }
}