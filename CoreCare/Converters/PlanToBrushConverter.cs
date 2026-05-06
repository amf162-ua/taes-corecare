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
                return plan switch
                {
                    TipoPlan.Premium => new SolidColorBrush(Color.FromArgb(255, 255, 215, 0)), // Gold
                    TipoPlan.Basico => new SolidColorBrush(Color.FromArgb(255, 144, 144, 144)), // Gray
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
