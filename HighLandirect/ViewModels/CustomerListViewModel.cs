using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Printing;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using HighLandirect.Domains;
using HighLandirect.Domains.Reports;
using HighLandirect.Foundations;
using HighLandirect.Services;
using Livet;
using Livet.Commands;
using System.IO;
using System.Text;
using Livet.Messaging;
using Microsoft.VisualBasic;

namespace HighLandirect.ViewModels
{
    public class CustomerListViewModel : ViewModel
    {
        private CustomerViewModel selectedCustomer;
        private ViewModelCommand addNewCommand;
        private ViewModelCommand removeCommand;
        private ViewModelCommand searchCommand;
        private ViewModelCommand addSendCustomerCommand;
        private ViewModelCommand addResceiveCustomerCommand;
        private ListenerCommand<ConfirmationMessage> printAtenaSealCommand;
        private ListenerCommand<ConfirmationMessage> printKokyakuDaichoCommand;
        private ViewModelCommand showDeletedDataCommand;
        private readonly IEntityService entityService;
        private readonly IList<Customer> customers;

        public CustomerListViewModel()
        {
        }

        public CustomerListViewModel(IEnumerable<Customer> customers, IEntityService entityService)
        {
            if (customers == null) { throw new ArgumentNullException("customers"); }
            this.customers = customers.ToList();

            this.entityService = entityService;

            this.ShowDeletedData = false;
            this.ShowDeletedDataFunc();
            this.SelectedCustomer = this.CustomerViewModels.FirstOrDefault();
        }

        public ObservableCollection<CustomerViewModel> CustomerViewModels { get; private set; }

        public CustomerViewModel SelectedCustomer
        {
            get { return selectedCustomer; }
            set
            {
                if (value == null) return;
                if (selectedCustomer == null
                        || selectedCustomer.Customer.CustNo != value.Customer.CustNo)
                {
                    selectedCustomer = value;
                    RaisePropertyChanged(() => SelectedCustomer);
                }
            }
        }

        public string SearchString { get; set; }

        #region Commands
        public ViewModelCommand AddNewCommand
        {
            get
            {
                return this.addNewCommand
                       ?? (this.addNewCommand = new ViewModelCommand(this.AddNewCustomer, this.CanAddCustomer));
            }
        }

        public ViewModelCommand RemoveCommand
        {
            get
            {
                return this.removeCommand
                       ?? (this.removeCommand = new ViewModelCommand(this.RemoveCustomer, this.CanRemoveCustomer));
            }
        }

        public ViewModelCommand SearchCommand
        {
            get
            {
                return this.searchCommand
                       ?? (this.searchCommand = new ViewModelCommand(this.SearchCustomer, this.CanSearchCustomer));
            }
        }

        public ViewModelCommand AddSendCustomerCommand
        {
            get
            {
                return this.addSendCustomerCommand
                       ?? (this.addSendCustomerCommand = new ViewModelCommand(this.AddSendCustomer, this.CanAddSendCustomerFunc));
            }
        }

        public ViewModelCommand AddResceiveCustomerCommand
        {
            get
            {
                return this.addResceiveCustomerCommand
                       ?? (this.addResceiveCustomerCommand = new ViewModelCommand(this.AddResceiveCustomer, this.CanAddResceiveCustomerFunc));
            }
        }

        public ListenerCommand<ConfirmationMessage> PrintAtenaSealCommand
        {
            get
            {
                return this.printAtenaSealCommand
                       ?? (this.printAtenaSealCommand = new ListenerCommand<ConfirmationMessage>(this.PrintAtenaSeal, this.CanPrintAtenaSeal));
            }
        }

        public ListenerCommand<ConfirmationMessage> PrintKokyakuDaichoCommand
        {
            get
            {
                return this.printKokyakuDaichoCommand
                       ?? (this.printKokyakuDaichoCommand = new ListenerCommand<ConfirmationMessage>(this.PrintKokyakuDaicho, this.CanPrintKokyakuDaicho));
            }
        }

        public ViewModelCommand ShowDeletedDataCommand
        {
            get
            {
                return this.showDeletedDataCommand
                       ?? (this.showDeletedDataCommand = new ViewModelCommand(this.ShowDeletedDataFunc));
            }
        }

        #endregion

        public bool? ShowDeletedData { get; set; }
        public void ShowDeletedDataFunc()
        {
            RefreshCustomerViewModels();
        }

