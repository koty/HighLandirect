using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using HighLandirect.Domains;
using HighLandirect.Services;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using System.Windows;
using Livet.Messaging.IO;
using System.Collections.Generic;
using System.Collections;

namespace HighLandirect.ViewModels
{
    public class ShellViewModel : ViewModel
    {
        private ViewModelCommand exitCommand;
        private ViewModelCommand aboutCommand;

        private readonly EntityService entityService;

        private readonly ProductListViewModel productListViewModel;
        private readonly CustomerListViewModel customerListViewModel;
        private readonly OrderListViewModel orderListViewModel;

        private bool isValid = true;

        public static string Title { get { return "HighLandirect"; } }

        public ShellViewModel()
        {
            entityService = new EntityService();
            entityService.Customerentities = new MyDataEntities();

            this.productListViewModel = new ProductListViewModel(entityService.Products, entityService);
            this.customerListViewModel = new CustomerListViewModel(entityService.Customers, entityService);
            this.orderListViewModel = new OrderListViewModel(entityService,
                                                             entityService.Orders,
                                                             entityService.Stores,
                                                             entityService.OrderHistories,
                                                             entityService.ReportMemos);

            //CustomerListのイベントをOrderListが購読
            this.CustomerListViewModel.OnResceiveCustomerAdded += this.OrderListViewModel.AddResceiveCustomer;
            this.CustomerListViewModel.OnSendCustomerAdded += this.OrderListViewModel.AddSendCustomer;

            //OrderListのイベントをCustomerListが購読
            this.OrderListViewModel.OnEditCustomerButtonClick += this.CustomerListViewModel.SetSelectedCustomer;
            this.OrderListViewModel.OnEditCustomerButtonClick += this.ChangeToCustomerList;
            this.orderListViewModel.OnAddNewCustomerButtonClick += this.CustomerListViewModel.AddNewCustomer;
            this.OrderListViewModel.OnAddNewCustomerButtonClick += this.ChangeToCustomerList;
            this.OrderListViewModel.OnSendCustomerChanged += this.CustomerListViewModel.ChangeCanAddSendCustomer;

            //初期描画時に既にOrderがある場合は、送付者を追加できない。
            this.CustomerListViewModel.ChangeCanAddSendCustomer(this.OrderListViewModel,
                new CustomerListEventArgs() { CustomerViewModel = this.OrderListViewModel.SendCustomerViewModel });
        }

        public ProductListViewModel ProductListViewModel
        {
            get { return this.productListViewModel; }
        }

        public CustomerListViewModel CustomerListViewModel
        {
            get { return this.customerListViewModel; }
        }

        public OrderListViewModel OrderListViewModel
        {
            get { return this.orderListViewModel; }
        }

        public ViewModelCommand AboutCommand
        {
            get
            {
                return this.aboutCommand
                       ?? (this.aboutCommand = new ViewModelCommand(this.ShowAboutMessage));
            }
        }

        public ViewModelCommand ExitCommand
        {
            get
            {
                return this.exitCommand
                       ?? (this.exitCommand = new ViewModelCommand(this.Shutdown));
            }
        }

        private void Shutdown()
        {
            Messenger.Raise(new InteractionMessage("CloseWindow"));
        }

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

        private void ShowAboutMessage()
        {
            Messenger.Raise(new InformationMessage("HighLandirect version 0.1", "お知らせ", "Info"));
        }

        public void ShellViewModelClosing()
        {
            this.Save();
            this.entityService.Customerentities.Dispose();
        }

        #region ConfirmCommand
        /*
	    private ListenerCommand<ConfirmationMessage> _ConfirmCommand;

	    public ListenerCommand<ConfirmationMessage> ConfirmCommand
        {
		    get
            {
			    if (_ConfirmCommand == null) {
				    _ConfirmCommand = new ListenerCommand<ConfirmationMessage>(Confirm);
                }
			    return _ConfirmCommand;
            }
        }

        private void Confirm(ConfirmationMessage parameter)
        {
            if (parameter.Response.HasValue && parameter.Response.Value)
            {
                MessageBox.Show("「はい」が選択されました。");
            }
        }
         */
        #endregion

        #region Get/Set BackUpDataCommand
        private ListenerCommand<SavingFileSelectionMessage> _GetBackUpDataCommand;

        public ListenerCommand<SavingFileSelectionMessage> GetBackUpDataCommand
        {
            get
            {
                if (_GetBackUpDataCommand == null)
                {
                    _GetBackUpDataCommand = new ListenerCommand<SavingFileSelectionMessage>(GetBackUpData);
                }
                return _GetBackUpDataCommand;
            }
        }

