using System.Collections.Generic;
using System.Linq;
using HighLandirect.Domains;
using Livet;

namespace HighLandirect.ViewModels
{
    public class OrderViewModel : ViewModel
    {
        public OrderViewModel(Order order)
        {
            this.Order = order;
        }

        private bool isValid = true;
        private Order order;

        public bool IsEnabled
        {
            get { return Order != null; }
        }

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

        /**
         * 画面で使う
         */
        public bool IsSelected { get; set; }

        public bool CanMoveRowToLower
        {
            get { return this.OrderListViewModel != null && this.OrderListViewModel.LastOrDefault() != this; }
        }

        public bool CanMoveRowToUpper
        {
            get { return this.OrderListViewModel != null && this.OrderListViewModel.FirstOrDefault() != this; }
        }

        public IEnumerable<OrderViewModel> OrderListViewModel { set; private get; }
    }
}
