using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using HighLandirect.Domains;
using HighLandirect.Services;
using Livet;
using Livet.Commands;

namespace HighLandirect.ViewModels
{
    public class ProductListViewModel : ViewModel
    {
        private readonly IEntityService entityService;

        private ViewModelCommand addNewCommand;
        private ViewModelCommand removeCommand;

        
        public ProductListViewModel(IEnumerable<Product> products, IEntityService entityService)
        {
            if (products == null) { throw new ArgumentNullException("products"); }

            var productViewModelList = products.Select(p => new ProductViewModel(this, p));
            this.ProductViewModels = new ObservableCollection<ProductViewModel>(productViewModelList);
            this.entityService = entityService;
        }

        public ObservableCollection<ProductViewModel> ProductViewModels { get; private set; }

        private ProductViewModel selectedProduct;

        public ProductViewModel SelectedProduct
        {
            get { return this.selectedProduct; }
            set
            {
                if (this.selectedProduct == null 
                        || this.selectedProduct.Product.ProductID != value.Product.ProductID)
                {
                    this.selectedProduct = value;
                    this.RaisePropertyChanged(() => SelectedProduct);
                }
            }
        }

        public ViewModelCommand AddNewCommand
        {
            get
            {
                return addNewCommand
                       ?? (addNewCommand = new ViewModelCommand(AddNewProduct, CanAddNewProduct));
            }
        }

        public ViewModelCommand RemoveCommand
        {
            get
            {
                return removeCommand
                       ?? (removeCommand = new ViewModelCommand(RemoveProduct, CanRemoveProduct));
            }
        }

        private bool CanAddNewProduct() { return true; }

        private void AddNewProduct()
        {
            int ProductId;
            if (this.entityService.Products.Count == 0)
            {
                ProductId = 1;
            }
            else
            {
                ProductId = this.entityService.Products
                                            .Select(x => x.ProductID)
                                            .Max() + 1;
            }
            var product = Product.CreateProduct(ProductId, "", 0);
            entityService.Products.Add(product);
            var productViewModel = new ProductViewModel(this, product);

            this.SelectedProduct = productViewModel;
        }

        private bool CanRemoveProduct() { return this.SelectedProduct != null; }

        private void RemoveProduct()
        {
            entityService.Products.Remove(this.SelectedProduct.Product);
        }


        private void UpdateCommands()
        {
            addNewCommand.RaiseCanExecuteChanged();
            removeCommand.RaiseCanExecuteChanged();
        }
        /*
        private void ProductListViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedProduct")
            {
                productViewModel.Product = productListViewModel.SelectedProduct;
                UpdateCommands();
            }
        }

        private void ProductViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValid")
            {
                UpdateCommands();
            }
        }
        */ 
    }
}
