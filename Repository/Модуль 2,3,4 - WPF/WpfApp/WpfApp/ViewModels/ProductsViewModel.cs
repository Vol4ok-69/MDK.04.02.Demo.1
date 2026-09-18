using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WpfApp.Helpers;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels;

public class ProductsViewModel : ViewModelBase
{
    private readonly NavigationService _navigationService;
    private readonly SessionService _sessionService;

    private string _searchText = string.Empty;
    private ManufacturerFilterViewModel? _selectedManufacturer;
    private string _selectedSort =
        "Без сортировки";
    private bool _isSortDescending;

    public ObservableCollection<ProductItemViewModel> Products { get; } = [];

    public ICollectionView ProductsView { get; }

    public ObservableCollection<ManufacturerFilterViewModel> Manufacturers { get; } = [];

    public ObservableCollection<string> SortOptions { get; } =
    [
        "Без сортировки",
        "Количество на складе",
        "Цена",
        "Скидка"
    ];

    public string FullName =>
        _sessionService.FullName;

    public string RoleName =>
        _sessionService.RoleName;

    public bool IsManager =>
        _sessionService.IsManager;

    public bool IsAdmin =>
        _sessionService.IsAdmin;

    public bool IsManagerOrAdmin =>
        IsManager || IsAdmin;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;

            _searchText = value;

            OnPropertyChanged();

