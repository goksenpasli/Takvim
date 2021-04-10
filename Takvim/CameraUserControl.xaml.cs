using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CatenaLogic.Windows.Presentation.WebcamPlayer;

namespace Takvim
{
    public partial class CameraUserControl : UserControl, INotifyPropertyChanged
    {
        private FilterInfo[] liste = CapDevice.DeviceMonikers;

        private FilterInfo seçiliKamera;

        public CameraUserControl()
        {
            InitializeComponent();
            DataContext = this;

            ResimYükle = new RelayCommand<object>(parameter =>
            {
                if (parameter is Data data)
                {
                    using MemoryStream ms = new();
                    JpegBitmapEncoder encoder = new();
                    encoder.Frames.Add(BitmapFrame.Create(new TransformedBitmap(Player.Device.BitmapSource, new RotateTransform(Player.Rotation))));
                    encoder.QualityLevel = 100;
                    encoder.Save(ms);
                    data.ResimData = ms.ToArray().WebpEncode(data.WebpQuality);
                    data.DosyaUzantı = ".webp";
                    data.Boyut = data.ResimData.Length / 1024;
                }
            }, parameter => SeçiliKamera is not null);

            Durdur = new RelayCommand<object>(parameter => Player.Device.Stop(), parameter => SeçiliKamera is not null && Player.Device.IsRunning);

            Oynat = new RelayCommand<object>(parameter => Player.Device.Start(), parameter => SeçiliKamera is not null && !Player.Device.IsRunning);

            PropertyChanged += CameraUserControl_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FilterInfo[] Liste
        {
            get => liste;

            set
            {
                if (liste != value)
                {
                    liste = value;
                    OnPropertyChanged(nameof(Liste));
                }
            }
        }

        public ICommand ResimYükle { get; }

        public ICommand Durdur { get; }

        public ICommand Oynat { get; }

        public FilterInfo SeçiliKamera
        {
            get => seçiliKamera;

            set
            {
                if (seçiliKamera != value)
                {
                    seçiliKamera = value;
                    OnPropertyChanged(nameof(SeçiliKamera));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void CameraUserControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is "SeçiliKamera")
            {
                Player.Device = new CapDevice(SeçiliKamera.MonikerString)
                {
                    MaxHeightInPixels = 1080
                };
            }
        }
    }
}
