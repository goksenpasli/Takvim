using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Extensions
{
    public class FileGraphControl : FrameworkElement
    {
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register("Brush", typeof(Brush), typeof(FileGraphControl), new FrameworkPropertyMetadata(Brushes.Blue, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FilesProperty = DependencyProperty.Register("Files", typeof(ObservableCollection<string>), typeof(FileGraphControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public FileGraphControl()
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            RenderTransformOrigin = new Point(0.5, 0.5);
            RenderTransform = new ScaleTransform
            {
                ScaleY = -1,
                ScaleX = 1
            };
        }

        public Brush Brush
        {
            get => (Brush)GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        public ObservableCollection<string> Files
        {
            get => (ObservableCollection<string>)GetValue(FilesProperty);
            set => SetValue(FilesProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }
            if (Files?.Any() == true)
            {
                double max = Files.Max(z => new FileInfo(z).Length);//should be taken from here
                Pen pen = new(Brush, ActualWidth / Files.Count);
                pen.Freeze();
                for (int i = 0; i < Files.Count; i++)
                {
                    drawingContext.DrawLine(pen, new Point(i * ActualWidth / Files.Count, 0.0), new Point(i * ActualWidth / Files.Count, new FileInfo(Files[i]).Length / max * ActualHeight));
                }
                base.OnRender(drawingContext);
            }
        }
    }
}