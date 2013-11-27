﻿using System.Collections.Generic;
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
        private OrderViewModel selectedOrder;
        private OrderHistory selectedOrderHistory;
        private Store selectedStore;
        private ReportMemo selectedReportMemo;
        private ViewModelCommand printCommand;
        private ViewModelCommand removeCommand;
        private ViewModelCommand addOrderFromSelectedHistoryCommand;
        private ViewModelCommand distinctSameCustomerClickCommand;
        private ViewModelCommand removeOrderFromSelectedHistoryCommand;

        private CustomerViewModel sendCustomerViewModel;
        public CustomerViewModel SendCustomerViewModel
        {
            get { return this.sendCustomerViewModel; }
            private set
            {
                if (this.sendCustomerViewModel == value) return;
                this.sendCustomerViewModel = value;
                this.RaisePropertyChanged(() => this.SendCustomerViewModel);
            }
        }

        public ObservableCollection<OrderViewModel> Orders { get; private set; }
        public ObservableCollection<OrderHistoryViewModel> OrderHistories { get; private set; }

        public EventHandler<CustomerListEventArgs> OnSendCustomerChanged;

        public OrderListViewModel()
        {
        }

        public OrderListViewModel(IEntityService entityService,
                                    IEnumerable<Order> orders,
                                    IEnumerable<Store> stores,
                                    IEnumerable<OrderHistory> histories,
                                    IEnumerable<ReportMemo> reportMemos)
        {
            if (orders == null)
            {
                throw new ArgumentNullException("orders");
            }

            //既定で重複を削除
            this.DistinctSameCustomer = true;

            this.stores = stores;
            this.reportMemos = reportMemos;

            this.selectedStore = this.stores.FirstOrDefault(x => x.IsDefault == true);
            this.selectedReportMemo = this.reportMemos.FirstOrDefault(x => x.IsDefault == true);

            this.entityService = entityService;
            this.Orders = new ObservableCollection<OrderViewModel>(orders.Select(x => new OrderViewModel(x)));
            this.Orders.CollectionChanged += (o, e) =>
            {
                if (e.NewItems != null)
                    foreach (var item in e.NewItems)
                        this.entityService.Orders.Add(((OrderViewModel)item).Order);
                if (e.OldItems != null)
                    foreach (var item in e.OldItems)
                        this.entityService.Orders.Remove(((OrderViewModel)item).Order);
            };
            if (this.Orders.Any())
            {
                var customer = this.entityService.Customers.Where(x => x.CustNo == this.Orders.First().Order.SendCustID).First();
                this.SendCustomerViewModel = new CustomerViewModel(customer);
                this.ShowOrderHistoryBySendCustomer();
                var arg = new CustomerListEventArgs() { CustomerViewModel = this.SendCustomerViewModel };
                //this.OnSendCustomerChanged(this, arg); //ここではまだイベント登録されてない。
            }
        }

        public bool DistinctSameCustomer { get; set; }

        public OrderViewModel SelectedOrder
        {
            get { return selectedOrder; }
            set
            {
                if (selectedOrder != value)
                {
                    selectedOrder = value;
                    this.RaisePropertyChanged(() => SelectedOrder);
                    this.RemoveCommand.RaiseCanExecuteChanged();
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

        public ViewModelCommand DistinctSameCustomerClickCommand
        {
            get
            {
                return this.distinctSameCustomerClickCommand
                       ?? (this.distinctSameCustomerClickCommand = new ViewModelCommand(this.ShowOrderHistoryBySendCustomer, this.CanAddOrderFromSelectedHistory));
            }
        }

        public ViewModelCommand RemoveOrderFromSelectedHistoryCommand
        {
            get
            {
                return this.removeOrderFromSelectedHistoryCommand
                       ?? (this.removeOrderFromSelectedHistoryCommand = new ViewModelCommand(this.RemoveOrderFromSelectedHistory, this.CanAddOrderFromSelectedHistory));
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
                       ?? (this.addOrderFromSelectedHistoryCommand = new ViewModelCommand(this.AddOrderFromSelectedHistory, this.CanAddOrderFromSelectedHistory));
            }
        }

        /**
         * 顧客一覧で依頼主顧客を選択したときのイベント
         */
        internal void AddSendCustomer(object sender, CustomerListEventArgs e)
        {
            this.SendCustomerViewModel = e.CustomerViewModel;
            this.ShowOrderHistoryBySendCustomer();
            //履歴から追加ボタンを有効に
            this.AddOrderFromSelectedHistoryCommand.RaiseCanExecuteChanged();
        }

        /**
         * 顧客一覧で受け取り先顧客を選択した時のイベント
         */
        internal void AddResceiveCustomer(object sender, CustomerListEventArgs e)
        {
            this.AddNewOrderCore(e.CustomerViewModel.Customer.CustNo);
            //印刷ボタンを有効に
            this.PrintCommand.RaiseCanExecuteChanged();
        }

        private void AddNewOrderCore(int ResceiveCustNo)
        {
            ((EntityService)this.entityService).Customerentities.SaveChanges();
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
                OrderID = this.Orders.Max(x => x.Order.OrderID) + 1;
            }
            var ProductIdDefaultValue = 2;
            var order = Order.CreateOrder(OrderID, DateTime.Now, ResceiveCustNo,
                                this.SendCustomerViewModel.Customer.CustNo, ProductIdDefaultValue);
            var orderVM = new OrderViewModel(order);
            this.Orders.Add(orderVM);
            this.RaisePropertyChanged(() => this.Orders);
            this.RemoveCommand.RaiseCanExecuteChanged();
            this.PrintCommand.RaiseCanExecuteChanged();

            this.SelectedOrder = orderVM;
        }

        private bool CanRemoveOrder() { return this.SelectedOrder != null; }
        private void RemoveOrder()
        {
            this.Orders.Remove(this.selectedOrder);
            if (!this.Orders.Any())
            {
                this.SendCustomerViewModel = null;
                var arg = new CustomerListEventArgs() { CustomerViewModel = null };
                this.OnSendCustomerChanged(this, arg);
                this.OrderHistories.Clear();
                this.AddOrderFromSelectedHistoryCommand.RaiseCanExecuteChanged();
                this.PrintCommand.RaiseCanExecuteChanged();
            }
        }

        private void ShowOrderHistoryBySendCustomer()
        {
            IEnumerable<OrderHistoryViewModel> list;
            if (this.DistinctSameCustomer)
            {
                list = entityService.OrderHistories.Where(
                    x => x.CustomerMasterSend.CustNo == this.SendCustomerViewModel.Customer.CustNo)
                                               .OrderByDescending(x => x.OrderDate)
                                               .Distinct(new ReceiveCustomerEqualityComparer())
                                               .Select(x => new OrderHistoryViewModel(x));
            }
            else
            {
                list = entityService.OrderHistories.Where(
                    x => x.CustomerMasterSend.CustNo == this.SendCustomerViewModel.Customer.CustNo)
                                               .OrderByDescending(x => x.OrderDate)
                                               .Select(x => new OrderHistoryViewModel(x));
            }

            this.OrderHistories = new ObservableCollection<OrderHistoryViewModel>(list);
            this.OrderHistories.CollectionChanged += (o, e) =>
            {
                if (e.NewItems != null)
                    foreach (var item in e.NewItems)
                        this.entityService.OrderHistories.Add(((OrderHistoryViewModel)item).OrderHistory);
                if (e.OldItems != null)
                    foreach (var item in e.OldItems)
                        this.entityService.OrderHistories.Remove(((OrderHistoryViewModel)item).OrderHistory);
            };            //↓これが必要な理由が分からん。↑でnewせずに、Clear＆Concatするとリストが更新されない。
            this.RaisePropertyChanged(() => this.OrderHistories);
            this.RefreshHistoryCommand();
        }

        /// <summary>
        /// 注文履歴を送付先顧客ごとに一意にするためのIEqualityComparer
        /// </summary>
        class ReceiveCustomerEqualityComparer : IEqualityComparer<OrderHistory>
        {

            public bool Equals(OrderHistory x, OrderHistory y)
            {
                return x.ReceiveCustID == y.ReceiveCustID;
            }

            public int GetHashCode(OrderHistory obj)
            {
                return obj.ReceiveCustID.GetHashCode();
            }
        }

        private bool CanAddOrderFromSelectedHistory()
        {
            return this.OrderHistories.Count > 0;
        }

        private void AddOrderFromSelectedHistory()
        {
            foreach (var ResceiveCustomer in this.OrderHistories.Where(x => x.IsSelected))
            {
                this.AddNewOrderCore(ResceiveCustomer.OrderHistory.ReceiveCustID);
            }
        }

        private void RemoveOrderFromSelectedHistory()
        {
            for (var i = this.OrderHistories.Count - 1; i >= 0; i--)
            {
                if (this.OrderHistories[i].IsSelected)
                {
                    this.OrderHistories.RemoveAt(i);
                }
            }
            this.RefreshHistoryCommand();
        }

        private void RefreshHistoryCommand()
        {
            this.AddOrderFromSelectedHistoryCommand.RaiseCanExecuteChanged();
            this.RemoveOrderFromSelectedHistoryCommand.RaiseCanExecuteChanged();
        }

        private bool CanPrintOrder() { return this.Orders != null && (this.Orders.Any()); }
        private void PrintOrder()
        {
            PrintOrderCore();
        }

        private void PrintOrderCore()
        {

            try
            {
                var ps = new LocalPrintServer();
                PrintQueue pq = null;
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
                        //pq = ps.GetPrintQueue("PrimoPDF");
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
                    var orderSource = new OrderSource(order.Order);

                    orderSource.CustomerCD = this.SelectedStore.CustomerCD;
                    orderSource.StoreId1 = this.SelectedStore.StoreId1;
                    orderSource.StoreId2 = this.SelectedStore.StoreId2;
                    orderSource.ReportMemo = this.SelectedReportMemo.ReportMemo1;

                    orderSources.Add(orderSource);
                }

                printer.Print(orderSources);
                long orderID = 0;
                if (this.entityService.OrderHistories.Any())
                {
                    orderID = this.entityService.OrderHistories.Max(x => x.OrderID);
                }
                //OrderからOrderHistoryへレコードをmove
                foreach (var order in entityService.Orders)
                {
                    orderID++;
                    this.entityService.OrderHistories.Add(OrderHistory.CreateOrderHistory(orderID,
                                                                                     order.OrderDate,
                                                                                     order.ReceiveCustID,
                                                                                     order.SendCustID,
                                                                                     order.ProductID));
                }

                //最終発送を更新
                this.SendCustomerViewModel.Customer.LatestSend = DateTime.Now;

                //最終宛先を更新
                foreach (var order in this.entityService.Orders)
                {
                    this.entityService.Customers.First(x => x.CustNo == order.CustomerMasterReceive.CustNo)
                        .LatestResceive = DateTime.Now;
                }
                
                //ここもentityServiceとOrdersの両方をClearする必要はない気がする。
                this.entityService.Orders.Clear();
                this.Orders.Clear();

                var arg = new CustomerListEventArgs() { CustomerViewModel = null };
                this.OnSendCustomerChanged(this, arg);
                this.PrintCommand.RaiseCanExecuteChanged();
                this.RemoveCommand.RaiseCanExecuteChanged();
            }
            catch
            {
                throw;
            }
        }

        private void UpdateCommands()
        {
            //addNewCommand.RaiseCanExecuteChanged();
            removeCommand.RaiseCanExecuteChanged();
        }
    }
}
