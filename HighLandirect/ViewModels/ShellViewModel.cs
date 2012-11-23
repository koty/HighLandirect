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

namespace HighLandirect.ViewModels
{
    public class ShellViewModel : ViewModel
    {
        private ViewModelCommand saveCommand;
        private ViewModelCommand exitCommand;
        private ViewModelCommand aboutCommand;
        private ViewModelCommand getBackUpDataCommand;
        private ViewModelCommand setBackUpDataCommand;
        private ViewModelCommand getExportDataCommand;
        private ViewModelCommand setImportDataCommand;
        //private readonly EntityController entityController;
        private readonly MyDataEntities customerEntities;

        private readonly ProductListViewModel productListViewModel;
        private readonly CustomerListViewModel customerListViewModel;
        private readonly OrderListViewModel orderListViewModel;

        private bool isValid = true;
        
        public static string Title { get { return "HighLandirect"; } }

        public ShellViewModel()
        {
            var entityService = new EntityService();
            customerEntities = new MyDataEntities();
            entityService.Customerentities = customerEntities;

            //this.entityController = new EntityController(entityService, this);
            this.productListViewModel = new ProductListViewModel(entityService.Products, entityService);
            this.customerListViewModel = new CustomerListViewModel(this, entityService.Customers, entityService);
            this.orderListViewModel = new OrderListViewModel(entityService, entityService.Orders,
                                                             entityService.Stores, 
                                                             entityService.OrderHistories,
                                                             entityService.ReportMemos);
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
                       ?? (this.saveCommand = new ViewModelCommand(this.SaveIfYouWant));
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

        public ViewModelCommand GetBackUpDataCommand
        {
            get
            {
                return this.getBackUpDataCommand
                       ?? (this.getBackUpDataCommand = new ViewModelCommand(this.GetBackUpData));
            }
        }

        public ViewModelCommand SetBackUpDataCommand
        {
            get
            {
                return this.setBackUpDataCommand
                       ?? (this.setBackUpDataCommand = new ViewModelCommand(this.SetBackUpData));
            }
        }

        public ViewModelCommand GetExportDataCommand
        {
            get
            {
                return this.getExportDataCommand
                       ?? (this.getExportDataCommand = new ViewModelCommand(this.GetExportData));
            }
        }

        public ViewModelCommand SetImportDataCommand
        {
            get
            {
                return this.setImportDataCommand
                       ?? (this.setImportDataCommand = new ViewModelCommand(this.SetImportData));
            }
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
            //messageService.ShowMessage(string.Format(CultureInfo.CurrentCulture, Resources.AboutText,
            //    ApplicationInfo.ProductName, ApplicationInfo.Version));
        }

        public void SetSendCustNo(int custNo)
        {
            throw new System.NotImplementedException();
        }

        internal void SetResceiveCustNo(int custNo)
        {
            throw new System.NotImplementedException();
        }

        //public void ShellViewModelClosing(object sender, CancelEventArgs e)
        public void ShellViewModelClosing()
        {
            if (this.HasChanges)
            {
                /*
                if (this.IsValid)
                {
                    bool? result = questionService.ShowQuestion("変更しますか？");
                    if (result == true)
                    {
                        if (!entityController.Save())
                        {
                            e.Cancel = true;
                        }
                    }
                    else if (result == null)
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    e.Cancel = !questionService.ShowYesNoQuestion("変更してないですけどOKですか？");
                }
                */
            }
        }


        private void GetBackUpData()
        {
            /*
            SaveIfYouWant();
            var result = this.fileDialogService.ShowSaveFileDialog(new FileType("SQL-Server Compact Database", ".sdf"));

            if (result.IsValid)
            {
                File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\Resources\MyData.sdf", result.FileName, true);
                this.messageService.ShowMessage("保存しました。");
            }
            else
            {
                this.messageService.ShowMessage("何かエラーです。保存に失敗しました。");
            }
            */ 
        }

        private void SaveIfYouWant()
        {
            /*
            if (entityController.HasChanges)
            {
                if (shellViewModel.IsValid)
                {
                    var SaveQuestionResult = questionService.ShowQuestion("変更があります。処理の前にその変更を保存しますか？");
                    if (SaveQuestionResult == true)
                    {
                        this.entityController.Save();
                    }
                }
            }
            */ 
        }

        private void SetBackUpData()
        {
            /*
            var result = this.fileDialogService.ShowOpenFileDialog(new FileType("SQL-Server Compact Database", ".sdf"));
            if (result.IsValid)
            {
                File.Copy(result.FileName, AppDomain.CurrentDomain.BaseDirectory + @"\Resources\MyData.sdf", true);
                this.messageService.ShowMessage("復元しました。再起動すると復元データを利用できます。");
            }
            else
            {
                this.messageService.ShowMessage("何かエラーです。保存に失敗しました。");
            }
            */ 
        }

        private void GetExportData()
        {
            /*
            SaveIfYouWant();
            var result = this.fileDialogService.ShowSaveFileDialog(new FileType("顧客情報", ".txt"));

            if (result.IsValid)
            {
                using (var sw = new StreamWriter(result.FileName, false, Encoding.UTF8))
                {
                    sw.WriteLine(Customer.HeaderCsv);
                    foreach (var customer in this.entityService.Customers)
                        sw.WriteLine(Customer.GetCsvFromCustomer(customer));
                }
                this.messageService.ShowMessage("エクスポート完了。");
            }
            else
            {
                //this.messageService.ShowMessage("何かエラーです。エクスポートに失敗しました。");
            }
            */ 
        }

        private void SetImportData()
        {
            /*
            var result = this.fileDialogService.ShowOpenFileDialog(new FileType("顧客情報", ".txt"));
            if (result.IsValid)
            {
                bool ExistsError = false;
                using (var sr = new StreamReader(result.FileName, Encoding.UTF8))
                using (var sw = new StreamWriter("ErrorList.txt", false, Encoding.UTF8))
                {
                    string Line = sr.ReadLine();
                    //ヘッダが設定されてたら読み捨てる
                    if (Line.Trim() == Customer.HeaderCsv)
                        Line = sr.ReadLine();
                    int i = 0;
                    while (Line != null)
                    {
                        i++;
                        if (Line.Trim().Length > 0)
                        {
                            try
                            {
                                int CustNo;
                                if (this.entityService.Customers.Count == 0)
                                {
                                    CustNo = 1;
                                }
                                else
                                {
                                    CustNo = this.entityService.Customers.OrderByDescending(x => x.CustNo).FirstOrDefault().CustNo;
                                }
                                var customer = Customer.GetCustomerFromCsv(Line, CustNo + 1);
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
                    this.messageService.ShowMessage("インポートが完了しましたが、エラーが発生したためインポートできなかった行があります。\nOKをクリックすると詳細情報を表示します。");
                    System.Diagnostics.Process.Start("notepad.exe", "ErrorList.txt");
                }
                else
                {
                    this.messageService.ShowMessage("インポート完了。");
                }
                customerListViewModel.Initialize(orderController);
            }
            else
            {
                //his.messageService.ShowMessage("何かエラーです。インポートに失敗しました。");
            }
            */ 
        }
        public void Shutdown()
        {
            this.customerEntities.Dispose();
        }

        public bool CanSave() { return this.IsValid; }

        public bool Save()
        {
            if (!CanSave())
            {
                throw new InvalidOperationException("You must not call Save when CanSave returns false.");
            }
            try
            {
                customerEntities.SaveChanges();
                return true;
            }
            catch (ValidationException e)
            {
                //messageService.ShowError(string.Format(CultureInfo.CurrentCulture, Resources.SaveErrorInvalidEntities, 
                //    e.Message));
                Debug.Assert(false, e.Message);
                return false;
            }
            catch (UpdateException e)
            {
                //messageService.ShowError(string.Format(CultureInfo.CurrentCulture, Resources.SaveErrorInvalidFields,
                //    e.InnerException.Message));
                Debug.Assert(false, e.Message);
                return false;
            }
        }

        public bool HasChanges
        {
            get { return (this.customerEntities != null && this.customerEntities.HasChanges); }
        }

    }
}
