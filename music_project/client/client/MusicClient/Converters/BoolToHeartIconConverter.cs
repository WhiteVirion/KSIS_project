using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MusicClient.Converters
{
    public class BoolToHeartIconConverter : IValueConverter
    {
        // Path Data for a filled heart (example - you might need to adjust this for your desired icon style)
        private const string FilledHeartData = "M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z";
        // Path Data for an outlined/empty heart (corrected)
        private const string EmptyHeartData = "M16.5 3c-1.74 0-3.41.81-4.5 2.09C10.91 3.81 9.24 3 7.5 3 4.42 3 2 5.42 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5c0-3.08-2.42-5.5-5.5-5.5zm0 2c1.93 0 3.5 1.57 3.5 3.5 0 2.73-2.55 5.15-7.05 9.24L12 18.89l-.95-1.1C6.55 13.15 4 10.73 4 8c0-1.93 1.57-3.5 3.5-3.5 1.54 0 3.04.99 3.57 2.36h1.87C13.46 5.99 14.96 5 16.5 5z";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFavorite)
            {
                return Geometry.Parse(isFavorite ? FilledHeartData : EmptyHeartData);
            }
            return Geometry.Parse(EmptyHeartData); // Default to empty heart
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 