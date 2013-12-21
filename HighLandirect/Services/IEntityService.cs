using System.Collections.ObjectModel;
using HighLandirect.Domains;
using Customer = HighLandirect.Domains.Customer;

namespace HighLandirect.Services
{
    public interface IEntityService
    {
        ObservableCollection<Product> Products { get; }

        ObservableCollection<Customer> Customers { get; }

        ObservableCollection<Order> Orders { get; }

        ObservableCollection<OrderHistory> OrderHistories { get; }

        ObservableCollection<Store> Stores { get; }

        ObservableCollection<ReportMemo> ReportMemos { get; }

        void AcceptChanges();

        void SaveChanges();
    }
}
