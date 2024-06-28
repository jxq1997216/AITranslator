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
    public class TaskStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TaskState state = (TaskState)value;
            return state switch
            {
                TaskState.Initialized => "已初始化",
                TaskState.Cleaning => "清理中",
                TaskState.WaitTranslate => "等待翻译",
                TaskState.Translating => "正在翻译",
                TaskState.WaitPause => "正在暂停",
                TaskState.Pause => "暂停",
                TaskState.WaitMerge => "等待合并",
                TaskState.Merging => "正在合并",
                TaskState.Completed => "已完成",
                _ => state.ToString(),
            }; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
