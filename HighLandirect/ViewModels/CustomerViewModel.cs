using HighLandirect.Domains;
using Livet;
using Livet.Behaviors.Messaging;
using Livet.Commands;
using Livet.Messaging;

namespace HighLandirect.ViewModels
{
    public class CustomerViewModel : ViewModel
    {
        private bool isValid = true;
        private Customer customer;

        public CustomerViewModel()
        {
        }

        public CustomerViewModel(Customer customer)
        {
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

        public void PostalCDTextBox_KeyUp()
        {
            if (this.Customer.PostalCD.Length < 7) return;

            //郵便番号検索
            this.SetJusho(this.Customer.PostalCD);

            //続けて番地を入力できるよう、フォーカスとカーソル位置を操作
            Messenger.Raise(new InteractionMessage("SetFocus"));
            Messenger.Raise(new InteractionMessage("MoveCaretToEnd"));
        }

        private void SetJusho(string PostalCD)
        {
            var postalConverter = new PostalCDGetter();
            postalConverter.GetAddress(PostalCD);
            customer.SetAddress(postalConverter);
        }
    }
}