        private void RefreshCustomerViewModels()
        {
            //whereは遅延実行なのでコンストラクタに書いても良いよーな気がするが。。。
            var filterdCustomers = this.ShowDeletedData == true
                                    ? customers
                                    : this.customers.Where(x => x.Delete != true);
            this.CustomerViewModels = new ObservableCollection<CustomerViewModel>(filterdCustomers.Select(x => new CustomerViewModel(x)));
            foreach(var customer in this.CustomerViewModels)
            {
                customer.CustomerViewModels = this.CustomerViewModels;
            }

            this.CustomerViewModels.CollectionChanged += (o, e) =>
            {
                if (e.NewItems != null)
                    foreach (var item in e.NewItems)
                        this.entityService.Customers.Add(((CustomerViewModel)item).Customer);
                if (e.OldItems != null)
                    foreach (var item in e.OldItems)
                        this.entityService.Customers.Remove(((CustomerViewModel)item).Customer);
            };
            this.RaisePropertyChanged(() => this.CustomerViewModels);
        }

        private bool CanAddCustomer() { return true; }
        private void AddNewCustomer()
        {
            //最大番号プラス1
            var CustNo = 1;
            if (this.entityService.Customers.Count > 1)
            {
                var firstOrDefault = this.entityService.Customers.OrderByDescending(x => x.CustNo).FirstOrDefault();
                if (firstOrDefault != null)
                    CustNo = firstOrDefault.CustNo + 1;
            }
            var customer = new Customer(CustNo) { Label = false, Delete = false };
            var customerVM = new CustomerViewModel(customer) { CustomerViewModels = this.CustomerViewModels };

            this.CustomerViewModels.Add(customerVM);
            this.RaisePropertyChanged(() => this.CustomerViewModels);
            this.SelectedCustomer = customerVM;
        }

        private bool CanRemoveCustomer() { return this.SelectedCustomer != null; }
        private void RemoveCustomer()
        {
            this.CustomerViewModels.Remove(this.SelectedCustomer);
        }

        private bool CanSearchCustomer() { return true; }

        private string _CachedSearchString = "";
        private CustomerViewModel[] _FoundCustomers;
        private int _FoundCustomersIndex;

        private void SearchCustomer()
        {
            if (this._CachedSearchString != this.SearchString)
            {
                this._CachedSearchString = this.SearchString;
                this._FoundCustomers = SearchCustomer(this.SearchString, this.CustomerViewModels);
                this._FoundCustomersIndex = -1;
            }

            if (this._FoundCustomers.Length == 0)
            {
                this.SelectedCustomer = null;
                Messenger.Raise(new InteractionMessage("NoOneFound"));
                return;
            }

            this._FoundCustomersIndex++;
            if (this._FoundCustomersIndex >= this._FoundCustomers.Length)
            {
                this._FoundCustomersIndex = 0;
            }
            this.SelectedCustomer = _FoundCustomers[this._FoundCustomersIndex];
        }

        /// <summary>
        /// とりあえずCustNameでの検索。電話番号、ふりがなでの検索も可能とする
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="customerViewModels"></param>
        /// <returns></returns>
        private static CustomerViewModel[] SearchCustomer(string searchString, IEnumerable<CustomerViewModel> customerViewModels)
        {
            int number;
            var re = new Regex("[０-９Ａ-Ｚａ-ｚ：－　]+");
            var searchStringNormalized = re.Replace(searchString, m => Strings.StrConv(m.Value, VbStrConv.Narrow));

            //顧客番号
            if (int.TryParse(searchStringNormalized, out number))
                return customerViewModels.Where(x => x.Customer.CustNo == number).ToArray();

            //電話番号
            if (int.TryParse(searchStringNormalized.Replace("-", ""), out number))
                return customerViewModels.Where(x => x.Customer.Phone.IndexOf(searchStringNormalized) >= 0).ToArray();

            //ふりがな
            if (IsHiragana(searchStringNormalized))
                return customerViewModels.Where(x => x.Customer.Furigana.IndexOf(searchStringNormalized) >= 0).ToArray();

            //漢字氏名
            return customerViewModels.Where(x => x.Customer.CustName.IndexOf(searchStringNormalized) >= 0).ToArray();

        }

        static bool IsHiragana(string str)
        {
            return Regex.IsMatch(str, @"^\p{IsHiragana}*$");
        }

        private void UpdateCommands()
        {
            this.AddNewCommand.RaiseCanExecuteChanged();
            this.RemoveCommand.RaiseCanExecuteChanged();
            this.AddSendCustomerCommand.RaiseCanExecuteChanged();
            this.AddResceiveCustomerCommand.RaiseCanExecuteChanged();
        }

        public EventHandler<CustomerListEventArgs> OnSendCustomerAdded;
        public EventHandler<CustomerListEventArgs> OnResceiveCustomerAdded;

        private bool _CanAddSendCustomer;
        public bool CanAddSendCustomer
        {
            get { return this._CanAddSendCustomer; }
            set
            {
                this._CanAddSendCustomer = value;
                this.UpdateCommands();
            }
        }

