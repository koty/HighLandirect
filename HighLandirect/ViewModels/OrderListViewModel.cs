using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Printing;
using System.Windows.Controls;
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
        private Store selectedStore;
        private ReportMemo selectedReportMemo;
        private ViewModelCommand printCommand;
        private ViewModelCommand printCommandSagawa;
        private ViewModelCommand printCommandYamatoMany;
        private ViewModelCommand removeCommand;
        private ViewModelCommand addOrderFromSelectedHistoryCommand;
        private ViewModelCommand distinctSameCustomerClickCommand;
        private ViewModelCommand removeOrderFromSelectedHistoryCommand;
        private ViewModelCommand editSelectedCustomerCommand;

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
            this.Orders = new ObservableCollection<OrderViewModel>();
            this.StartFromAtYamatoMany = 1;
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

            //既定で重複を削除
            this.DistinctSameCustomer = true;

            this.stores = stores;
            this.reportMemos = reportMemos;

            this.selectedStore = this.stores.FirstOrDefault(x => x.IsDefault == true);
            this.selectedReportMemo = this.reportMemos.FirstOrDefault(x => x.IsDefault == true);

            this.entityService = entityService;
            CreateOrderCollection(orders);
            this.StartFromAtYamatoMany = 1;
        }

        private void CreateOrderCollection(IEnumerable<Order> orders)
        {
            this.Orders = new ObservableCollection<OrderViewModel>(orders.Select(x => new OrderViewModel(x)));
            this.Orders.CollectionChanged += (o, e) =>
            {
                if (e.OldItems != null)
                    foreach (var item in e.OldItems)
                        this.entityService.Orders.Remove(((OrderViewModel)item).Order);

                if (e.NewItems != null)
                    foreach (var item in e.NewItems)
                        this.entityService.Orders.Add(((OrderViewModel)item).Order);
            };
            if (this.Orders.Any())
            {
                var customer = this.entityService.Customers
                    .First(x => x.CustNo == this.Orders.First().Order.SendCustID);
                this.SendCustomerViewModel = new CustomerViewModel(customer);
                this.ShowOrderHistoryBySendCustomer();
                //var arg = new CustomerListEventArgs() { CustomerViewModel = this.SendCustomerViewModel };
                //this.OnSendCustomerChanged(this, arg); //ここではまだイベント登録されてない。
            }
            this.RaisePropertyChanged(() => this.Orders);
        }

        public bool DistinctSameCustomer { get; set; }

        public OrderViewModel SelectedOrder
        {
            get { return selectedOrder; }
            set
            {
                if (selectedOrder == value) return;
                selectedOrder = value;
                this.RaisePropertyChanged(() => SelectedOrder);
                this.UpdateCommands();
            }
        }

        public Store SelectedStore
        {
            get { return selectedStore; }
            set
            {
                if (selectedStore.Equals(value)) return;
                selectedStore = value;
                this.RaisePropertyChanged(() => SelectedStore);
            }
        }

        public ReportMemo SelectedReportMemo
        {
            get { return selectedReportMemo; }
            set
            {
                if (Equals(selectedReportMemo, value)) return;
                selectedReportMemo = value;
                this.RaisePropertyChanged(() => SelectedReportMemo);
            }
        }

        public int StartFromAtYamatoMany { get; set; }

        public ViewModelCommand PrintCommand
        {
            get
            {
                return this.printCommand
                       ?? (this.printCommand = new ViewModelCommand(this.PrintOrder, this.CanPrintOrder));
            }
        }

        public ViewModelCommand PrintCommandSagawa
        {
            get
            {
                return this.printCommandSagawa
                       ?? (this.printCommandSagawa = new ViewModelCommand(this.PrintOrderSagawa, this.CanPrintOrder));
            }
        }

        public ViewModelCommand PrintCommandYamatoMany
        {
            get
            {
                return this.printCommandYamatoMany
                       ?? (this.printCommandYamatoMany = new ViewModelCommand(this.PrintOrderYamatoMany, this.CanPrintOrder));
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

        #region 注文履歴から顧客を修正する
        public event EventHandler<CustomerListEventArgs> OnEditCustomerButtonClick;

        public ViewModelCommand EditSelectedCustomerCommand
        {
            get
            {
                return this.editSelectedCustomerCommand
                       ?? (this.editSelectedCustomerCommand = new ViewModelCommand(this.EditSelectedCustomer, this.CanEditSelectedCustomerCommand));
            }
        }

        private ViewModelCommand selectedOrderHistoryChangedCommand;

        public ViewModelCommand SelectedOrderHistoryChangedCommand
        {
            get
            {
                return this.selectedOrderHistoryChangedCommand
                       ?? (this.selectedOrderHistoryChangedCommand = new ViewModelCommand(this.selectedOrderHistoryChanged));
            }
        }

        private void EditSelectedCustomer()
        {
            var e = new CustomerListEventArgs()
            {
                CustomerViewModel = new CustomerViewModel(this.OrderHistories.First(x => x.IsSelected).OrderHistory.CustomerMasterReceive)
            };

            OnEditCustomerButtonClick(this, e);
        }

        private void selectedOrderHistoryChanged()
        {
            this.UpdateCommands();
        }
        #endregion

        #region 注文履歴から顧客を新規追加する
        public event EventHandler<CustomerListEventArgs> OnAddNewCustomerButtonClick;
        private ViewModelCommand addNewCustomerCommand;

        public ViewModelCommand AddNewCustomerCommand
        {
            get
            {
                return this.addNewCustomerCommand
                       ?? (this.addNewCustomerCommand = new ViewModelCommand(this.addNewCustomer));
            }
        }

        private void addNewCustomer()
        {
            var e = new CustomerListEventArgs();
            OnAddNewCustomerButtonClick(this, e);
        }

        #endregion
        /**
         * 顧客一覧で依頼主顧客を選択したときのイベント
         */
        internal void AddSendCustomer(object sender, CustomerListEventArgs e)
        {
            this.SendCustomerViewModel = e.CustomerViewModel;
            this.ShowOrderHistoryBySendCustomer();
            //履歴から追加ボタンを有効に
            this.UpdateCommands();
        }

        /**
         * 顧客一覧で受け取り先顧客を選択した時のイベント
         */
        internal void AddResceiveCustomer(object sender, CustomerListEventArgs e)
        {
            this.AddNewOrderCore(e.CustomerViewModel.Customer.CustNo);
            //印刷ボタンを有効に
            this.UpdateCommands();
        }

        private void AddNewOrderCore(int ResceiveCustNo)
        {
            this.SaveChanges();
            //テンポラリにOrderテーブル使っちゃったら同時実行はできんな。。。

            //最大番号プラス1
            long OrderID;
            if (!this.Orders.Any())
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
            const int ProductIdDefaultValue = 2;
            var order = Order.CreateOrder(OrderID, DateTime.Now, ResceiveCustNo,
                                this.SendCustomerViewModel.Customer.CustNo, ProductIdDefaultValue);
            var orderVM = new OrderViewModel(order);
            this.Orders.Add(orderVM);
            this.RaisePropertyChanged(() => this.Orders);
            this.UpdateCommands();

            this.SelectedOrder = orderVM;
        }

        private void AcceptChanges()
        {
            this.entityService.AcceptChanges();
        }

        private void SaveChanges()
        {
            this.entityService.SaveChanges();
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
                this.UpdateCommands();
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
            };
            //↓これが必要な理由が分からん。↑でnewせずに、Clear＆Concatするとリストが更新されない。
            this.RaisePropertyChanged(() => this.OrderHistories);
            this.UpdateCommands();
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
            return (this.OrderHistories != null && this.OrderHistories.Count > 0);
        }

        private bool CanEditSelectedCustomerCommand()
        {
            return (this.OrderHistories != null && this.OrderHistories.Count(x => x.IsSelected) == 1);
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
            this.RaisePropertyChanged(() => this.OrderHistories);
            this.SaveChanges();
            this.UpdateCommands();
        }

        private bool CanPrintOrder() { return this.Orders != null && (this.Orders.Any()); }
        private void PrintOrder()
        {
            PrintOrderCore<ReportYamato>(10m, 4.5m);
        }

        private void PrintOrderSagawa()
        {
            PrintOrderCore<ReportSagawa>(8.5m, 4.0m);
        }

        private void PrintOrderYamatoMany()
        {
            var printer = new PrintListReport<OrderSource, ReportYamatoMany>(10, null);
            var orderSources = getOrderSources();
            //テキストボックス入力値-1個、頭に空行を足す。
            if (this.StartFromAtYamatoMany > 1)
            {
                for (var i = 0; i < this.StartFromAtYamatoMany - 1; i++)
                {
                    orderSources.Insert(0, new OrderSource());
                }
            }
            printer.Print(orderSources, x => new OrderSourceCollection(x));
            UpdateOrderHistory();
        }

        private void PrintOrderCore<ReportType>(decimal widthByInch, decimal heightByInch) where ReportType : UserControl, new()
        {
            var ps = new LocalPrintServer();
            PrintQueue pq = null;
            try
            {
                pq = ps.GetPrintQueue("FUJITSU FMPR5000"); //指定したプリンタ
                pq.UserPrintTicket.PageMediaSize = new PageMediaSize((double)(widthByInch * 96m), (double)(heightByInch * 96m)); //pixcel指定。1pixel=1/96inchだそうで
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

            var printer = new PrintCutSheetReport<OrderSource, ReportType>(pq);

            var orderSources = getOrderSources();

            printer.Print(orderSources);
            UpdateOrderHistory();
        }

        private void UpdateOrderHistory()
        {
            long orderID = 0;
            if (this.entityService.OrderHistories.Any())
            {
                orderID = this.entityService.OrderHistories.Max(x => x.OrderID);
            }
            //OrderからOrderHistoryへレコードをmove
            foreach (var order in this.Orders)
            {
                orderID++;
                this.OrderHistories.Add(new OrderHistoryViewModel(OrderHistory.CreateOrderHistory(orderID,
                                                                                      order.Order.OrderDate,
                                                                                      order.Order.ReceiveCustID,
                                                                                      order.Order.SendCustID,
                                                                                      order.Order.ProductID)));
            }
            //ほんとはここでorderhistoryを並べ替えたい

            //最終発送を更新
            this.SendCustomerViewModel.Customer.LatestSend = DateTime.Now;

            //最終宛先を更新
            foreach (var order in this.Orders)
            {
                this.entityService.Customers.First(x => x.CustNo == order.Order.CustomerMasterReceive.CustNo)
                    .LatestResceive = DateTime.Now;
            }

            // ClearだとCollectionChangedが起きないので一個一個消す
            for(var i = this.Orders.Count - 1; i >= 0; i--)
                this.Orders.RemoveAt(i);

            var arg = new CustomerListEventArgs() { CustomerViewModel = null };
            this.RaisePropertyChanged(() => this.SendCustomerViewModel);
            this.RaisePropertyChanged(() => this.OrderHistories);
            this.RaisePropertyChanged(() => this.Orders);
            this.OnSendCustomerChanged(this, arg);
            this.SaveChanges();
            this.UpdateCommands();
        }

        private List<OrderSource> getOrderSources()
        {
            var orderSources = this.Orders.Select(
                order => new OrderSource(order.Order)
                {
                    CustomerCD = this.SelectedStore.CustomerCD,
                    StoreId1 = this.SelectedStore.StoreId1,
                    StoreId2 = this.SelectedStore.StoreId2,
                    ReportMemo = this.SelectedReportMemo.ReportMemo1
                }).ToList();
            return orderSources;
        }

        private void UpdateCommands()
        {
            this.AddOrderFromSelectedHistoryCommand.RaiseCanExecuteChanged();
            this.RemoveOrderFromSelectedHistoryCommand.RaiseCanExecuteChanged();
            this.PrintCommand.RaiseCanExecuteChanged();
            this.PrintCommandSagawa.RaiseCanExecuteChanged();
            this.PrintCommandYamatoMany.RaiseCanExecuteChanged();
            this.RemoveCommand.RaiseCanExecuteChanged();
            this.EditSelectedCustomerCommand.RaiseCanExecuteChanged();
            this.MoveRowToLowerCommand.RaiseCanExecuteChanged();
            this.MoveRowToUpperCommand.RaiseCanExecuteChanged();
        }

        #region 注文グリッドの上へ下へボタン
        private ViewModelCommand moveRowToUpperCommand;
        public ViewModelCommand MoveRowToUpperCommand
        {
            get
            {
                return this.moveRowToUpperCommand
                       ?? (this.moveRowToUpperCommand = new ViewModelCommand(this.MoveRowToUpper, this.CanMoveRowToUpper));
            }
        }

        private void MoveRowToUpper()
        {
            this.MoveRowCore(true);
        }

        private ViewModelCommand moveRowToLowerCommand;
        public ViewModelCommand MoveRowToLowerCommand
        {
            get
            {
                return this.moveRowToLowerCommand
                       ?? (this.moveRowToLowerCommand = new ViewModelCommand(this.MoveRowToLower, this.CanMoveRowToLower));
            }
        }

        private void MoveRowToLower()
        {
            this.MoveRowCore(false);
        }

        private void MoveRowCore(bool up)
        {
            var orientation = up ? -1 : 1;
            for (var i = 0; i < this.Orders.Count; i++)
            {
                if (this.Orders[i] != this.SelectedOrder) continue;

                //いっこ移動
                this.Orders.Move(i, i + orientation);

                //this.RaisePropertyChanged(() => this.Orders);
                this.UpdateCommands();
                this.AcceptChanges();
                break;
            }
        }

        private bool CanMoveRowToLower()
        {
            return this.SelectedOrder != null && this.Orders != null && this.Orders.LastOrDefault() != this.SelectedOrder;
        }

        private bool CanMoveRowToUpper()
        {
            return this.SelectedOrder != null && this.Orders != null && this.Orders.FirstOrDefault() != this.SelectedOrder;
        }

        #endregion
    }
}
