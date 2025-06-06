using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace MusicClient.Converters
{
    public class RepeatModeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive && isActive)
                return (SolidColorBrush)Application.Current.TryFindResource("AccentColor") ?? Brushes.Green;
            return (SolidColorBrush)Application.Current.TryFindResource("TextSecondary") ?? Brushes.Gray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 