        private void GetBackUpData(SavingFileSelectionMessage parameter)
        {
            if (parameter.Response == null || parameter.Response.Length == 0)
                return;

            File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\Domains\Resources\MyData.sdf", parameter.Response[0], true);
            Messenger.Raise(new InformationMessage("保存しました。", "お知らせ", "Info"));
        }

        private ListenerCommand<OpeningFileSelectionMessage> _SetBackUpDataCommand;

        public ListenerCommand<OpeningFileSelectionMessage> SetBackUpDataCommand
        {
            get
            {
                if (_SetBackUpDataCommand == null)
                {
                    _SetBackUpDataCommand = new ListenerCommand<OpeningFileSelectionMessage>(SetBackUpData);
                }
                return _SetBackUpDataCommand;
            }
        }

        private void SetBackUpData(OpeningFileSelectionMessage parameter)
        {
            if (parameter.Response == null || parameter.Response.Length == 0)
                return;

            File.Copy(parameter.Response[0], AppDomain.CurrentDomain.BaseDirectory + @"\Resources\MyData.sdf", true);
            Messenger.Raise(new InformationMessage("復元しました。再起動すると復元データを利用できます。", "お知らせ", "Info"));
        }

        #endregion

        #region Set/Get Ex/Im portData

        private ListenerCommand<SavingFileSelectionMessage> _GetExportDataCommand;
        public ListenerCommand<SavingFileSelectionMessage> GetExportDataCommand
        {
            get
            {
                if (_GetExportDataCommand == null)
                {
                    _GetExportDataCommand = new ListenerCommand<SavingFileSelectionMessage>(GetExportData);
                }
                return _GetExportDataCommand;
            }
        }
        private void GetExportData(SavingFileSelectionMessage parameter)
        {
            if (parameter.Response == null || parameter.Response.Length == 0)
                return;

            this.Save();

            using (var sw = new StreamWriter(parameter.Response[0], false, Encoding.UTF8))
            {
                sw.WriteLine(Customer.HeaderCsv);
                foreach (var customer in this.entityService.Customers)
                    sw.WriteLine(Customer.GetCsvFromCustomer(customer));
            }
            Messenger.Raise(new InformationMessage("エクスポート完了。", "お知らせ", "Info"));
        }

        private ListenerCommand<OpeningFileSelectionMessage> _SetImportDataCommand;
        public ListenerCommand<OpeningFileSelectionMessage> SetImportDataCommand
        {
            get
            {
                if (_SetImportDataCommand == null)
                {
                    _SetImportDataCommand = new ListenerCommand<OpeningFileSelectionMessage>(SetImportData);
                }
                return _SetImportDataCommand;
            }
        }

        private void SetImportData(OpeningFileSelectionMessage parameter)
        {
            if (parameter.Response == null || parameter.Response.Length == 0)
                return;

            bool ExistsError = this.CustomerListViewModel.SetImportData(parameter.Response[0]);

            if (ExistsError)
            {
                Messenger.Raise(new InformationMessage("インポートが完了しましたが、エラーが発生したためインポートできなかった行があります。\nOKをクリックすると詳細情報を表示します。",
                    "お知らせ", "Info"));
                System.Diagnostics.Process.Start("notepad.exe", "ErrorList.txt");
            }
            else
            {
                Messenger.Raise(new InformationMessage("インポート完了。", "お知らせ", "Info"));
            }
            //customerListViewModel.Initialize(orderController);
        }

        #endregion

        #region save

        public bool CanSave() { return this.IsValid; }

        public bool Save()
        {
            if (!CanSave())
            {
                throw new InvalidOperationException("You must not call Save when CanSave returns false.");
            }
            try
            {
                this.SaveCore();
                return true;
            }
            catch (ValidationException e)
            {
                Messenger.Raise(new InformationMessage(e.Message, "お知らせ", "Info"));
                Debug.Assert(false, e.Message);
                return false;
            }
            catch (UpdateException e)
            {
                Messenger.Raise(new InformationMessage(e.Message, "お知らせ", "Info"));
                Debug.Assert(false, e.Message);
                return false;
            }
        }

        private void SaveCore()
        {
            this.entityService.Customerentities.SaveChanges();
        }
        #endregion

        private int selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get { return this.selectedTabIndex; }
            set
            {
                if (this.selectedTabIndex != value)
                {
                    this.selectedTabIndex = value;
                    this.RaisePropertyChanged(() => this.SelectedTabIndex);
                }
            }
        }

        private void ChangeToCustomerList(object sender, CustomerListEventArgs e)
        {
            this.SelectedTabIndex = 1;
        }
    }
}
