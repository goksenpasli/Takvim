using Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace Takvim
{
    public class MovableControl : DependencyObject
    {
        public static readonly DependencyProperty DraggedDataProperty = DependencyProperty.RegisterAttached("DraggedData", typeof(object), typeof(MovableControl), new PropertyMetadata(null));

        public static readonly DependencyProperty MoveDataProperty = DependencyProperty.RegisterAttached("MoveData", typeof(bool), typeof(MovableControl), new PropertyMetadata(false, Changed));

        public static readonly DependencyProperty PlacedDataProperty = DependencyProperty.RegisterAttached("PlacedData", typeof(object), typeof(MovableControl), new PropertyMetadata(null));

        public static object GetDraggedData(DependencyObject obj)
        {
            return obj.GetValue(DraggedDataProperty);
        }

        public static bool GetMoveData(DependencyObject obj)
        {
            return (bool)obj.GetValue(MoveDataProperty);
        }

        public static object GetPlacedData(DependencyObject obj)
        {
            return obj.GetValue(PlacedDataProperty);
        }

        public static void SetDraggedData(DependencyObject obj, object value)
        {
            obj.SetValue(DraggedDataProperty, value);
        }

        public static void SetMoveData(DependencyObject obj, bool value)
        {
            obj.SetValue(MoveDataProperty, value);
        }

        public static void SetPlacedData(DependencyObject obj, object value)
        {
            obj.SetValue(PlacedDataProperty, value);
        }

        private static void Button_DragEnter(object sender, DragEventArgs e)
        {
            MainViewModel dc = GetDraggedData(sender as Button) as MainViewModel;
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

        private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uielement && GetMoveData(d))
            {
                uielement.DragEnter += Uielement_DragEnter;
                uielement.MouseMove += Uielement_MouseMove;
                uielement.GiveFeedback += Uielement_GiveFeedback;
                uielement.Drop += Selector_Drop;
            }

            if (d is Button backforwardmonthbutton && GetMoveData(d))
            {
                backforwardmonthbutton.DragEnter += Button_DragEnter;
            }
        }

        private static void Selector_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("data") && GetPlacedData(sender as UIElement) is Data dc)
            {
                (e.Data.GetData("data") as XmlNode)["Gun"].InnerText = dc.TamTarih.ToString("o");
                MainViewModel.xmlDataProvider.Document.Save(MainViewModel.xmldatapath);
                dc.VeriSayısı++;
                CollectionViewSource.GetDefaultView((Application.Current.MainWindow.DataContext as MainViewModel)?.AyGünler).Refresh();
                MainViewModel.xmlDataProvider.Refresh();
            }
        }

        private static void Uielement_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("data") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private static void Uielement_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.Move)
            {
                using Cursor customCursor = CursorHelper.CreateCursor(VisualTreeHelper.GetParent(e.Source as UIElement) as UIElement);
                if (customCursor != null)
                {
                    e.UseDefaultCursors = false;
                    Mouse.SetCursor(customCursor);
                }
            }
            else
            {
                e.UseDefaultCursors = true;
            }

            e.Handled = true;
        }

        private static void Uielement_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && GetDraggedData(sender as UIElement) is XmlNode dc)
            {
                DragDrop.DoDragDrop(sender as UIElement, new DataObject("data", dc), DragDropEffects.Move);
            }
        }
    }
}