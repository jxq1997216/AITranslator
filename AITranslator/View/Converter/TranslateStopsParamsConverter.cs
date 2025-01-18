using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AITranslator.View.Converter
{
    public class TranslateStopsParamsConverter : IValueConverter
    {
        static Dictionary<string, string> EscapeChars = new Dictionary<string, string>()
       {
           { "\\",@"\\"},
           { "\'",@"\'"},
           { "\"",@"\"""},
           { "\0",@"\0"},
           { "\a",@"\a"},
           { "\b",@"\b"},
           { "\f",@"\f"},
           { "\n",@"\n"},
           { "\r",@"\r"},
           { "\t",@"\t"},
           { "\v",@"\v"},
       };
        static Dictionary<string, string> REscapeChars = EscapeChars.Reverse().ToDictionary();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<string> strings = (ObservableCollection<string>)value;
            string[] converterdStr = Array.ConvertAll(strings.ToArray(), s => ConverEscapeChar(s));
            return string.Join(Environment.NewLine, converterdStr);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? stopsStr = value.ToString();
            if (stopsStr is not null)
            {
                string[] converterdStr = Array.ConvertAll(stopsStr.Split(Environment.NewLine, options: StringSplitOptions.RemoveEmptyEntries), s => ConverBackEscapeChar(s));
                return new ObservableCollection<string>(converterdStr);
            }
            else
                return new ObservableCollection<string>();
        }

        string ConverEscapeChar(string input)
        {
            foreach (var escapeChar in EscapeChars)
                input = input.Replace(escapeChar.Key, escapeChar.Value);
            return input;
        }
        string ConverBackEscapeChar(string input)
        {
            foreach (var escapeChar in REscapeChars)
                input = input.Replace(escapeChar.Value, escapeChar.Key);
            return input;
        }
    }
}
