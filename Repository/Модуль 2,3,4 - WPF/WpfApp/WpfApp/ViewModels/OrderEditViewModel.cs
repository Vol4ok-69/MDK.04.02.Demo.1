using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;
using WpfApp.Helpers;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels;

public class OrderEditViewModel : ViewModelBase
{
    private readonly NavigationService _navigationService;
    private readonly int? _orderId;
    private readonly Action? _onSaved;

    private OrderStatus? _selectedStatus;
    private PickupPoint? _selectedPickupPoint;
    private User? _selectedUser;
    private Product? _selectedProduct;

    private string _newQuantityText = "1";

    private DateTime? _orderDate = DateTime.Today;
    private DateTime? _deliveryDate =
        DateTime.Today.AddDays(7);

    public ObservableCollection<string> ErrorMessages { get; } = [];

    public ObservableCollection<Product> Products { get; } = [];

    public ObservableCollection<OrderProductEditItemViewModel> OrderProducts { get; } = [];

    public ObservableCollection<OrderStatus> OrderStatuses { get; } = [];

    public ObservableCollection<PickupPoint> PickupPoints { get; } = [];

    public ObservableCollection<User> Users { get; } = [];

    public int? OrderId =>
        _orderId;

    public bool IsEditMode =>
        _orderId.HasValue;

    public string PageTitle =>
        IsEditMode
            ? "Редактирование заказа"
            : "Добавление заказа";

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (_selectedProduct == value)
                return;

            _selectedProduct = value;

