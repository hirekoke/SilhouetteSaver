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
using SilhouetteSaverLib;

namespace SilhouetteSaver
{
    /// <summary>
    /// GrayAlphaEffectControl.xaml の相互作用ロジック
    /// </summary>
    public partial class GrayAlphaEffectControl : UserControl
    {
        public GrayAlphaEffectControl()
        {
            InitializeComponent();
        }
    }

    class RatioConverter : IValueConverter
    {
        // 0-1 to double (* max parameter)
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return 0.0;

            double max = 10;
            if (parameter != null)
            {
                try
                {
                    max = (double)parameter;
                }
                catch (InvalidCastException)
                {
                    max = 10;
                }
            }

            try
            {
                double v = (double)value;
                return (double)(v * max);
            }
            catch (InvalidCastException)
            {
                return 0.0;
            }
        }

        // to 0-1
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return 0.0;

            double max = 10;
            if (parameter != null)
            {
                try
                {
                    max = (double)parameter;
                }
                catch (InvalidCastException)
                {
                    max = 10;
                }
            }

            try
            {
                double v = (double)value;
                if (max == 0) return 0.0;
                return v / max;
            }
            catch (InvalidCastException)
            {
                return 0.0;
            }
        }
    }
}
