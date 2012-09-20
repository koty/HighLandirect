using HighLandirect.Domains;
using Livet;

namespace HighLandirect.ViewModels
{
    public class ProductViewModel : ViewModel
    {
        private bool isValid = true;
        private Product product;
        private readonly ProductListViewModel productListViewModel;

        public ProductViewModel(ProductListViewModel productListViewModel, Product product)
        {
            this.productListViewModel = productListViewModel;
            this.Product = product;
        }

        public bool IsEnabled
        {
            get { return Product != null; }
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

        public Product Product
        {
            get { return product; }
            set
            {
                if (product != value)
                {
                    product = value;
                    RaisePropertyChanged("Product");
                    RaisePropertyChanged("IsEnabled");
                }
            }
        }
    }
}
