using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AITranslator.View.Converter
{
    public class AddSubConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double param = System.Convert.ToDouble(parameter);
            double val = (double)value;
            return val + param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double param = (double)parameter;
            double val = (double)value;
            return val - param;
        }
    }
}
