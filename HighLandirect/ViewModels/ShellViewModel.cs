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
        private ViewModelCommand saveCommand;
        private ViewModelCommand exitCommand;
        private ViewModelCommand aboutCommand;

        //private readonly EntityController entityController;
        //private readonly MyDataEntities customerEntities;
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

            this.CustomerListViewModel.OnResceiveCustomerAdded += this.OrderListViewModel.AddResceiveCustomer;
            this.CustomerListViewModel.OnSendCustomerAdded += this.OrderListViewModel.AddSendCustomer;
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
        public ViewModelCommand SaveCommand
        {
            get
            {
                return this.saveCommand
                       ?? (this.saveCommand = new ViewModelCommand(this.SaveCore));
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
            this.entityService.Customerentities.Dispose();
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

        protected virtual void OnClosing(CancelEventArgs e)
        {
            //if (Closing != null) { Closing(this, e); }
        }

        private void ViewClosing(object sender, CancelEventArgs e)
        {
            OnClosing(e);
        }

        private void ShowAboutMessage()
        {
            Messenger.Raise(new InformationMessage("HighLandirect version 0.1", "お知らせ", "Info"));
        }

        //public void ShellViewModelClosing(object sender, CancelEventArgs e)
        public void ShellViewModelClosing()
        {
            this.Save();
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

            File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\Resources\MyData.sdf", parameter.Response[0], true);
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

        public ListenerCommand<OpeningFileSelectionMessage> GetExportDataCommand
        {
            get
            {
                if (_SetBackUpDataCommand == null)
                {
                    _SetBackUpDataCommand = new ListenerCommand<OpeningFileSelectionMessage>(GetExportData);
                }
                return _SetBackUpDataCommand;
            }
        }
        private void GetExportData(OpeningFileSelectionMessage parameter)
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
                return _SetBackUpDataCommand;
            }
        }

        private void SetImportData(OpeningFileSelectionMessage parameter)
        {
            if (parameter.Response == null || parameter.Response.Length == 0)
                return;

            bool ExistsError = false;
            using (var sr = new StreamReader(parameter.Response[0], Encoding.UTF8))
            using (var sw = new StreamWriter("ErrorList.txt", false, Encoding.UTF8))
            {
                string Line = sr.ReadLine();
                //ヘッダが設定されてたら読み捨てる
                if (Line.Trim() == Customer.HeaderCsv)
                    Line = sr.ReadLine();
                int i = 0;
                int CustNo = GetMaxCustNo();

                while (Line != null)
                {
                    i++;
                    if (Line.Trim().Length > 0)
                    {
                        try
                        {
                            CustNo++;
                            var customer = Customer.GetCustomerFromCsv(Line, CustNo);
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

        private int GetMaxCustNo()
        {
            int CustNo = 0;
            if (this.entityService.Customers.Count > 0)
            {
                //もっとカッコいいやり方はないんだろうか。。。
                var result = this.entityService.Customerentities.Customers.OrderBy("it.CustNo desc");
                foreach (var c in result)
                {
                    CustNo = c.CustNo;
                    break;
                }
            }
            return CustNo;
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

        public bool HasChanges
        {
            get { return (this.entityService.Customerentities != null 
                            && this.entityService.Customerentities.HasChanges); }
        }

    }
}
