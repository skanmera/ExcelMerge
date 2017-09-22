using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ExcelMerge.GUI.ValueConverters
{
    public class MultiEmptyStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return false;

            if (!values.Any())
                return false;

            var ret = true;
            foreach (var value in values)
            {
                ret &= !string.IsNullOrEmpty(value?.ToString());
            }

            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
