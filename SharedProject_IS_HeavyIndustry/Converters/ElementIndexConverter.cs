using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SharedProject_IS_HeavyIndustry.Converters
{
    public class ElementIndexConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is IList list && values[1] != null)
            {
                int index = list.IndexOf(values[1]) + 1; // Add 1 to make the index 1-based.
                return index.ToString(); // Return the index as a string.
            }
            return "-1";
        }
    }
}