using System;
using System.Linq;
using System.Windows.Data;
using System.Globalization;
using HighLandirect.Domains;

namespace HighLandirect.Converters
{
    public class ProductConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var ProductID = int.Parse(value.ToString());

            MyDataEntities customerEntities = new MyDataEntities();
            var q = from r in customerEntities.Products
                    where r.ProductID == ProductID
                    select r;

            if (q.Count() == 0)
                return "";

            return q.FirstOrDefault().ProductName;                        
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            MyDataEntities customerEntities = new MyDataEntities();
            var q = from r in customerEntities.Products
                    where r.ProductName == value.ToString()
                    select r;

            if (q.Count() == 0)
                return "";

            return q.FirstOrDefault().ProductID;
        }

        #endregion
    }
}
