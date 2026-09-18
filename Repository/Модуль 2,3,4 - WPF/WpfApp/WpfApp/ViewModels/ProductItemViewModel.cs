using System.Windows;
using System.Windows.Media;
using WpfApp.Helpers;
using WpfApp.Models;

namespace WpfApp.ViewModels;

public class ProductItemViewModel : ViewModelBase
{
    private readonly ImageSource _imageSource;

    public Product Product { get; }

    public string SKU =>
        Product.SKU;

    public string Name =>
        Product.Name;

    public string CategoryName =>
        Product.ProductCategory.Name;

    public string Description =>
        Product.Description;

    public string ManufacturerName =>
        Product.Manufacturer.Name;

    public string SupplierName =>
        Product.Supplier.Name;

    public decimal Price =>
        Product.Price;

    public decimal CurrentDiscount =>
        Product.CurrentDiscount;

    public int StockQuantity =>
        Product.StockQuantity;

    public string UnitName =>
        Product.UnitOfMeasurement.Name;

    public decimal FinalPrice =>
        Product.Price *
        (1 - Product.CurrentDiscount / 100);

    public bool HasDiscount =>
        Product.CurrentDiscount > 0;

    public ImageSource ImageSource =>
        _imageSource;

    public Brush RowBackground
    {
        get
        {
            if (Product.StockQuantity == 0)
            {
                return new SolidColorBrush(
                    Color.FromRgb(173, 216, 230));
            }

            if (Product.CurrentDiscount > 12)
            {
                return new SolidColorBrush(
                    Color.FromRgb(244, 164, 96));
            }

            return new SolidColorBrush(
                Colors.White);
        }
    }

    public Visibility RegularPriceVisibility =>
        HasDiscount
            ? Visibility.Collapsed
            : Visibility.Visible;

    public Visibility DiscountedPriceVisibility =>
        HasDiscount
            ? Visibility.Visible
            : Visibility.Collapsed;

    public ProductItemViewModel(Product product)
    {
        Product = product;

        _imageSource =
            ImageHelper.GetImage(Product.Photo);
    }
}