using HighLandirect.Domains;
using Livet;

namespace HighLandirect.ViewModels
{
    public class OrderViewModel : ViewModel
    {
        private bool isValid = true;
        private Order order;

        public bool IsEnabled { get { return Order != null; } }

        public bool IsValid
        {
            get { return isValid; }
            set
            {
                if (isValid != value)
                {
                    isValid = value;
                    RaisePropertyChanged("IsValid");
                }
            }
        }

        public Order Order
        {
            get { return order; }
            set
            {
                if (order != value)
                {
                    order = value;
                    RaisePropertyChanged("Order");
                    RaisePropertyChanged("IsEnabled");
                }
            }
        }
    }
}
