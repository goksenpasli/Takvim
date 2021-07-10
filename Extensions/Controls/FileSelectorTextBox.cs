﻿using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Extensions
{
    public class FileSelectorTextBox : TextBox, INotifyPropertyChanged
    {
        public static readonly DependencyProperty DosyalarProperty = DependencyProperty.Register("Dosyalar", typeof(ObservableCollection<string>), typeof(FileSelectorTextBox), new PropertyMetadata(null));

        public static readonly DependencyProperty FileListPanelVisibilityProperty = DependencyProperty.Register("FileListPanelVisibility", typeof(Visibility), typeof(FileSelectorTextBox), new PropertyMetadata(Visibility.Visible));

        private string filePath = "Dosya Seçilmedi";

        static FileSelectorTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FileSelectorTextBox), new FrameworkPropertyMetadata(typeof(FileSelectorTextBox)));
        }

        public FileSelectorTextBox()
        {
            _ = CommandBindings.Add(new CommandBinding(SelectFile, SelectFileCommand));
            _ = CommandBindings.Add(new CommandBinding(RemoveItem, RemoveFileCommand));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Dosyalar
        {
            get => (ObservableCollection<string>)GetValue(DosyalarProperty);
            set => SetValue(DosyalarProperty, value);
        }

        public Visibility FileListPanelVisibility
        {
            get => (Visibility)GetValue(FileListPanelVisibilityProperty);
            set => SetValue(FileListPanelVisibilityProperty, value);
        }

        public string FilePath
        {
            get => filePath;

            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }

        public ICommand RemoveItem { get; } = new RoutedCommand();

        public ICommand SelectFile { get; } = new RoutedCommand();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RemoveFileCommand(object sender, ExecutedRoutedEventArgs e)
        {
            _ = Dosyalar.Remove(e.Parameter as string);
            FilePath = Dosyalar.Count == 0 ? "Dosya Seçilmedi" : $"{Dosyalar.Count} Dosya Seçildi";
        }

        private void SelectFileCommand(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
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