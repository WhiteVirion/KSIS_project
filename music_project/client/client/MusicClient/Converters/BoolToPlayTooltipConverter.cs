using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicClient.Converters
{
    public class BoolToPlayTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPlaying)
                return isPlaying ? "Пауза" : "Воспроизвести";
            return "Воспроизвести";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 