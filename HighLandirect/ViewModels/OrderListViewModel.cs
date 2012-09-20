using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Printing;
using System.Windows.Input;
using System;
using System.Linq;
using HighLandirect.Domains;
using HighLandirect.Domains.Reports;
using HighLandirect.Foundations;
using HighLandirect.Services;
using Livet;
using Livet.Commands;

namespace HighLandirect.ViewModels
{
    public class OrderListViewModel : ViewModel
    {
        private readonly IEnumerable<Store> stores;
        private readonly IEnumerable<ReportMemo> reportMemos;
        private readonly IEntityService entityService;
        //private readonly ObservableCollection<Order> selectedOrders;
        //private readonly ObservableCollection<OrderHistory> selectedOrderHistories;
        private Order selectedOrder;
        private OrderHistory selectedOrderHistory;
        private Store selectedStore;
        private ReportMemo selectedReportMemo;
        private ViewModelCommand printCommand;
        private ViewModelCommand removeCommand;
        private ViewModelCommand addNewCommand;
        private ViewModelCommand addOrderFromSelectedHistoryCommand;
        //private ICommand filterOrderHistoryCommand;
        private ViewModelCommand showOrderHistoryBySendCustomerCommand;

        public OrderListViewModel()
        {
        }

        public OrderListViewModel(IEntityService entityService,
                                    IEnumerable<Order> orders,
                                    IEnumerable<Store> stores,
                                    IEnumerable<ReportMemo> reportMemos)
        {
            if (orders == null)
            {
                throw new ArgumentNullException("orders");
            }

            //this.selectedOrders = new ObservableCollection<Order>();
            //this.selectedOrderHistories = new ObservableCollection<OrderHistory>();

            this.stores = stores;
            this.reportMemos = reportMemos;

            this.selectedStore = this.stores.FirstOrDefault(x => x.IsDefault == true);
            this.selectedReportMemo = this.reportMemos.FirstOrDefault(x => x.IsDefault == true);
            this.sendCustomerId = "";
            this.entityService = entityService;
            this.OrderHistories = new ObservableCollection<OrderHistory>();
            this.Orders = new ObservableCollection<Order>(orders);
        }

        public ObservableCollection<Order> Orders { get; private set; }
        public ObservableCollection<OrderHistory> OrderHistories { get; private set; }

        //public ObservableCollection<Order> SelectedOrders
        //{
        //    get { return selectedOrders; }
        //}

        //public ObservableCollection<OrderHistory> SelectedOrderHistories
        //{
        //    get { return selectedOrderHistories; }
        //}

        public Order SelectedOrder
        {
            get { return selectedOrder; }
            set
            {
                if (selectedOrder != value)
                {
                    selectedOrder = value;
                    this.RaisePropertyChanged(() => SelectedOrder);
                }
            }
        }

        public OrderHistory SelectedOrderHistory
        {
            get { return selectedOrderHistory; }
            set
            {
                if (selectedOrderHistory != value)
                {
                    selectedOrderHistory = value;
                    this.RaisePropertyChanged(() => SelectedOrderHistory);
                }
            }
        }

        public Store SelectedStore
        {
            get { return selectedStore; }
            set
            {
                if (selectedStore != value)
                {
                    selectedStore = value;
                    this.RaisePropertyChanged(() => SelectedStore);
                }
            }
        }

        public ReportMemo SelectedReportMemo
        {
            get { return selectedReportMemo; }
            set
            {
                if (selectedReportMemo != value)
                {
                    selectedReportMemo = value;
                    this.RaisePropertyChanged(() => SelectedReportMemo);
                }
            }
        }

        public ViewModelCommand PrintCommand
        {
            get
            {
                return this.printCommand
                       ?? (this.printCommand = new ViewModelCommand(this.PrintOrder, this.CanPrintOrder));
            }
        }

        public ViewModelCommand AddNewCommand
        {
            get
            {
                return this.addNewCommand
                       ?? (this.addNewCommand = new ViewModelCommand(this.AddNewOrder));
            }
        }

