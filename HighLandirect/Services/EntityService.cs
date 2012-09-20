using System.Collections.ObjectModel;
using System.Linq;
using HighLandirect.Domains;
using Customer = HighLandirect.Domains.Customer;

namespace HighLandirect.Services
{
    public class EntityService : IEntityService
    {
        private MyDataEntities customerEntities;
        private EntityObservableCollection<Customer> customers;
        private EntityObservableCollection<Product> products;
        private EntityObservableCollection<Order> orders;
        private EntityObservableCollection<OrderHistory> orderHistories;
        private EntityObservableCollection<Store> stores;
        private EntityObservableCollection<ReportMemo> reportMemos;

        public MyDataEntities Customerentities
        {
            get { return customerEntities; }
            set { customerEntities = value; }
        }

        public ObservableCollection<Customer> Customers
        {
            get 
            {
                if (customers == null && customerEntities != null)
                {
                    IQueryable<Customer> customersQuery = customerEntities.CreateQuery<Customer>("[Customers]");
                    customers = new EntityObservableCollection<Customer>(customerEntities, "Customers", customersQuery);
                }
                return customers;
            }
        }

        public ObservableCollection<Product> Products
        {
            get
            {
                if (products == null && customerEntities != null)
                {
                    IQueryable<Product> productsQuery = customerEntities.CreateQuery<Product>("[Products]");
                    products = new EntityObservableCollection<Product>(customerEntities, "Products", productsQuery);
                }
                return products;
            }
        }

        public ObservableCollection<Order> Orders
        {
            get
            {
                if (orders == null && customerEntities != null)
                {
                    IQueryable<Order> ordersQuery = customerEntities.CreateQuery<Order>("[Orders]");
                    orders = new EntityObservableCollection<Order>(customerEntities, "Orders", ordersQuery);
                }
                return orders;
            }
        }

        public ObservableCollection<OrderHistory> OrderHistories
        {
            get
            {
                if (orderHistories == null && customerEntities != null)
                {
                    IQueryable<OrderHistory> productsQuery = customerEntities.CreateQuery<OrderHistory>("[OrderHistories]");
                    orderHistories = new EntityObservableCollection<OrderHistory>(customerEntities, "OrderHistories", productsQuery);
                }
                return orderHistories;
            }
        }

        public ObservableCollection<Store> Stores
        {
            get
            {
                if (stores == null && customerEntities != null)
                {
                    IQueryable<Store> storesQuery = customerEntities.CreateQuery<Store>("[Stores]");
                    stores = new EntityObservableCollection<Store>(customerEntities, "Stores", storesQuery);
                }
                return stores;
            }
        }

        public ObservableCollection<ReportMemo> ReportMemos
        {
            get
            {
                if (reportMemos == null && customerEntities != null)
                {
                    IQueryable<ReportMemo> reportMemosQuery = customerEntities.CreateQuery<ReportMemo>("[ReportMemos]");
                    reportMemos = new EntityObservableCollection<ReportMemo>(customerEntities, "ReportMemos", reportMemosQuery);
                }
                return reportMemos;
            }
        }
    }
}
