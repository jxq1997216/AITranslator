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
    public class StartButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTooltip = System.Convert.ToBoolean(parameter);
            TaskState state = (TaskState)value;
            if (isTooltip)
            {
                if (state == TaskState.WaitTranslate || state == TaskState.Translating || state == TaskState.WaitPause || state == TaskState.Cleaning)
                    return "暂停";
                else
                    return "开始";
            }
            else
            {
                if (state == TaskState.WaitTranslate || state == TaskState.Translating || state == TaskState.WaitPause || state == TaskState.Cleaning)
                    return "‖";
                else
                    return "▶";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
