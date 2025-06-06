using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicClient.Converters
{
    public class DurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double totalSeconds && totalSeconds > 0)
            {
                TimeSpan time = TimeSpan.FromSeconds(totalSeconds);
                return time.ToString(@"mm\:ss");
            }
            return "00:00"; // Или string.Empty, или другое значение по умолчанию
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 