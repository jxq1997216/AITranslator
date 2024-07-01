using AITranslator.View.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AITranslator.View.Converter
{
    public class CommunicatorTypeToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CommunicatorType communicatorType = (CommunicatorType)value;
            return (int)communicatorType;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = (int)value;
            return (CommunicatorType)index;
        }
    }
}
