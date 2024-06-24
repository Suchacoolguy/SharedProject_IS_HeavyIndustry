using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace SharedProject_IS_HeavyIndustry.Converters;

public class ListToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is List<double> doubleList)
        {
            return string.Join(",", doubleList);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return str.Split(',').Select(double.Parse).ToList();
        }
        return new List<double>();
    }
}