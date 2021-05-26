using CatenaLogic.Windows.Presentation.WebcamPlayer;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Takvim
{
    public partial class CameraUserControl : UserControl, INotifyPropertyChanged
    {
        private CapDevice device;

        private FilterInfo[] liste = CapDevice.DeviceMonikers;

        private double rotation = 180;

        private FilterInfo seçiliKamera;

        public CameraUserControl()
        {
            InitializeComponent();
            DataContext = this;

            KameradanResimYükle = new RelayCommand<object>(parameter =>
            {
                if (parameter is Data data)
                {
                    using MemoryStream ms = new();
                    JpegBitmapEncoder encoder = new();
                    encoder.Frames.Add(BitmapFrame.Create(new TransformedBitmap(Device.BitmapSource, new RotateTransform(Rotation))));
                    encoder.QualityLevel = 100;
                    encoder.Save(ms);
                    data.ResimData = ms.ToArray().WebpEncode(data.WebpQuality);
                    data.DosyaUzantı = ".webp";
                    data.Boyut = data.ResimData.Length / 1024;
                }
            }, parameter => SeçiliKamera is not null);

            Durdur = new RelayCommand<object>(parameter => Device.Stop(), parameter => SeçiliKamera is not null && Device.IsRunning);

            Oynat = new RelayCommand<object>(parameter => Device.Start(), parameter => SeçiliKamera is not null && !Device.IsRunning);

            PropertyChanged += CameraUserControl_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CapDevice Device
        {
            get => device;

            set
            {
                if (device != value)
                {
                    device = value;
                    OnPropertyChanged(nameof(Device));
                }
            }
        }

        public ICommand Durdur { get; }

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

        public ICommand Oynat { get; }

        public ICommand KameradanResimYükle { get; }

        public double Rotation
        {
            get => rotation;

            set
            {
                if (rotation != value)
                {
                    rotation = value;
                    OnPropertyChanged(nameof(Rotation));
                }
            }
        }

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
                Device = new CapDevice(SeçiliKamera.MonikerString)
                {
                    MaxHeightInPixels = 1080
                };
            }
        }
    }
}