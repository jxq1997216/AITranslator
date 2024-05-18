using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AITranslator.View.Converter
{
    public class TaskbarIconToolTipTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTranslating = (bool)values[0];
            bool isBreaked = (bool)values[1];
            double progress = (double)values[2];

            if (isTranslating)
                return $"翻译中...{progress:0.##}%";

            if (progress >= 100)
                return "翻译完成";

            if (isBreaked)
                return $"已暂停...{progress:0.##}%";
            else
                return "未开始";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
