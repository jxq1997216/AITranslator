using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AITranslator.View.Converter
{
    public class StartButtonTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string[] strs = parameter.ToString().Split(' ');
            if (strs.Length != 4)
                throw new ArgumentException("无效的参数！");

            bool isTranslating = (bool)values[0];
            bool isBreaked = (bool)values[1];
            double progress = (double)values[2];

            if (isTranslating)
                return strs[0];

            if (progress >= 100)
                return strs[3];

            return isBreaked ? strs[1] : strs[2];
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
