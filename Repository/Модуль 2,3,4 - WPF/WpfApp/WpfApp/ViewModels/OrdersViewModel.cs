using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using WpfApp.Helpers;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels;

public class OrdersViewModel : ViewModelBase
{
    private readonly NavigationService _navigationService;
    private readonly SessionService _sessionService;

    public ObservableCollection<OrderItemViewModel> Orders { get; } = [];

    public string FullName =>
        _sessionService.FullName;

    public string RoleName =>
        _sessionService.RoleName;

    public bool IsAdmin =>
        _sessionService.IsAdmin;

    public ICommand AddOrderCommand { get; }

    public ICommand EditOrderCommand { get; }

    public ICommand DeleteOrderCommand { get; }

    public ICommand BackCommand { get; }

    public OrdersViewModel(
        NavigationService navigationService,
        SessionService sessionService)
    {
        _navigationService = navigationService;
        _sessionService = sessionService;

        AddOrderCommand =
            new RelayCommand(AddOrder);

        EditOrderCommand =
            new RelayCommand<OrderItemViewModel>(
                EditOrder);

        DeleteOrderCommand =
            new RelayCommand<OrderItemViewModel>(
                DeleteOrder);

        BackCommand =
            new RelayCommand(Back);

        _ = LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            using var db =
                App.CreateDbContext();

            var orders =
                await db.Orders
                    .AsNoTracking()
                    .Include(x => x.OrderStatus)
                    .Include(x => x.PickupPoint)
                    .Include(x => x.User)
                    .Include(x => x.OrderProducts)
                        .ThenInclude(x => x.Product)
                    .OrderBy(x => x.Id)
                    .ToListAsync();

            Orders.Clear();

            foreach (var order in orders)
            {
                Orders.Add(
                    new OrderItemViewModel(order));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            Orders.Clear();

            MessageBox.Show(
                "Не удалось загрузить заказы. Проверьте подключение к базе данных.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddOrder()
    {
        if (!IsAdmin)
            return;

        _navigationService.NavigateToOrderEdit(
            null,
            () => _ = LoadOrdersAsync());
    }

    private void EditOrder(
        OrderItemViewModel? order)
    {
        if (!IsAdmin || order == null)
            return;

        _navigationService.NavigateToOrderEdit(
            order.Id,
            () => _ = LoadOrdersAsync());
    }

    private void DeleteOrder(
        OrderItemViewModel? order)
    {
        if (!IsAdmin || order == null)
            return;

        var result =
            MessageBox.Show(
                $"Удалить заказ №{order.Code}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            using var db =
                App.CreateDbContext();

            using var transaction =
                db.Database.BeginTransaction();

            var dbOrder =
                db.Orders.FirstOrDefault(
                    x => x.Id == order.Id);

            if (dbOrder == null)
            {
                MessageBox.Show(
                    "Заказ не найден.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            var orderProducts =
                db.OrderProducts
                    .Where(
                        x =>
                            x.OrderId ==
                            order.Id)
                    .ToList();

            db.OrderProducts.RemoveRange(
                orderProducts);

            db.Orders.Remove(dbOrder);

            db.SaveChanges();

            transaction.Commit();

            _ = LoadOrdersAsync();
        }
        catch (DbUpdateException ex)
        {
            Debug.WriteLine(ex);

            MessageBox.Show(
                "Не удалось удалить заказ. Возможно, он используется в связанных данных.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            MessageBox.Show(
                "Произошла ошибка при удалении заказа.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void Back()
    {
        _navigationService.NavigateToProducts();
    }
}