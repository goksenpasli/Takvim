using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;

namespace Takvim
{
    public class MovableControl:DependencyObject
    {
        public static bool GetMoveData(DependencyObject obj) => (bool)obj.GetValue(MoveDataProperty);

        public static void SetMoveData(DependencyObject obj, bool value) => obj.SetValue(MoveDataProperty, value);

        public static readonly DependencyProperty MoveDataProperty = DependencyProperty.RegisterAttached("MoveData", typeof(bool), typeof(MovableControl), new PropertyMetadata(false,Changed));

        private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Canvas canvas)
            {
                canvas.DragEnter += Canvas_DragEnter;
                canvas.MouseMove += Canvas_MouseMove;
            }
            if (d is ListBox listBox)
            {
                listBox.Drop += ListBox_Drop;
            }
            if (d is Button backforwardmonthbutton)
            {
                backforwardmonthbutton.DragEnter += Button_DragEnter;
            }
        }

        private static void Button_DragEnter(object sender, DragEventArgs e)
        {
            var dc = (sender as Button)?.DataContext as MainViewModel;
            switch ((sender as Button)?.Tag.ToString())
            {
                case "Ayİleri":
                    dc?.Ayİleri.Execute(null);
                    break;
                case "AyGeri":
                    dc?.AyGeri.Execute(null);
                    break;
            }
        }

        private static void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("data"))
            {
                var dc = (e.Source as ListBox)?.DataContext as Data;
                var dropdata = e.Data.GetData("data") as XmlNode;
                dropdata["Gun"].InnerText = dc.TamTarih.ToString("o");
                MainViewModel.xmlDataProvider.Document.Save(MainViewModel.xmlpath);
                CollectionViewSource.GetDefaultView((Application.Current.MainWindow.DataContext as MainViewModel)?.AyGünler).Refresh();
                MainViewModel.xmlDataProvider.Refresh();
            }
        }

        private static void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var dc = (sender as Canvas)?.DataContext as XmlNode;
                DragDrop.DoDragDrop(sender as Canvas, new DataObject("data", dc), DragDropEffects.Move);
            }
        }

        private static void Canvas_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("data") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }
    }
}
