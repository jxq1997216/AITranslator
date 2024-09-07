using AITranslator.View.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AITranslator.View.Converter
{

    public class ViewTitleConverter : IMultiValueConverter
    {
        string[] arr_ViewTitle = ["未完成","已完成","手动翻译","日志","设置","模板"];
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            for (int i = 0; i < values.Length; i++)
            {
                bool isChecked = (bool)values[i];
                if (isChecked)
                    return arr_ViewTitle[i];
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
