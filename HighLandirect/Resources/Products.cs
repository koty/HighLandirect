using System.Collections.Generic;
using System.Linq;
using HighLandirect.Domains;
using MyDataEntities = HighLandirect.Domains.MyDataEntities;

namespace HighLandirect.Resources
{
    public class Products
    {
        public IList<Product> GetProducts()
        {
            var customerEntities = new MyDataEntities();
            return customerEntities.Products.ToList();
        }
        public Products() { }
    }
}
