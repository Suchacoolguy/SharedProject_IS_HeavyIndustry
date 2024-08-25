using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SharedProject_IS_HeavyIndustry.Converters
{
    public class DivideByTenConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return (double)intValue - 2;
            }
            Console.WriteLine("DivideByTenConverter: value is not int");
            return value;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}