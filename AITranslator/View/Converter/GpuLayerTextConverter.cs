using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AITranslator.View.Converter
{
    public class GpuLayerTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double gpuLayoutCount = (double)value;
            if (gpuLayoutCount >= 50)
                return "MAX";
            return gpuLayoutCount;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string gpuLayoutCount = (string)value;
            if (gpuLayoutCount.ToUpper() == "MAX")
                return 50;
            if (int.TryParse(gpuLayoutCount,out int count))
                return count;
            return 0;
        }
    }
}
