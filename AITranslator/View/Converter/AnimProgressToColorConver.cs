using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace AITranslator.View.Converter
{
    public class AnimProgressToColorConver : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //Color result = Color.FromArgb(255, 255, 255, 255);
            double progress = System.Convert.ToDouble(values[0]);

            Color startColor = (Color)values[1];
            Color endColor = (Color)values[2];
            Color result = startColor + (endColor - startColor) * (float)progress;
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
