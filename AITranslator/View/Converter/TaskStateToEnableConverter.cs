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
    public class TaskStateToEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TaskState taskState = (TaskState)value;
            string param = parameter.ToString();
            switch (param)
            {
                case "Start":
                    return taskState != TaskState.WaitMerge && taskState != TaskState.Merging;
                case "Set":
                    return taskState != TaskState.WaitTranslate && taskState != TaskState.Translating && taskState != TaskState.WaitPause && taskState != TaskState.WaitMerge && taskState != TaskState.Merging && taskState != TaskState.Cleaning;
                case "Merge":
                    return taskState == TaskState.WaitMerge && taskState != TaskState.Merging;
                default:
                    return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
