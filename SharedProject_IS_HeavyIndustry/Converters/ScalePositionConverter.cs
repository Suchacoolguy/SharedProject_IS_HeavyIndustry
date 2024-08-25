using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SharedProject_IS_HeavyIndustry.Converters
{
    public class ScalePositionConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double originalPosition)
            {
                double scaleFactor = System.Convert.ToDouble(parameter);
                return originalPosition * scaleFactor;
            }
            return value;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}