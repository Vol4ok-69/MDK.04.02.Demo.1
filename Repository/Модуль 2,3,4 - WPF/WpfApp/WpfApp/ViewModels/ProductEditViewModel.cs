using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp.Helpers;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels;

public class ProductEditViewModel : ViewModelBase
{
    private readonly NavigationService _navigationService;
    private readonly int? _productId;

    private ImageSource? _imageSource;

    private string _sku = string.Empty;
    private string _name = string.Empty;
    private string _description = string.Empty;

    private string _priceText = string.Empty;
    private string _stockQuantityText = string.Empty;
    private string _currentDiscountText = string.Empty;

    private Category? _selectedCategory;
    private Manufacturer? _selectedManufacturer;
    private Supplier? _selectedSupplier;
    private Unit? _selectedUnit;

    private string? _photo;
    private string _imagePath = string.Empty;

    private readonly ObservableCollection<string> _errorMessages = [];

    public bool IsEditMode =>
        _productId.HasValue;

    public string PageTitle =>
        IsEditMode
            ? "Редактирование товара"
            : "Добавление товара";

    public int ProductId { get; private set; }

    public string SKU
    {
        get => _sku;
        set
        {
            if (_sku == value)
                return;

            _sku = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;

            _name = value;
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (_description == value)
                return;

            _description = value;
            OnPropertyChanged();
        }
    }

    public string PriceText
    {
        get => _priceText;
        set
        {
            if (_priceText == value)
                return;

            _priceText = value;
            OnPropertyChanged();
        }
    }

    public string StockQuantityText
    {
        get => _stockQuantityText;
        set
        {
            if (_stockQuantityText == value)
                return;

            _stockQuantityText = value;
            OnPropertyChanged();
        }
    }

    public string CurrentDiscountText
    {
        get => _currentDiscountText;
        set
        {
            if (_currentDiscountText == value)
                return;

            _currentDiscountText = value;
            OnPropertyChanged();
        }
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory == value)
                return;

