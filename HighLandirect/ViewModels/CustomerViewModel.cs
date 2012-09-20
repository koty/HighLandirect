using HighLandirect.Domains;
using Livet;
using Livet.Commands;

namespace HighLandirect.ViewModels
{
    public class CustomerViewModel : ViewModel
    {
        private bool isValid = true;
        private Customer customer;
        private readonly CustomerListViewModel customerListViewModel;

        public CustomerViewModel()
        {
        }

        public CustomerViewModel(CustomerListViewModel customerListViewModel, Customer customer)
        {
            this.customerListViewModel = customerListViewModel;
            this.Customer = customer;
        }

        public bool IsEnabled
        {
            get { return Customer != null; }
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

        public Customer Customer
        {
            get { return customer; }
            set
            {
                if (customer != value)
                {
                    customer = value;
                    RaisePropertyChanged("Customer");
                    RaisePropertyChanged("IsEnabled");
                }
            }
        }

        private ViewModelCommand textBox_KeyUpCommand;

        public ViewModelCommand TextBox_KeyUpCommand
        {
            get { return this.textBox_KeyUpCommand ?? (this.textBox_KeyUpCommand = new ViewModelCommand(TextBox_KeyUp)); }
        }

        private void TextBox_KeyUp()
        {
            //this.Customer.PostalCD.Select(this.PostalCD.Text.Length, 0);
            if (this.Customer.PostalCD.Length < 8) return;

            this.SetJusho(this.Customer.PostalCD);

            //続けて番地を入力できるよう、フォーカスとカーソル位置を操作
            //this.Customer.Address1.Focus();
            //this.Customer.Select(this.Customer.Address1.Length, 0);
        }

        private void SetJusho(string PostalCD)
        {
            var postalConverter = new PostalCDGetter();
            postalConverter.GetAddress(PostalCD);
            customer.SetAddress(postalConverter);
        }
    }
}
