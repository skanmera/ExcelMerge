using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ExcelMerge.GUI.ValueConverters
{
    public class FileDialogParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var obj = values[0];
            var propertyName = values[1] as string;
            var propertyInfo = obj.GetType().GetProperties().FirstOrDefault(p => p.Name == propertyName);

            return new FileDialogParameter(obj, propertyInfo);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
