using System.Windows.Controls;

namespace Takvim
{
    /// <summary>
    /// Interaction logic for Compressor.xaml
    /// </summary>
    public partial class Compressor : UserControl
    {
        public Compressor()
        {
            InitializeComponent();
            DataContext = new ZipViewModel();
        }
    }
}