﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AITranslator.View.Converter
{
    public class LoadModelButtonTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string[] strs = parameter.ToString().Split(' ');
            if (strs.Length != 3)
                throw new ArgumentException("无效的参数！");

            bool modelLoaded = (bool)values[0];
            if (modelLoaded)
                return strs[0];

            bool isLoadingModel = (bool)values[1];

            return isLoadingModel ? strs[2] : strs[1];
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