            _selectedCategory = value;
            OnPropertyChanged();
        }
    }

    public Manufacturer? SelectedManufacturer
    {
        get => _selectedManufacturer;
        set
        {
            if (_selectedManufacturer == value)
                return;

            _selectedManufacturer = value;
            OnPropertyChanged();
        }
    }

    public Supplier? SelectedSupplier
    {
        get => _selectedSupplier;
        set
        {
            if (_selectedSupplier == value)
                return;

            _selectedSupplier = value;
            OnPropertyChanged();
        }
    }

    public Unit? SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            if (_selectedUnit == value)
                return;

            _selectedUnit = value;
            OnPropertyChanged();
        }
    }

    public string ImagePath
    {
        get => _imagePath;
        private set
        {
            if (_imagePath == value)
                return;

            _imagePath = value;
            OnPropertyChanged();
        }
    }

    public ImageSource? ImageSource
    {
        get => _imageSource;
        private set
        {
            if (_imageSource == value)
                return;

            _imageSource = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> ErrorMessages =>
        _errorMessages;

    public ObservableCollection<Category> Categories { get; } = [];

    public ObservableCollection<Manufacturer> Manufacturers { get; } = [];

    public ObservableCollection<Supplier> Suppliers { get; } = [];

    public ObservableCollection<Unit> Units { get; } = [];

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand SelectImageCommand { get; }

    public ProductEditViewModel(
        NavigationService navigationService,
        int? productId = null)
    {
        _navigationService = navigationService;
        _productId = productId;

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
        SelectImageCommand =
            new RelayCommand(SelectImage);

        LoadReferenceData();

        if (IsEditMode)
        {
            LoadProduct();
        }
        else
        {
            try
            {
                ProductId =
                    GetNextProductId();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                ErrorMessages.Add(
                    "Не удалось определить идентификатор нового товара.");
            }
        }
    }

    private void LoadReferenceData()
    {
        try
        {
            using var db =
                App.CreateDbContext();

            Categories.Clear();
            Manufacturers.Clear();
            Suppliers.Clear();
            Units.Clear();

            foreach (var category in
                     db.Categories
                         .AsNoTracking()
                         .OrderBy(x => x.Name)
                         .ToList())
            {
                Categories.Add(category);
            }

            foreach (var manufacturer in
                     db.Manufacturers
                         .AsNoTracking()
                         .OrderBy(x => x.Name)
                         .ToList())
            {
                Manufacturers.Add(manufacturer);
            }

            foreach (var supplier in
                     db.Suppliers
                         .AsNoTracking()
                         .OrderBy(x => x.Name)
                         .ToList())
            {
                Suppliers.Add(supplier);
            }

            foreach (var unit in
                     db.Units
                         .AsNoTracking()
                         .OrderBy(x => x.Name)
                         .ToList())
            {
                Units.Add(unit);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            ErrorMessages.Add(
                "Не удалось загрузить справочные данные товара.");
        }
    }

    private void LoadProduct()
    {
        try
        {
            using var db =
                App.CreateDbContext();

            var product =
                db.Products
                    .AsNoTracking()
                    .Include(x => x.ProductCategory)
                    .Include(x => x.Manufacturer)
                    .Include(x => x.Supplier)
                    .Include(x => x.UnitOfMeasurement)
                    .FirstOrDefault(
                        x => x.Id == _productId);

            if (product == null)
            {
                ErrorMessages.Add(
                    "Товар не найден в базе данных.");

                return;
            }

            ProductId = product.Id;
            SKU = product.SKU;
            Name = product.Name;
            Description = product.Description;

            PriceText =
                product.Price.ToString(
                    "0.##",
                    CultureInfo.CurrentCulture);

            StockQuantityText =
                product.StockQuantity.ToString(
                    CultureInfo.InvariantCulture);

            CurrentDiscountText =
                product.CurrentDiscount.ToString(
                    "0.##",
                    CultureInfo.CurrentCulture);

            SelectedCategory =
                Categories.FirstOrDefault(
                    x => x.Id == product.ProductCategoryId);

            SelectedManufacturer =
                Manufacturers.FirstOrDefault(
                    x => x.Id == product.ManufacturerId);

            SelectedSupplier =
                Suppliers.FirstOrDefault(
                    x => x.Id == product.SupplierId);

            SelectedUnit =
                Units.FirstOrDefault(
                    x => x.Id == product.UnitOfMeasurementId);

            _photo = product.Photo;

            ImagePath =
                ImageHelper.GetImagePath(
                    product.Photo) ?? string.Empty;

            ImageSource =
                ImageHelper.GetImage(
                    product.Photo);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            ErrorMessages.Add(
                "Не удалось загрузить данные товара.");
        }
    }

    private int GetNextProductId()
    {
        using var db =
            App.CreateDbContext();

        return
            (db.Products
                .Select(x => (int?)x.Id)
                .Max() ?? 0)
            + 1;
    }

    private void SelectImage()
    {
        var dialog = new OpenFileDialog
        {
            Title =
                "Выберите изображение товара",

            Filter =
                "Изображения|*.jpg;*.jpeg;*.png;*.bmp"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.CacheOption =
                BitmapCacheOption.OnLoad;

            bitmap.UriSource =
                new Uri(
                    dialog.FileName,
                    UriKind.Absolute);

            bitmap.EndInit();
            bitmap.Freeze();

            ImagePath = dialog.FileName;
            ImageSource = bitmap;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            ErrorMessages.Clear();

            ErrorMessages.Add(
                "Не удалось загрузить выбранное изображение.");
        }
    }

    private void Save()
    {
        ErrorMessages.Clear();

        if (!Validate(
                out decimal price,
                out int stockQuantity,
                out decimal discount))
        {
            return;
        }

        if (!IsEditMode &&
            ProductId <= 0)
        {
            ErrorMessages.Add(
                "Не удалось определить идентификатор товара.");

            return;
        }

        string? newPhoto = null;
        string? oldPhoto = null;

        try
        {
            using var db =
                App.CreateDbContext();

            string sku = SKU.Trim();

            bool skuExists =
                db.Products.Any(
                    x =>
                        x.SKU == sku &&
                        x.Id != ProductId);

            if (skuExists)
            {
                ErrorMessages.Add(
                    "Товар с таким артикулом уже существует.");

                return;
            }

            if (IsEditMode)
            {
                newPhoto =
                    UpdateProduct(
                        db,
                        price,
                        stockQuantity,
                        discount,
                        out oldPhoto);
            }
            else
            {
                newPhoto =
                    AddProduct(
                        db,
                        price,
                        stockQuantity,
                        discount);
            }

            db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            Debug.WriteLine(ex);

            if (!string.IsNullOrWhiteSpace(newPhoto))
                ImageHelper.DeleteManagedImage(
                    newPhoto);

            ErrorMessages.Add(
                "Не удалось сохранить товар. Проверьте введённые данные.");
            return;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            if (!string.IsNullOrWhiteSpace(newPhoto))
                ImageHelper.DeleteManagedImage(
                    newPhoto);

            ErrorMessages.Add(
                "Произошла ошибка при сохранении товара.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(newPhoto) &&
            !string.IsNullOrWhiteSpace(oldPhoto) &&
            !string.Equals(
                newPhoto,
                oldPhoto,
                StringComparison.OrdinalIgnoreCase))
        {
            ImageHelper.DeleteManagedImage(
                oldPhoto);
        }

        MessageBox.Show(
            IsEditMode
                ? "Товар успешно изменён."
                : "Товар успешно добавлен.",
            "Информация",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        _navigationService.NavigateToProducts();
    }

    private string? AddProduct(
        DataBaseContext db,
        decimal price,
        int stockQuantity,
        decimal discount)
    {
        var product = new Product
        {
            Id = ProductId,
            SKU = SKU.Trim(),
            Name = Name.Trim(),
            Description = Description.Trim(),
            Price = price,
            StockQuantity = stockQuantity,
            CurrentDiscount = discount,
            ProductCategoryId =
                SelectedCategory!.Id,
            ManufacturerId =
                SelectedManufacturer!.Id,
            SupplierId =
                SelectedSupplier!.Id,
            UnitOfMeasurementId =
                SelectedUnit!.Id
        };

        string? newPhoto = null;

        if (!string.IsNullOrWhiteSpace(ImagePath) &&
            File.Exists(ImagePath))
        {
            newPhoto =
                SaveImage(ImagePath);

            product.Photo = newPhoto;
        }

        db.Products.Add(product);

        return newPhoto;
    }

    private string? UpdateProduct(
        DataBaseContext db,
        decimal price,
        int stockQuantity,
        decimal discount,
        out string? oldPhoto)
    {
        var product =
            db.Products.FirstOrDefault(
                x => x.Id == ProductId);

        if (product == null)
            throw new InvalidOperationException(
                "Товар не найден.");

        oldPhoto = product.Photo;

        product.SKU = SKU.Trim();
        product.Name = Name.Trim();
        product.Description = Description.Trim();
        product.Price = price;
        product.StockQuantity = stockQuantity;
        product.CurrentDiscount = discount;
        product.ProductCategoryId =
            SelectedCategory!.Id;
        product.ManufacturerId =
            SelectedManufacturer!.Id;
        product.SupplierId =
            SelectedSupplier!.Id;
        product.UnitOfMeasurementId =
            SelectedUnit!.Id;

        string? oldAbsolutePath =
            ImageHelper.GetImagePath(oldPhoto);

        bool imageChanged =
            !string.IsNullOrWhiteSpace(ImagePath) &&
            File.Exists(ImagePath) &&
            !string.Equals(
                Path.GetFullPath(ImagePath),
                oldAbsolutePath,
                StringComparison.OrdinalIgnoreCase);

        if (!imageChanged)
            return null;

        string newPhoto =
            SaveImage(ImagePath);

        product.Photo = newPhoto;

        return newPhoto;
    }

    private string SaveImage(
        string sourcePath)
    {
        const long maxFileSize =
            10 * 1024 * 1024;

        var fileInfo =
            new FileInfo(sourcePath);

        if (fileInfo.Length > maxFileSize)
        {
            throw new InvalidOperationException(
                "Размер изображения не должен превышать 10 МБ.");
        }

        string imagesDirectory =
            Path.Combine(
                AppContext.BaseDirectory,
                "Images");

        Directory.CreateDirectory(
            imagesDirectory);

        string fileName =
            $"{Guid.NewGuid():N}.jpg";

        string destinationPath =
            Path.Combine(
                imagesDirectory,
                fileName);

        try
        {
            var bitmap = new BitmapImage();

            using (var stream =
                   File.OpenRead(sourcePath))
            {
                bitmap.BeginInit();

                bitmap.CacheOption =
                    BitmapCacheOption.OnLoad;

                bitmap.StreamSource = stream;

                bitmap.EndInit();
                bitmap.Freeze();
            }

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            if (width <= 0 || height <= 0)
                throw new InvalidOperationException(
                    "Некорректные размеры изображения.");

            double scale =
                Math.Min(
                    300.0 / width,
                    200.0 / height);

            int targetWidth =
                Math.Max(
                    1,
                    (int)(width * scale));

            int targetHeight =
                Math.Max(
                    1,
                    (int)(height * scale));

            var resized =
                new TransformedBitmap(
                    bitmap,
                    new ScaleTransform(
                        (double)targetWidth / width,
                        (double)targetHeight / height));

            resized.Freeze();

            var encoder =
                new JpegBitmapEncoder();

            encoder.Frames.Add(
                BitmapFrame.Create(resized));

            using var output =
                File.Create(destinationPath);

            encoder.Save(output);

            return Path.Combine(
                "Images",
                fileName);
        }
        catch
        {
            try
            {
                if (File.Exists(destinationPath))
                    File.Delete(destinationPath);
            }
            catch
            {
            }

            throw;
        }
    }

    private bool Validate(
        out decimal price,
        out int stockQuantity,
        out decimal discount)
    {
        price = 0;
        stockQuantity = 0;
        discount = 0;

        if (string.IsNullOrWhiteSpace(SKU))
            ErrorMessages.Add(
                "Введите артикул товара.");

        if (string.IsNullOrWhiteSpace(Name))
            ErrorMessages.Add(
                "Введите наименование товара.");

        if (string.IsNullOrWhiteSpace(Description))
            ErrorMessages.Add(
                "Введите описание товара.");

        if (!TryParseDecimal(
                PriceText,
                out price))
        {
            ErrorMessages.Add(
                "Цена должна быть числом.");
        }
        else if (price < 0)
        {
            ErrorMessages.Add(
                "Цена не может быть отрицательной.");
        }

        if (!int.TryParse(
                StockQuantityText,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out stockQuantity))
        {
            ErrorMessages.Add(
                "Количество товара должно быть целым числом.");
        }
        else if (stockQuantity < 0)
        {
            ErrorMessages.Add(
                "Количество товара не может быть отрицательным.");
        }

        if (!TryParseDecimal(
                CurrentDiscountText,
                out discount))
        {
            ErrorMessages.Add(
                "Скидка должна быть числом.");
        }
        else if (discount < 0 || discount > 100)
        {
            ErrorMessages.Add(
                "Скидка должна быть в пределах от 0 до 100.");
        }

        if (SelectedCategory == null)
            ErrorMessages.Add(
                "Выберите категорию товара.");

        if (SelectedManufacturer == null)
            ErrorMessages.Add(
                "Выберите производителя.");

        if (SelectedSupplier == null)
            ErrorMessages.Add(
                "Выберите поставщика.");

        if (SelectedUnit == null)
            ErrorMessages.Add(
                "Выберите единицу измерения.");

        return ErrorMessages.Count == 0;
    }

    private static bool TryParseDecimal(
        string text,
        out decimal value)
    {
        string normalized =
            text.Trim().Replace(',', '.');

        return decimal.TryParse(
            normalized,
            NumberStyles.AllowLeadingSign |
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);
    }

    private void Cancel()
    {
        _navigationService.GoBack();
    }
}