        public ViewModelCommand RemoveCommand
        {
            get
            {
                return this.removeCommand
                       ?? (this.removeCommand = new ViewModelCommand(this.RemoveOrder, this.CanRemoveOrder));
            }
        }

        public ViewModelCommand AddOrderFromSelectedHistoryCommand
        {
            get
            {
                return this.addOrderFromSelectedHistoryCommand
                       ?? (this.addOrderFromSelectedHistoryCommand = new ViewModelCommand(this.AddOrderFromSelectedHistory));
            }
        }

        public ViewModelCommand ShowOrderHistoryBySendCustomerCommand
        {
            get
            {
                return this.showOrderHistoryBySendCustomerCommand
                       ?? (this.showOrderHistoryBySendCustomerCommand = new ViewModelCommand(this.ShowOrderHistoryBySendCustomer));
            }
        }

        //public ICommand FilterOrderHistoryCommand
        //{
        //    get { return filterOrderHistoryCommand; }
        //    set
        //    {
        //        if (filterOrderHistoryCommand != value)
        //        {
        //            filterOrderHistoryCommand = value;
        //            RaisePropertyChanged("FilterOrderHistoryCommand");
        //        }
        //    }
        //}

        private string sendCustomerId;
        public string SendCustomerId
        {
            get { return sendCustomerId; }
            set
            {
                if (sendCustomerId != value)
                {
                    sendCustomerId = value;
                    this.RaisePropertyChanged(() => SendCustomerId);
                }
            }
        }

        private int sendCustNo;
        public int SendCustNo
        {
            get { return this.sendCustNo; }
            set
            {
                this.sendCustNo = value;
                this.SendCustomerId = this.sendCustNo.ToString();
                this.ShowOrderHistoryBySendCustomer();
                if (this.ResceiveCustNo > 0)
                {
                    this.AddNewOrder();
                }
            }
        }

        private int resceiveCustNo;
        public int ResceiveCustNo
        {
            get { return this.resceiveCustNo; }
            set
            {
                this.resceiveCustNo = value;
                if (this.SendCustNo > 0)
                    this.AddNewOrder();
            }
        }

        private void AddNewOrder()
        {
            //テンポラリにOrderテーブル使っちゃったら同時実行はできんな。。。

            //最大番号プラス1
            long OrderID;
            if (this.Orders == null || !this.Orders.Any())
            {
                //一件目のときは履歴の最大番号
                if (this.OrderHistories == null || !this.OrderHistories.Any())
                {
                    OrderID = 1;
                }
                else
                {
                    OrderID = this.entityService.OrderHistories.Max(x => x.OrderID) + 1;
                }
            }
            else
            {
                //二件目以降は画面のデータを使う
                OrderID = this.entityService.Orders.Max(x => x.OrderID) + 1;
            }
            var order = Order.CreateOrder(OrderID, DateTime.Now, this.ResceiveCustNo, this.SendCustNo, 2);

            this.entityService.Orders.Add(order);

            this.resceiveCustNo = 0;
            this.sendCustNo = 0;

            this.SelectedOrder = order;
        }

        private bool CanRemoveOrder() { return this.SelectedOrder != null; }
        private void RemoveOrder()
        {
            this.entityService.Orders.Remove(this.selectedOrder);
        }

        private void ShowOrderHistoryBySendCustomer()
        {
            //新しい注文履歴50件を表示する
            this.OrderHistories.Clear();
            this.OrderHistories = new ObservableCollection<OrderHistory>(entityService.OrderHistories.Where(
                x => x.CustomerMasterSend.CustNo.ToString() == this.SendCustomerId)
                                           .OrderByDescending(x => x.OrderDate)
                                           .Take(50));
        }

