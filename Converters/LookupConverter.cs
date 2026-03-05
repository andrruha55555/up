using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace AdminUP.Converters
{
    /// <summary>
    /// Converts an int? ID to a display name using a Dictionary lookup.
    /// Usage: {Binding classroom_id, Converter={StaticResource LookupConverter}, ConverterParameter={Binding ...}}
    /// Better used via code-behind lookups in ViewModel.
    /// </summary>
    public class LookupConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return string.Empty;

            var id = values[0];
            var dict = values[1] as Dictionary<int, string>;

            if (dict == null || id == null) return string.Empty;

            if (id is int intId && dict.TryGetValue(intId, out var name))
                return name;

            return id?.ToString() ?? string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
