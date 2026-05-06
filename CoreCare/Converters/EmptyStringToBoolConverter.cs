using System;
using System.Globalization;
using System.Windows.Data;

namespace CoreCare.Converters
{
    public class EmptyStringToBoolConverter : IValueConverter
    {
        public static readonly EmptyStringToBoolConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
