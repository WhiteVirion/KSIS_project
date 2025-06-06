using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MusicClient.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = false;
            if (value is bool)
            {
                bValue = (bool)value;
            }
            else if (value is bool?)
            {
                bool? tmp = (bool?)value;
                bValue = tmp.HasValue ? tmp.Value : false;
            }

            // Инвертирование логики, если параметр "invert" или "inverse"
            if (parameter != null)
            {
                string paramString = parameter.ToString().ToLower();
                if (paramString == "invert" || paramString == "inverse")
                {
                    bValue = !bValue;
                }
            }
            return bValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                return (Visibility)value == Visibility.Visible;
            }
            return false;
        }
    }
}