using HighLandirect.Domains;
using Livet;
using Livet.Behaviors.Messaging;
using Livet.Commands;
using Livet.Messaging;
using System.Linq;
using System.Collections.Generic;

namespace HighLandirect.ViewModels
{
    public class CustomerViewModel : ViewModel
    {
        private bool isValid = true;
        private Customer customer;
        private IEnumerable<CustomerViewModel> customerViewModels;

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

        public IEnumerable<CustomerViewModel> CustomerViewModels
        {
            set
            {
                this.customerViewModels = value;
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

        public void CustNameTextBox_LostFocus()
        {
            if (this.Customer.Furigana.Length > 3)
            {
                if (this.customerViewModels
                        .Any(x => eq(x.Customer.Furigana, this.Customer.Furigana)))
                {
                    Messenger.Raise(new InteractionMessage("ConfirmDuplicateRegistration"));
                }
            }
        }

        private static bool eq(string furigana1, string furigana2)
        {
            return (furigana1.Replace(" ", "").Replace("　", "")
                 == furigana2.Replace(" ", "").Replace("　", ""));
        }
    }
}
