using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MusicClient.Converters
{
    public class BoolToFavoriteColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFavorite)
            {
                string colorKey = isFavorite ? "AccentColor" : "TextSecondary";
                return Application.Current.TryFindResource(colorKey) as SolidColorBrush ?? Brushes.Gray; // Возвращаем серый по умолчанию, если ресурс не найден
            }
            return Brushes.Gray; // Серый по умолчанию
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
 
 
 