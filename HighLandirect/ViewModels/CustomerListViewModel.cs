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

namespace HighLandirect.ViewModels
{
    public class CustomerListViewModel : ViewModel
    {
        private Customer selectedCustomer;
        private ViewModelCommand addNewCommand;
        private ViewModelCommand removeCommand;
        private ViewModelCommand searchCommand;
        private ViewModelCommand addSendCustomerCommand;
        private ViewModelCommand addResceiveCustomerCommand;
        private ViewModelCommand printAtenaSealCommand;
        private ViewModelCommand printKokyakuDaichoCommand;
        private bool showDeletedData;
        private readonly IEntityService entityService;
        private readonly ShellViewModel shellVM;

        public CustomerListViewModel()
        {
        }

        public CustomerListViewModel(ShellViewModel shellVM,
            IEnumerable<Customer> customers, IEntityService entityService)
        {
            if (customers == null) { throw new ArgumentNullException("customers"); }

            var customerViewModelList = customers.Select(customer => new CustomerViewModel(this, customer));
            this.CustomerViewModels = new ObservableCollection<CustomerViewModel>(customerViewModelList);
            this.entityService = entityService;
            this.shellVM = shellVM;
            this.HideDeletedData();
        }

        public void ScrollIntoView()
        {
            //var view = this.customerListView as ContentControl;
            //var grid = ((Panel)view.Content).Children[1] as DataGrid;
            //grid.ScrollIntoView(this.selectedCustomer);
        }


        public ObservableCollection<CustomerViewModel> CustomerViewModels { get; private set; }

        public Customer SelectedCustomer
        {
            get { return selectedCustomer; }
            set
            {
                if (selectedCustomer != value)
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
                       ?? (this.addSendCustomerCommand = new ViewModelCommand(this.AddSendCustomer, this.CanAddSendCustomer));
            }
        }

        public ViewModelCommand AddResceiveCustomerCommand
        {
            get
            {
                return this.addResceiveCustomerCommand
                       ?? (this.addResceiveCustomerCommand = new ViewModelCommand(this.AddResceiveCustomer, this.CanAddResceiveCustomer));
            }
        }

        public ViewModelCommand PrintAtenaSealCommand
        {
            get
            {
                return this.printAtenaSealCommand
                       ?? (this.printAtenaSealCommand = new ViewModelCommand(this.PrintAtenaSeal, this.CanPrintAtenaSeal));
            }
        }

        public ViewModelCommand PrintKokyakuDaichoCommand
        {
            get
            {
                return this.printKokyakuDaichoCommand
                       ?? (this.printKokyakuDaichoCommand = new ViewModelCommand(this.PrintKokyakuDaicho, this.CanPrintKokyakuDaicho));
            }
        }
        #endregion

        public void ShowDeletedData()
        {
            this.showDeletedData = true;
            RefreshCustomerViewModels();
        }

        public void HideDeletedData()
        {
            this.showDeletedData = false;
            RefreshCustomerViewModels();
        }

        private void RefreshCustomerViewModels()
        {
            //whereは遅延実行なのでコンストラクタに書いても良いよーな気がするが。。。
            this.CustomerViewModels =
                new ObservableCollection<CustomerViewModel>(
                    this.CustomerViewModels.Where(customerVM => customerVM.Customer.Delete == this.showDeletedData));
            this.RaisePropertyChanged(() => this.CustomerViewModels);
        }

        private bool CanAddCustomer() { return true; }
        private void AddNewCustomer()
        {
            //最大番号プラス1
            int CustNo;
            if (this.entityService.Customers.Count == 0)
            {
                CustNo = 1;
            }
            else
            {
                CustNo = this.entityService.Customers
                                    .OrderByDescending(x => x.CustNo)
                                    .FirstOrDefault().CustNo + 1;
            }
            var customer = new Customer(CustNo);
            customer.Label = false;
            customer.Delete = false;
            entityService.Customers.Add(customer);

            this.SelectedCustomer = customer;
        }

