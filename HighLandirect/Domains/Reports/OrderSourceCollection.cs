using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HighLandirect.Domains.Reports
{
    public class OrderSourceCollection
    {
        public IEnumerable<OrderSource> Orders { get; private set; }

        public OrderSourceCollection()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var list = new List<OrderSource>();
                for(var i = 0 ; i < 10; i++)
                {
                    list.Add(new OrderSource());
                }
                this.Orders = list;
            }
        }

        public OrderSourceCollection(IEnumerable<OrderSource> orders)
        {
            this.Orders = orders;
        }
    }
}
