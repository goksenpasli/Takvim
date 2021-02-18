using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(FileSelectorTextBox), new PropertyMetadata(null));

        static FileSelectorTextBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(FileSelectorTextBox), new FrameworkPropertyMetadata(typeof(FileSelectorTextBox)));

        public FileSelectorTextBox()
        {
            CommandBindings.Add(new CommandBinding(SelectFile, SelectFileCommand));
        }

        private void SelectFileCommand(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = false, Filter = "Tüm Dosyalar (*.*)|*.*" };
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
            }
        }
    }
}
