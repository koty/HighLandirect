using System;
using System.Windows.Data;
using System.Globalization;

namespace HighLandirect.Converters
{
    class PostalMarkConverter : IValueConverter
    {
        private static readonly PostalMarkConverter defaultInstance = new PostalMarkConverter();

        public static PostalMarkConverter Default { get { return defaultInstance; } }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value as string)) return "";

            var str = value.ToString();

            if (str.Length > 3)
            {
                return "〒" + str.Substring(0, 3) + "-" + str.Substring(3, str.Length - 3);
            }
            else
            {
                return "〒" + str;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString().Replace("〒", "").Replace("-", "");
        }
    }
}
