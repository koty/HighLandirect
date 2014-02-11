using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HighLandirect.Domains.Reports
{
    public class CustomerSource
    {
        public IEnumerable<Customer> Customers { get; private set; }

        public CustomerSource()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var list = new List<Customer>();
                for (var i = 0; i < 10; i++)
                {
                    list.Add(new Customer()
                    {
                        PrefectureName = "○県" + i.ToString(),
                        CityName = "○市" + i.ToString(),
                        Address1 = "住所１" + i.ToString(),
                        Address2 = "住所２" + i.ToString(),
                        Address3 = "住所３" + i.ToString(),
                        Address4 = "住所４" + i.ToString(),
                        CustName = "長野太郎" + i.ToString(),
                        Phone = "03-1234-5678",
                        PostalCD = "1234567",
                        CustNo = i
                    });
                }
                this.Customers = list;
            }
        }

        public CustomerSource(IEnumerable<Customer> customers)
        {
            this.Customers = customers;
        }
    }
}
