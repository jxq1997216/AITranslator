using AITranslator.Exceptions;
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
    public class CommunicatorTypeToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string[] strs = parameter.ToString().Split(' ');
            if (strs.Length != 4)
                throw new ArgumentException("无效的参数！");

            CommunicatorType communicatorType = (CommunicatorType)values[0];
            switch (communicatorType)
            {
                case CommunicatorType.LLama:
                    bool isLoaded = (bool)values[1];
                    if (isLoaded)
                        return $"{strs[3]} [{values[4]}]";

                    bool isLoading = (bool)values[2];
                    if (isLoading)
                        return $"{strs[2]}{(double)values[3]:0.##}%";
                    else
                        return strs[1];
                case CommunicatorType.OpenAI:
                    return strs[0];
                default:
                    throw ExceptionThrower.InvalidCommunicator;
            }

           
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
