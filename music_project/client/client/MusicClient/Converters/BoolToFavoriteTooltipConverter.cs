using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicClient.Converters
{
    public class BoolToFavoriteTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFavorite)
            {
                string[] tooltips = parameter as string[] ?? (parameter as string)?.Split('|');
                if (tooltips != null && tooltips.Length == 2)
                {
                    return isFavorite ? tooltips[0] : tooltips[1]; // e.g., "Удалить из Любимых" : "Добавить в Любимые"
                }
                // Fallback if parameter is not correctly set
                return isFavorite ? "Убрать из любимых" : "Добавить в любимые";
            }
            return "Добавить в любимые"; // Default fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 