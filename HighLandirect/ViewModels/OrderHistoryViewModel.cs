using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;
using HighLandirect.Domains;



namespace HighLandirect.ViewModels
{
    public class OrderHistoryViewModel : ViewModel
    {

        public OrderHistoryViewModel(OrderHistory orderhistory)
        {
            this.OrderHistory = orderhistory;
        }

        private bool isValid = true;
        private OrderHistory orderhistory;

        public bool IsEnabled { get { return OrderHistory != null; } }

        public bool IsValid
        {
            get { return isValid; }
            set
            {
                if (isValid != value)
                {
                    isValid = value;
                    RaisePropertyChanged(() => IsValid);
                }
            }
        }

        public OrderHistory OrderHistory
        {
            get { return orderhistory; }
            set
            {
                if (orderhistory != value)
                {
                    orderhistory = value;
                    RaisePropertyChanged(() => OrderHistory);
                    RaisePropertyChanged(() => IsEnabled);
                }
            }
        }

        /**
         * 画面で使う
         */
        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (this.isSelected == value) return;
                this.isSelected = value;
                this.RaisePropertyChanged(() => this.IsSelected);
            }
        }
    }
}
