using System;
using System.Globalization;
using System.Windows.Data;
using CoreCare.Models;

namespace CoreCare.Converters
{
    public class StringToRoleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserRole role)
            {
                return role == UserRole.Cliente ? "Cliente" : "Administrador";
            }
            return "Todos";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            return str switch
            {
                "Cliente" => UserRole.Cliente,
                "Administrador" => UserRole.Administrador,
                _ => null
            };
        }
    }
}
