using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MusicClient.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue && parameter is string colorParameterString)
            {
                string[] colors = colorParameterString.Split(';');
                if (colors.Length == 2)
                {
                    try
                    {
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(isTrue ? colors[0] : colors[1]));
                    }
                    catch
                    {
                        return DependencyProperty.UnsetValue; // Ошибка в параметре цвета
                }
            }
            }
            return DependencyProperty.UnsetValue; // Параметр не задан или неверного типа
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
