using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Takvim
{
    public class FileSelectorTextBox : TextBox
    {
        public ICommand SelectFile { get; } = new RoutedCommand();

        public string FilePath
        {
            get => (string)GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        public ObservableCollection<string> Dosyalar
        {
            get { return (ObservableCollection<string>)GetValue(DosyalarProperty); }
            set { SetValue(DosyalarProperty, value); }
        }

        public static readonly DependencyProperty DosyalarProperty = DependencyProperty.Register("Dosyalar", typeof(ObservableCollection<string>), typeof(FileSelectorTextBox), new PropertyMetadata(null));

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(FileSelectorTextBox), new PropertyMetadata(null));

        static FileSelectorTextBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(FileSelectorTextBox), new FrameworkPropertyMetadata(typeof(FileSelectorTextBox)));

        public FileSelectorTextBox()
        {
            CommandBindings.Add(new CommandBinding(SelectFile, SelectFileCommand));
        }

        private void SelectFileCommand(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Tüm Dosyalar (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Dosyalar = new ObservableCollection<string>();
                foreach (string item in openFileDialog.FileNames)
                {
                    Dosyalar.Add(item);
                }
                FilePath = Dosyalar.Count > 1 ? $"{Dosyalar.Count} Dosya Seçildi" : Dosyalar[0];
            }
        }
    }
}
