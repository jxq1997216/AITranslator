using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AITranslator.View.Converter
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] strs = parameter.ToString().Split(' ');
            if (strs.Length != 2)
                throw new ArgumentException("无效的参数！");

            bool b = (bool)value;
            return b? strs[0]: strs[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] strs = parameter.ToString().Split(' ');
            if (strs.Length != 2)
                throw new ArgumentException("无效的参数！");

            return value.ToString() == strs[0];
        }
    }
}
