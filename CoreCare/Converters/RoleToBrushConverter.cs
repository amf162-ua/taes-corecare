using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CoreCare.Models;

namespace CoreCare.Converters
{
    public class RoleToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserRole role)
            {
                return role == UserRole.Administrador 
                    ? (Brush)new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFF"))  // Cyan
                    : (Brush)new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00")); // Green
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#717182")); // Muted
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