            OnPropertyChanged();
        }
    }

    public string NewQuantityText
    {
        get => _newQuantityText;
        set
        {
            if (_newQuantityText == value)
                return;

            _newQuantityText = value;

            OnPropertyChanged();
        }
    }

    public OrderStatus? SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (_selectedStatus == value)
                return;

            _selectedStatus = value;

            OnPropertyChanged();
        }
    }

    public PickupPoint? SelectedPickupPoint
    {
        get => _selectedPickupPoint;
        set
        {
            if (_selectedPickupPoint == value)
                return;

            _selectedPickupPoint = value;

            OnPropertyChanged();
        }
    }

    public User? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (_selectedUser == value)
                return;

            _selectedUser = value;

            OnPropertyChanged();
        }
    }

    public DateTime? OrderDate
    {
        get => _orderDate;
        set
        {
            if (_orderDate == value)
                return;

            _orderDate = value;

            OnPropertyChanged();
        }
    }

    public DateTime? DeliveryDate
    {
        get => _deliveryDate;
        set
        {
            if (_deliveryDate == value)
                return;

            _deliveryDate = value;

            OnPropertyChanged();
        }
    }

    public ICommand AddProductCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand BackCommand { get; }

    public OrderEditViewModel(
        NavigationService navigationService,
        int? orderId,
        Action? onSaved)
    {
        _navigationService = navigationService;
        _orderId = orderId;
        _onSaved = onSaved;

        AddProductCommand =
            new RelayCommand(AddProduct);

        SaveCommand =
            new RelayCommand(Save);

        BackCommand =
            new RelayCommand(Back);

        LoadData();
    }

    private void LoadData()
    {
        try
        {
            using var db =
                App.CreateDbContext();

            Products.Clear();
            OrderStatuses.Clear();
            PickupPoints.Clear();
            Users.Clear();

            foreach (var product in
                     db.Products
                         .AsNoTracking()
                         .OrderBy(x => x.SKU)
                         .ToList())
            {
                Products.Add(product);
            }

            foreach (var status in
                     db.OrderStatuses
                         .AsNoTracking()
                         .OrderBy(x => x.Id)
                         .ToList())
            {
                OrderStatuses.Add(status);
            }

            foreach (var pickupPoint in
                     db.PickupPoints
                         .AsNoTracking()
                         .OrderBy(x => x.Id)
                         .ToList())
            {
                PickupPoints.Add(
                    pickupPoint);
            }

            foreach (var user in
                     db.Users
                         .AsNoTracking()
                         .Where(
                             x =>
                                 x.RoleId ==
                                 RoleIds.Client)
                         .OrderBy(x => x.Surname)
                         .ThenBy(x => x.Name)
                         .ToList())
            {
                Users.Add(user);
            }

            SelectedStatus =
                OrderStatuses.FirstOrDefault();

            SelectedPickupPoint =
                PickupPoints.FirstOrDefault();

            SelectedUser =
                Users.FirstOrDefault();

            SelectedProduct =
                Products.FirstOrDefault();

            if (!IsEditMode)
                return;

            var order =
                db.Orders
                    .AsNoTracking()
                    .Include(x => x.OrderProducts)
                        .ThenInclude(x => x.Product)
                    .FirstOrDefault(
                        x => x.Id == _orderId!.Value);

            if (order == null)
            {
                ErrorMessages.Add(
                    "Заказ не найден.");

                return;
            }

            SelectedStatus =
                OrderStatuses.FirstOrDefault(
                    x => x.Id == order.OrderStatusId);

            SelectedPickupPoint =
                PickupPoints.FirstOrDefault(
                    x => x.Id == order.PickupPointId);

            SelectedUser =
                Users.FirstOrDefault(
                    x => x.Id == order.UserId);

            OrderDate =
                new DateTime(
                    order.Date.Year,
                    order.Date.Month,
                    order.Date.Day);

            DeliveryDate =
                new DateTime(
                    order.DeliveryDate.Year,
                    order.DeliveryDate.Month,
                    order.DeliveryDate.Day);

            OrderProducts.Clear();

            foreach (var orderProduct
                     in order.OrderProducts)
            {
                var product =
                    Products.FirstOrDefault(
                        x =>
                            x.Id ==
                            orderProduct.ProductId);

                if (product == null)
                    continue;

                OrderProducts.Add(
                    new OrderProductEditItemViewModel(
                        product,
                        orderProduct.Quantity,
                        RemoveProduct));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            ErrorMessages.Add(
                "Не удалось загрузить данные заказа.");
        }
    }

    private void AddProduct()
    {
        ErrorMessages.Clear();

        if (SelectedProduct == null)
        {
            ErrorMessages.Add(
                "Выберите товар.");

            return;
        }

        if (!int.TryParse(
                NewQuantityText,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int quantity)
            || quantity <= 0)
        {
            ErrorMessages.Add(
                "Количество должно быть целым числом больше нуля.");

            return;
        }

        bool alreadyExists =
            OrderProducts.Any(
                x =>
                    x.Product.Id ==
                    SelectedProduct.Id);

        if (alreadyExists)
        {
            ErrorMessages.Add(
                "Этот товар уже добавлен в заказ. Измените его количество в списке ниже.");

            return;
        }

        OrderProducts.Add(
            new OrderProductEditItemViewModel(
                SelectedProduct,
                quantity,
                RemoveProduct));

        NewQuantityText = "1";
    }

    private void RemoveProduct(
        OrderProductEditItemViewModel item)
    {
        OrderProducts.Remove(item);
    }

    private bool Validate(
        out List<(int ProductId, int Quantity)> items)
    {
        items = [];

        if (OrderProducts.Count == 0)
        {
            ErrorMessages.Add(
                "Добавьте хотя бы один товар в заказ.");
        }

        var usedProductIds =
            new HashSet<int>();

        foreach (var item in OrderProducts)
        {
            if (!item.TryGetQuantity(
                    out int quantity))
            {
                ErrorMessages.Add(
                    $"Некорректное количество товара «{item.Product.SKU}».");

                continue;
            }

            if (!usedProductIds.Add(
                    item.Product.Id))
            {
                ErrorMessages.Add(
                    "Один и тот же товар нельзя добавить в заказ несколько раз.");

                continue;
            }

            items.Add(
                (item.Product.Id, quantity));
        }

        if (SelectedUser == null)
        {
            ErrorMessages.Add(
                "Выберите клиента.");
        }
        else if (SelectedUser.RoleId !=
                 RoleIds.Client)
        {
            ErrorMessages.Add(
                "Выбранный пользователь не является клиентом.");
        }

        if (SelectedStatus == null)
        {
            ErrorMessages.Add(
                "Выберите статус заказа.");
        }

        if (SelectedPickupPoint == null)
        {
            ErrorMessages.Add(
                "Выберите пункт выдачи.");
        }

        if (OrderDate == null)
        {
            ErrorMessages.Add(
                "Укажите дату заказа.");
        }

        if (DeliveryDate == null)
        {
            ErrorMessages.Add(
                "Укажите дату выдачи.");
        }

        if (OrderDate != null &&
            DeliveryDate != null &&
            DeliveryDate.Value.Date <
            OrderDate.Value.Date)
        {
            ErrorMessages.Add(
                "Дата выдачи не может быть раньше даты заказа.");
        }

        return ErrorMessages.Count == 0;
    }

    private void Save()
    {
        ErrorMessages.Clear();

        if (!Validate(
                out List<(int ProductId, int Quantity)> items))
        {
            return;
        }

        try
        {
            using var db =
                App.CreateDbContext();

            using var transaction =
                db.Database.BeginTransaction();

            Order order;

            if (IsEditMode)
            {
                order =
                    db.Orders
                        .FirstOrDefault(
                            x =>
                                x.Id ==
                                _orderId!.Value)!;

                if (order == null)
                {
                    ErrorMessages.Add(
                        "Заказ не найден.");

                    return;
                }
            }
            else
            {
                order = new Order
                {
                    UserId = SelectedUser!.Id,
                    Code = 0,
                    Date =
                        DateOnly.FromDateTime(
                            OrderDate!.Value),
                    DeliveryDate =
                        DateOnly.FromDateTime(
                            DeliveryDate!.Value),
                    PickupPointId =
                        SelectedPickupPoint!.Id,
                    OrderStatusId =
                        SelectedStatus!.Id
                };

                db.Orders.Add(order);

                db.SaveChanges();

                order.Code =
                    order.Id + 900;
            }

            order.UserId =
                SelectedUser!.Id;

            order.Date =
                DateOnly.FromDateTime(
                    OrderDate!.Value);

            order.DeliveryDate =
                DateOnly.FromDateTime(
                    DeliveryDate!.Value);

            order.PickupPointId =
                SelectedPickupPoint!.Id;

            order.OrderStatusId =
                SelectedStatus!.Id;

            var existing =
                db.OrderProducts
                    .Where(
                        x =>
                            x.OrderId ==
                            order.Id)
                    .ToDictionary(
                        x => x.ProductId);

            var selectedIds =
                new HashSet<int>();

            foreach (var item in items)
            {
                selectedIds.Add(
                    item.ProductId);

                if (existing.TryGetValue(
                        item.ProductId,
                        out OrderProduct? existingItem))
                {
                    existingItem.Quantity =
                        item.Quantity;
                }
                else
                {
                    db.OrderProducts.Add(
                        new OrderProduct
                        {
                            OrderId =
                                order.Id,
                            ProductId =
                                item.ProductId,
                            Quantity =
                                item.Quantity
                        });
                }
            }

            foreach (var existingItem
                     in existing.Values)
            {
                if (!selectedIds.Contains(
                        existingItem.ProductId))
                {
                    db.OrderProducts.Remove(
                        existingItem);
                }
            }

            db.SaveChanges();

            transaction.Commit();

            _onSaved?.Invoke();

            _navigationService.GoBack();
        }
        catch (DbUpdateException ex)
        {
            Debug.WriteLine(ex);

            ErrorMessages.Add(
                "Не удалось сохранить заказ. Проверьте данные и повторите попытку.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            ErrorMessages.Add(
                "Произошла ошибка при сохранении заказа.");
        }
    }

    private void Back()
    {
        _navigationService.GoBack();
    }
}