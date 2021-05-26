using System.Windows;
using System.Windows.Controls.Primitives;

namespace Takvim
{
    public class NumericUpDownControl : ScrollBar
    {
        static NumericUpDownControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDownControl), new FrameworkPropertyMetadata(typeof(NumericUpDownControl)));
    }
}
