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
    public class AnimProgressTo4ColorConver : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //Color result = Color.FromArgb(255, 255, 255, 255);
            double progress = System.Convert.ToDouble(values[0]);

            Color uncheckedColor = (Color)values[1];
            Color uncheckedEnrerColor = (Color)values[2];
            Color checkedColor = (Color)values[3];
            Color checkedEnterColor = (Color)values[4];
            bool isChecked = (bool)values[5];
            Color result;
            if (!isChecked)
                result = uncheckedColor + (uncheckedEnrerColor - uncheckedColor) * (float)progress;
            else
                result = checkedColor + (checkedEnterColor - checkedColor) * (float)progress;
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
