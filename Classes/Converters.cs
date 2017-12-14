﻿using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Classes
{
    public class ConverteToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                return new SolidColorBrush(GetColorFromHexa(value.ToString()));
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string hexaColor = value.ToString();
                return Color.FromArgb(
                       System.Convert.ToByte(hexaColor.Substring(1, 2), 16),
                       System.Convert.ToByte(hexaColor.Substring(3, 2), 16),
                       System.Convert.ToByte(hexaColor.Substring(5, 2), 16),
                       System.Convert.ToByte(hexaColor.Substring(7, 2), 16));
            }
            catch
            {
                return value;
            }
        }

        private Color GetColorFromHexa(string hexaColor)
        {
            return Color.FromArgb(
                   System.Convert.ToByte(hexaColor.Substring(1, 2), 16),
                   System.Convert.ToByte(hexaColor.Substring(3, 2), 16),
                   System.Convert.ToByte(hexaColor.Substring(5, 2), 16),
                   System.Convert.ToByte(hexaColor.Substring(7, 2), 16));
        }
    }
}
