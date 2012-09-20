using System;
using System.Windows.Data;

namespace HighLandirect.Converters
{
    public class NumberFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object
            parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return System.Convert.ToInt32(value).ToString(parameter as string);
            }
            catch (Exception)
            {
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                return int.Parse(value.ToString().Replace(",", ""));
            }
            catch (Exception)
            {
                return value;
            }
        }
    }
}
