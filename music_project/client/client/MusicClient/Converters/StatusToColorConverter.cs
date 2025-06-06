using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MusicClient.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string statusMessage)
            {
                if (statusMessage.ToLower().Contains("успешно"))
                {
                    return Brushes.Green;
                }
                if (statusMessage.ToLower().Contains("ошибка") || statusMessage.ToLower().Contains("не удалось"))
                {
                    return Brushes.Red;
                }
            }
            // Возвращаем цвет по умолчанию для текста, если статус не определен или сообщение null
            // Лучше использовать ресурс, если он определен в App.xaml для TextSecondary или TextPrimary
            return Brushes.LightGray; // Или Application.Current.FindResource("TextSecondaryBrush") as Brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 