            ProductsView.Refresh();
        }
    }

    public ManufacturerFilterViewModel? SelectedManufacturer
    {
        get => _selectedManufacturer;
        set
        {
            if (_selectedManufacturer == value)
                return;

            _selectedManufacturer = value;

            OnPropertyChanged();

            ProductsView.Refresh();
        }
    }

    public string SelectedSort
    {
        get => _selectedSort;
        set
        {
            if (_selectedSort == value)
                return;

            _selectedSort = value;

            OnPropertyChanged();

            ApplySorting();
        }
    }

    public bool IsSortDescending
    {
        get => _isSortDescending;
        set
        {
            if (_isSortDescending == value)
                return;

            _isSortDescending = value;

            OnPropertyChanged();

            ApplySorting();
        }
    }

    public string SortDirectionText =>
        IsSortDescending
            ? "По убыванию"
            : "По возрастанию";

    public ICommand LogoutCommand { get; }

    public ICommand ToggleSortDirectionCommand { get; }

    public ICommand ResetFiltersCommand { get; }

    public ICommand AddProductCommand { get; }

    public ICommand OrdersCommand { get; }

    public ICommand EditProductCommand { get; }

    public ICommand DeleteProductCommand { get; }

    public ProductsViewModel(
        NavigationService navigationService,
        SessionService sessionService)
    {
        _navigationService = navigationService;
        _sessionService = sessionService;

        ProductsView =
            CollectionViewSource.GetDefaultView(
                Products);

        ProductsView.Filter =
            FilterProduct;

        LogoutCommand =
            new RelayCommand(Logout);

        ToggleSortDirectionCommand =
            new RelayCommand(
                ToggleSortDirection);

        ResetFiltersCommand =
            new RelayCommand(
                ResetFilters);

        AddProductCommand =
            new RelayCommand(
                AddProduct);

        EditProductCommand =
            new RelayCommand<ProductItemViewModel>(
                EditProduct);

        DeleteProductCommand =
            new RelayCommand<ProductItemViewModel>(
                DeleteProduct);

        OrdersCommand =
            new RelayCommand(
                OpenOrders);

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadProductsAsync();
        await LoadManufacturersAsync();

        ApplySorting();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            using var db =
                App.CreateDbContext();

            var products =
                await db.Products
                    .AsNoTracking()
                    .Include(x => x.ProductCategory)
                    .Include(x => x.Manufacturer)
                    .Include(x => x.Supplier)
                    .Include(x => x.UnitOfMeasurement)
                    .ToListAsync();

            Products.Clear();

            foreach (var product in products)
            {
                Products.Add(
                    new ProductItemViewModel(
                        product));
            }

            ProductsView.Refresh();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            Products.Clear();

            MessageBox.Show(
                "Не удалось загрузить товары. Проверьте подключение к базе данных.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task LoadManufacturersAsync()
    {
        try
        {
            using var db =
                App.CreateDbContext();

            var manufacturers =
                await db.Manufacturers
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .ToListAsync();

            Manufacturers.Clear();

            Manufacturers.Add(
                new ManufacturerFilterViewModel(
                    null));

            foreach (var manufacturer
                     in manufacturers)
            {
                Manufacturers.Add(
                    new ManufacturerFilterViewModel(
                        manufacturer));
            }

            SelectedManufacturer =
                Manufacturers.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            Manufacturers.Clear();

            Manufacturers.Add(
                new ManufacturerFilterViewModel(
                    null));

            SelectedManufacturer =
                Manufacturers.FirstOrDefault();

            MessageBox.Show(
                "Не удалось загрузить список производителей.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddProduct()
    {
        if (!IsAdmin)
            return;

        _navigationService.NavigateToProductEdit();
    }

    private void EditProduct(
        ProductItemViewModel? product)
    {
        if (!IsAdmin || product == null)
            return;

        _navigationService.NavigateToProductEdit(
            product.Product.Id);
    }

    private void DeleteProduct(
        ProductItemViewModel? product)
    {
        if (!IsAdmin || product == null)
            return;

        var result =
            MessageBox.Show(
                $"Удалить товар «{product.Name}»?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            using var db =
                App.CreateDbContext();

            bool isUsedInOrder =
                db.OrderProducts
                    .Any(
                        x =>
                            x.ProductId ==
                            product.Product.Id);

            if (isUsedInOrder)
            {
                MessageBox.Show(
                    "Удаление невозможно. Товар присутствует в заказе.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            var productToDelete =
                db.Products.FirstOrDefault(
                    x =>
                        x.Id ==
                        product.Product.Id);

            if (productToDelete == null)
            {
                MessageBox.Show(
                    "Товар не найден в базе данных.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            string? photo =
                productToDelete.Photo;

            db.Products.Remove(
                productToDelete);

            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(photo))
            {
                ImageHelper.DeleteManagedImage(
                    photo);
            }

            _ = LoadProductsAsync();
        }
        catch (DbUpdateException ex)
        {
            Debug.WriteLine(ex);

            MessageBox.Show(
                "Не удалось удалить товар. Товар используется в связанных данных.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            MessageBox.Show(
                "Произошла ошибка при удалении товара.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private bool FilterProduct(
        object item)
    {
        if (item is not ProductItemViewModel product)
            return false;

        if (SelectedManufacturer?.Manufacturer != null &&
            product.Product.ManufacturerId !=
            SelectedManufacturer.Manufacturer.Id)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        string search =
            SearchText.Trim();

        return Contains(product.SKU, search)
               || Contains(product.Name, search)
               || Contains(product.CategoryName, search)
               || Contains(product.Description, search)
               || Contains(product.ManufacturerName, search)
               || Contains(product.SupplierName, search)
               || Contains(product.UnitName, search);
    }

    private static bool Contains(
        string? value,
        string search)
    {
        return !string.IsNullOrWhiteSpace(value)
               &&
               value.Contains(
                   search,
                   StringComparison.OrdinalIgnoreCase);
    }

    private void ApplySorting()
    {
        using (
            ProductsView.DeferRefresh())
        {
            ProductsView.SortDescriptions.Clear();

            if (SelectedSort ==
                "Количество на складе")
            {
                ProductsView.SortDescriptions.Add(
                    new SortDescription(
                        nameof(
                            ProductItemViewModel.StockQuantity),
                        GetSortDirection()));
            }
            else if (SelectedSort ==
                     "Цена")
            {
                ProductsView.SortDescriptions.Add(
                    new SortDescription(
                        nameof(
                            ProductItemViewModel.Price),
                        GetSortDirection()));
            }
            else if (SelectedSort ==
                     "Скидка")
            {
                ProductsView.SortDescriptions.Add(
                    new SortDescription(
                        nameof(
                            ProductItemViewModel.CurrentDiscount),
                        GetSortDirection()));
            }
        }

        OnPropertyChanged(
            nameof(SortDirectionText));
    }

    private ListSortDirection GetSortDirection() =>
        IsSortDescending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;

    private void ToggleSortDirection()
    {
        IsSortDescending =
            !IsSortDescending;
    }

    private void ResetFilters()
    {
        SearchText = string.Empty;

        SelectedManufacturer =
            Manufacturers.FirstOrDefault();

        SelectedSort =
            "Без сортировки";

        IsSortDescending = false;

        ProductsView.Refresh();
    }

    private void OpenOrders()
    {
        if (!IsManagerOrAdmin)
            return;

        _navigationService.NavigateToOrders();
    }

    private void Logout()
    {
        _sessionService.Clear();

        _navigationService.NavigateToAuthorization();
    }
}