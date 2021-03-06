using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CatenaLogic.Windows.Presentation.WebcamPlayer;

namespace Takvim
{
    public partial class CameraUserControl : UserControl
    {
        public CameraUserControl()
        {
            InitializeComponent();
            DataContext = this;
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                FilterInfo[] camMonikers = CapDevice.DeviceMonikers;
                if (camMonikers.Length > 0)
                {
                    Player.Device = new CapDevice(camMonikers[0].MonikerString)
                    {
                        MaxHeightInPixels = 1080
                    };
                }
            }

            ResimYükle = new RelayCommand<object>(parameter =>
            {
                if (parameter is Data data)
                {
                    using MemoryStream ms = new();
                    JpegBitmapEncoder encoder = new();
                    encoder.Frames.Add(BitmapFrame.Create(new TransformedBitmap(Player.Source as InteropBitmap, new RotateTransform(Player.Rotation))));
                    encoder.QualityLevel = 80;
                    encoder.Save(ms);
                    data.ResimData = ms.ToArray().WebpEncode(data.WebpQuality);
                    data.ResimUzantı = ".webp";
                    data.Boyut = data.ResimData.Length / 1024;
                }
            }, parameter => true);
        }
        public ICommand ResimYükle { get; }
    }
}
