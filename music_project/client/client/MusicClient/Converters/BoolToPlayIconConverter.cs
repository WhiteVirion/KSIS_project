using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace MusicClient.Converters
{
    public class BoolToPlayIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPlaying)
                return Application.Current.TryFindResource(isPlaying ? "PauseIcon" : "PlayIcon");
            return Application.Current.TryFindResource("PlayIcon");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 