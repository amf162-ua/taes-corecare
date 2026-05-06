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
                return role switch
                {
                    UserRole.Administrador => new SolidColorBrush(Color.FromArgb(255, 220, 20, 60)), // Crimson
                    UserRole.Cliente => new SolidColorBrush(Color.FromArgb(255, 70, 130, 180)), // Steel Blue
                    _ => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)) // Gray
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
