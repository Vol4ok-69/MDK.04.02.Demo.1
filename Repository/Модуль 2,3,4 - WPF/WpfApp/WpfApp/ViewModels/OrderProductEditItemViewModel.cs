using System.Globalization;
using System.Windows.Input;
using WpfApp.Helpers;
using WpfApp.Models;

namespace WpfApp.ViewModels;

public class OrderProductEditItemViewModel : ViewModelBase
{
    private Product _product;
    private int _quantity;
    private string _quantityText;

    public Product Product
    {
        get => _product;
        set
        {
            if (_product == value)
                return;

            _product = value;

            OnPropertyChanged();
        }
    }

    public string QuantityText
    {
        get => _quantityText;
        set
        {
            if (_quantityText == value)
                return;

            _quantityText = value;

            OnPropertyChanged();
        }
    }

    public int Quantity =>
        _quantity;

    public ICommand IncreaseQuantityCommand { get; }

    public ICommand DecreaseQuantityCommand { get; }

    public ICommand RemoveCommand { get; }

    public OrderProductEditItemViewModel(
        Product product,
        int quantity,
        Action<OrderProductEditItemViewModel> remove)
    {
        _product = product;
        _quantity = Math.Max(1, quantity);

        _quantityText =
            _quantity.ToString(
                CultureInfo.InvariantCulture);

        IncreaseQuantityCommand =
            new RelayCommand(
                IncreaseQuantity);

        DecreaseQuantityCommand =
            new RelayCommand(
                DecreaseQuantity);

        RemoveCommand =
            new RelayCommand(
                () => remove(this));
    }

    public bool TryGetQuantity(
        out int quantity)
    {
        return int.TryParse(
                   QuantityText,
                   NumberStyles.Integer,
                   CultureInfo.InvariantCulture,
                   out quantity)
               && quantity > 0;
    }

    private void IncreaseQuantity()
    {
        if (!TryGetQuantity(
                out int quantity))
        {
            quantity = 0;
        }

        quantity++;

        _quantity = quantity;

        QuantityText =
            quantity.ToString(
                CultureInfo.InvariantCulture);

        OnPropertyChanged(
            nameof(Quantity));
    }

    private void DecreaseQuantity()
    {
        if (!TryGetQuantity(
                out int quantity))
        {
            quantity = 1;
        }

        if (quantity <= 1)
            return;

        quantity--;

        _quantity = quantity;

        QuantityText =
            quantity.ToString(
                CultureInfo.InvariantCulture);

        OnPropertyChanged(
            nameof(Quantity));
    }
}