        private void AddOrderFromSelectedHistory()
        {
            int intCustomerId;
            if (!int.TryParse(this.SendCustomerId, out intCustomerId))
            {
                return;
            }
            /*
            foreach (var ResceiveCustomer in this.SelectedOrderHistories)
            {
                this.resceiveCustNo = ResceiveCustomer.ReceiveCustID;
                this.sendCustNo = intCustomerId;
                this.AddNewOrder();
            }
            */ 
            this.resceiveCustNo = this.selectedOrderHistory.ReceiveCustID;
            this.sendCustNo = intCustomerId;
            this.AddNewOrder();
        }

        //private bool CanFilterOrderHistory(object CustomerIdText) { return true; }
        //private void FilterOrderHistory(object CostomerIdText)
        //{
        //    this.orderListViewModel.OrderHistories =
        //            entityService.OrderHistories.Where(x => x.CustomerMasterSend.CustNo.ToString() == CostomerIdText.ToString());
        //}

        private bool CanPrintOrder() { return this.Orders != null && (this.Orders.Any()); }
        private void PrintOrder()
        {
            /*
            if (this.questionService.ShowYesNoQuestion("印刷しますか？"))
            {
                PrintOrderCore();
            }
            */ 
            PrintOrderCore();
        }

        private void PrintOrderCore()
        {

            try
            {
                var ps = new LocalPrintServer();
                PrintQueue pq;
                try
                {
                    pq = ps.GetPrintQueue("FUJITSU FMPR5000"); //指定したプリンタ
                    pq.UserPrintTicket.PageMediaSize = new PageMediaSize(10 * 96, 4.5 * 96); //pixcel指定。1pixel=1/96inchだそうで
                    pq.UserPrintTicket.PageResolution = new PageResolution(96, 96);
                    pq.UserPrintTicket.InputBin = InputBin.Tractor;
                }
                catch (PrintQueueException)
                {
                    try
                    {   //PrimoPDF
                        pq = ps.GetPrintQueue("PrimoPDF");
                    }
                    catch (PrintQueueException)
                    {
                        pq = ps.DefaultPrintQueue; //どれもダメなら既定のプリンタ
                    }
                }

                var printer = new PrintCutSheetReport<OrderSource, ReportYamato>(pq);

                var orderSources = new List<OrderSource>();
                foreach (var order in this.Orders)
                {
                    var orderSource = new OrderSource(order);

                    orderSource.CustomerCD = this.SelectedStore.CustomerCD;
                    orderSource.StoreId1 = this.SelectedStore.StoreId1;
                    orderSource.StoreId2 = this.SelectedStore.StoreId2;
                    orderSource.ReportMemo = this.SelectedReportMemo.ReportMemo1;

                    orderSources.Add(orderSource);
                }

                printer.Print(orderSources);

                //OrderからOrderHistoryへレコードをmove
                foreach (var order in entityService.Orders)
                {
                    this.entityService.OrderHistories.Add(OrderHistory.CreateOrderHistory(order.OrderID,
                                                                                     order.OrderDate,
                                                                                     order.ReceiveCustID,
                                                                                     order.SendCustID,
                                                                                     order.ProductID));
                }

                //最終発送を更新
                this.entityService.Customers.First(x => x.CustNo.ToString() == this.SendCustomerId)
                    .LatestSend = DateTime.Now;
                //最終宛先を更新
                foreach (var order in this.entityService.Orders)
                {
                    this.entityService.Customers.First(x => x.CustNo == order.CustomerMasterReceive.CustNo)
                        .LatestResceive = DateTime.Now;
                }
                this.entityService.Orders.Clear();
            }
            catch
            {
                throw;
            }
        }

        private void UpdateCommands()
        {
            addNewCommand.RaiseCanExecuteChanged();
            removeCommand.RaiseCanExecuteChanged();
        }

        /*
        private void OrderListViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedOrder")
            {
                orderViewModel.Order = orderListViewModel.SelectedOrder;
                UpdateCommands();
            }
        }

        private void OrderViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValid")
            {
                UpdateCommands();
            }
        }
        */ 
    }
}
