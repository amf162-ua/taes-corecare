using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CoreCare.Models;

namespace CoreCare.Converters
{
    public class PlanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TipoPlan plan)
            {
                return plan == TipoPlan.Premium
                    ? (Brush)new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA500"))  // Orange
                    : (Brush)new SolidColorBrush((Color)ColorConverter.ConvertFromString("#717182")); // Muted
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#717182")); // Muted
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
