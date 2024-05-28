using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AITranslator.View.Converter
{
    public class GpuLayerCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int gpuLayoutCount = (int)value;
            if (gpuLayoutCount == -1)
                return 50;
            return gpuLayoutCount;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double gpuLayoutCount = (double)value;
            if (gpuLayoutCount >= 50)
                return -1;
            return gpuLayoutCount;
        }
    }
}
