using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Takvim
{
    public class GünTatilConverter : IValueConverter
    {
        private readonly List<Tuple<string, int>> tatiller = new List<Tuple<string, int>>();

        public GünTatilConverter()
        {
            tatiller.Add(new Tuple<string, int>("Ocak", 1));
            tatiller.Add(new Tuple<string, int>("Nisan", 23));
            tatiller.Add(new Tuple<string, int>("Mayıs", 1));
            tatiller.Add(new Tuple<string, int>("Mayıs", 19));
            tatiller.Add(new Tuple<string, int>("Temmuz", 15));
            tatiller.Add(new Tuple<string, int>("Ağustos", 30));
            tatiller.Add(new Tuple<string, int>("Ekim", 29));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Data data && tatiller.Any(z => z.Item1 == data.Ay && z.Item2 == data.Gün);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class YarımGünTatilConverter : IValueConverter
    {
        private readonly List<Tuple<string, int>> tatiller = new List<Tuple<string, int>>();

        public YarımGünTatilConverter() => tatiller.Add(new Tuple<string, int>("Ekim", 28));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Data data && tatiller.Any(z => z.Item1 == data.Ay && z.Item2 == data.Gün);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<object, bool> canExecute;

        private readonly Action<object> execute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged { add => CommandManager.RequerySuggested += value; remove => CommandManager.RequerySuggested -= value; }

        public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter);

        public void Execute(object parameter) => execute(parameter);
    }

    public abstract class InpcBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected virtual void OnPropertyChanging(string propertyName) => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    public class Data : InpcBase
    {
        private int gün;

        private string günAdı;

        private string ay;

        private int offset;

        public int Gün
        {
            get => gün;
            set
            {
                if (gün != value)
                {
                    gün = value;
                    OnPropertyChanged(nameof(Gün));
                }
            }
        }

        public string Ay
        {
            get { return ay; }

            set
            {
                if (ay != value)
                {
                    ay = value;
                    OnPropertyChanged(nameof(Ay));
                }
            }
        }

        public string GünAdı
        {
            get { return günAdı; }
            set
            {
                if (günAdı != value)
                {
                    günAdı = value;
                    OnPropertyChanged(nameof(GünAdı));
                }
            }
        }

        public int Offset
        {
            get { return offset; }

            set
            {
                if (offset != value)
                {
                    offset = value;
                    OnPropertyChanged(nameof(Offset));
                }
            }
        }
    }

    public class MainViewModel : InpcBase
    {
        private ObservableCollection<Data> günler;

        public ObservableCollection<Data> Günler
        {
            get { return günler; }

            set
            {
                if (günler != value)
                {
                    günler = value;
                    OnPropertyChanged(nameof(Günler));
                }
            }
        }

        private int seçiliYıl = DateTime.Now.Year;

        private int bugünIndex = DateTime.Today.DayOfYear - 1;

        public int SeçiliYıl
        {
            get { return seçiliYıl; }

            set
            {
                if (seçiliYıl != value)
                {
                    seçiliYıl = value;
                    TakvimOluştur();
                    OnPropertyChanged(nameof(SeçiliYıl));
                }
            }
        }

        public int BugünIndex
        {
            get { return bugünIndex; }

            set
            {
                if (bugünIndex != value)
                {
                    bugünIndex = value;
                    OnPropertyChanged(nameof(BugünIndex));
                }
            }
        }

        public MainViewModel()
        {
            TakvimOluştur();
            Geri = new RelayCommand(parameter => SeçiliYıl--, parameter => SeçiliYıl > 1);
            İleri = new RelayCommand(parameter => SeçiliYıl++, parameter => SeçiliYıl < 9999);
        }

        public ICommand Geri { get; }

        public ICommand İleri { get; }

        private void TakvimOluştur()
        {
            Günler = new ObservableCollection<Data>();
            for (int i = 1; i <= 12; i++)
            {
                for (int j = 1; j <= 31; j++)
                {
                    string tarih = $"{SeçiliYıl}-{i}-{j}";
                    if (DateTime.TryParse(tarih, out _))
                    {
                        var data = new Data
                        {
                            GünAdı = DateTime.Parse(tarih).ToString("ddd"),
                            Gün = DateTime.Parse(tarih).Day,
                            Ay = DateTime.Parse(tarih).ToString("MMMM"),
                            Offset = (int)(DateTime.Parse(tarih).DayOfWeek)
                        };
                        Günler.Add(data);
                    }
                }
            }
        }
    }
}