        private bool CanAddSendCustomerFunc() { return this.CanAddSendCustomer; }
        private void AddSendCustomer()
        {
            var arg = new CustomerListEventArgs() {CustomerViewModel = this.SelectedCustomer };
            this.CanAddSendCustomer = false;
            this.OnSendCustomerAdded(this, arg);
        }

        private bool CanAddResceiveCustomerFunc() { return !this.CanAddSendCustomer; }
        private void AddResceiveCustomer()
        {
            var arg = new CustomerListEventArgs() {CustomerViewModel = this.SelectedCustomer };

            this.OnResceiveCustomerAdded(this, arg);
        }

        internal void ChangeCanAddSendCustomer(Object o, CustomerListEventArgs arg)
        {
            //CustomerViewModelが無い場合は送信者を追加できる。
            this.CanAddSendCustomer = arg.CustomerViewModel == null;
        }

        #region 印刷関係

        private bool CanPrintAtenaSeal()
        {
            return this.CustomerViewModels.Any(customerVM => customerVM.Customer.Label == true);
        }

        private bool CanPrintKokyakuDaicho()
        {
            return this.CustomerViewModels.Any(customerVM => customerVM.Customer.Label == true);
        }

        private void PrintAtenaSeal(ConfirmationMessage parameter)
        {
            if (parameter.Response == true)
            {
                PrintAtenaSealCore();
            }
        }

        private void PrintKokyakuDaicho(ConfirmationMessage parameter)
        {
            if (parameter.Response == true)
            {
                PrintKokyakuDaichoCore();
            }
        }

        private void PrintAtenaSealCore()
        {
            var pq = GetPrintQueue();

            var printer = new PrintListReport<Customer, AtenaSeal>(12, pq);
            //ラベル印刷するチェックが入ってる人
            printer.Print(this.CustomerViewModels
                              .Where(customerVM => customerVM.Customer.Label == true)
                              .OrderBy(customerVM => customerVM.Customer.Furigana)
                              .Select(customerVM => customerVM.Customer),
                              x => new CustomerSource(x.Cast<Customer>()));
        }

        private void PrintKokyakuDaichoCore()
        {
            var pq = GetPrintQueue();

            var printer = new PrintListReport<Customer, KokyakuDaicho>(17, pq);

            printer.Print(this.CustomerViewModels
                              .Where(customerVM => customerVM.Customer.Label == true)
                              .OrderBy(customerVM => customerVM.Customer.CustNo)
                              .Select(customerVM => customerVM.Customer),
                              x => new CustomerSource(x.Cast<Customer>()));
        }

        private static PrintQueue GetPrintQueue()
        {
            return null;
            /*
            var ps = new LocalPrintServer();
            var pq = ps.DefaultPrintQueue; //既定のプリンタ
            //A4縦
            pq.UserPrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
            pq.UserPrintTicket.PageOrientation = PageOrientation.Portrait;
            return pq;
            */
        }
        #endregion

        internal bool SetImportData(string filePath)
        {
            var ExistsError = false;
            using (var sr = new StreamReader(filePath, Encoding.UTF8))
            using (var sw = new StreamWriter("ErrorList.txt", false, Encoding.UTF8))
            {
                var Line = sr.ReadLine();
                //ヘッダが設定されてたら読み捨てる
                if (Line == null || Line.Trim() == Customer.HeaderCsv)
                    Line = sr.ReadLine();
                var i = 0;
                var CustNo = GetMaxCustNo();

                while (Line != null)
                {
                    i++;
                    if (Line.Trim().Length > 0)
                    {
                        try
                        {
                            CustNo++;
                            var customer = Customer.GetCustomerFromCsv(Line, CustNo);
                            //ほんとはObservableCollection<CustomerViewModels> にAddすれば、entityServiceにAddする必要はないんだろうが。。。
                            this.customers.Add(customer);
                            this.entityService.Customers.Add(customer);
                        }
                        catch (Exception ex)
                        {
                            sw.WriteLine("{0}行目の{1}を設定中『{2}』のためエラーが発生しました。この行はインポートされません。",
                                        i.ToString(), ex.Source, ex.Message);
                            ExistsError = true;
                        }
                    }
                    Line = sr.ReadLine();
                }
            }

            this.RefreshCustomerViewModels();

            return ExistsError;
        }

        private int GetMaxCustNo()
        {
            int CustNo = 0;
            if (this.CustomerViewModels.Count > 0)
            {
                CustNo = this.CustomerViewModels.Select(x => x.Customer)
                    .Max(x => x.CustNo);
            }
            return CustNo;
        }

        internal void SetSelectedCustomer(object sender, CustomerListEventArgs e)
        {
            this.SelectedCustomer = e.CustomerViewModel;
        }
        internal void AddNewCustomer(object sender, CustomerListEventArgs e)
        {
            this.AddNewCustomer();
        }
    }
}