        private bool CanRemoveCustomer() { return this.SelectedCustomer != null; }
        private void RemoveCustomer()
        {
            this.entityService.Customers.Remove(this.SelectedCustomer);
        }

        private bool CanSearchCustomer() { return true; }
        private void SearchCustomer()
        {
            //とりあえずCustNameでの検索。電話番号、ふりがなでの検索も可能とする

            var FoundCustomer = GetCustomerQuery(this.SearchString);
            this.SelectedCustomer = FoundCustomer;

            //スクロールバーを移動する
            this.ScrollIntoView();
        }

        private Customer GetCustomerQuery(string searchString)
        {
            Customer FoundCustomer;
            int dummy;
            if (int.TryParse(SearchString.Replace("-", ""), out dummy))
            {
                FoundCustomer = this.entityService.Customers.FirstOrDefault(x => x.Phone.IndexOf(SearchString) >= 0);
            }
            else if (IsHiragana(searchString))
            {
                FoundCustomer = this.entityService.Customers.FirstOrDefault(x => x.Furigana.IndexOf(SearchString) >= 0);
            }
            else
            {
                FoundCustomer = this.entityService.Customers.FirstOrDefault(x => x.CustName.IndexOf(SearchString) >= 0);
            }
            return FoundCustomer;
        }

        static bool IsHiragana(string str)
        {
            return Regex.IsMatch(str, @"^\p{IsHiragana}*$");
        }

        private void UpdateCommands()
        {
            addNewCommand.RaiseCanExecuteChanged();
            removeCommand.RaiseCanExecuteChanged();
        }
        /*
        private void CustomerListViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedCustomer")
            {
                customerViewModel.Customer = customerListViewModel.SelectedCustomer;
                UpdateCommands();
            }
        }

        private void CustomerViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValid")
            {
                UpdateCommands();
            }
        }
        */
        private bool CanAddSendCustomer() { return true; }
        private void AddSendCustomer()
        {
            this.shellVM.SetSendCustNo(this.SelectedCustomer.CustNo);
        }

        private bool CanAddResceiveCustomer() { return true; }
        private void AddResceiveCustomer()
        {
            this.shellVM.SetResceiveCustNo(this.SelectedCustomer.CustNo);
        }

        private bool CanPrintAtenaSeal()
        {
            return this.CustomerViewModels.Any(customerVM => customerVM.Customer.Label == true);
        }
        private void PrintAtenaSeal()
        {
            PrintAtenaSealCore();
        }

        private bool CanPrintKokyakuDaicho()
        {
            return this.CustomerViewModels.Any(customerVM => customerVM.Customer.Label == true);
        }
        private void PrintKokyakuDaicho()
        {
            PrintKokyakuDaichoCore();
        }

        private void PrintAtenaSealCore()
        {
            var ps = new LocalPrintServer();
            var pq = ps.DefaultPrintQueue; //既定のプリンタ
            //A4縦
            pq.UserPrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
            pq.UserPrintTicket.PageOrientation = PageOrientation.Portrait;

            var printer = new PrintListReport<Customer, AtenaSeal>(4, pq);
            //ラベル印刷するチェックが入ってる人
            printer.Print(this.CustomerViewModels.Where(customerVM => customerVM.Customer.Label == true)
                              .Select(customerVM => customerVM.Customer));
        }

        private void PrintKokyakuDaichoCore()
        {
            var ps = new LocalPrintServer();
            var pq = ps.DefaultPrintQueue; //既定のプリンタ
            //A4縦
            pq.UserPrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
            pq.UserPrintTicket.PageOrientation = PageOrientation.Portrait;

            var printer = new PrintListReport<Customer, KokyakuDaicho>(10, pq);
            //ラベル印刷するチェックが入ってる人
            printer.Print(this.CustomerViewModels.Where(customerVM => customerVM.Customer.Label == true)
                              .Select(customerVM => customerVM.Customer));
        }
    }
}
