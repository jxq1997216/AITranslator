using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AITranslator.View.Converter
{
    public class ComboBoxStyleMarginConverter : IValueConverter
    {
        readonly Thickness NaNWidthMargin = new Thickness(8, 3, 24, 3);
        readonly Thickness HasWidthMargin = new Thickness(8, 3, 0, 3);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double width = (double)value;
            if (width is double.NaN)
                return NaNWidthMargin;
            else
                return HasWidthMargin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
