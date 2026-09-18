using WpfApp.Models;

namespace WpfApp.ViewModels;

public class OrderItemViewModel(Order order)
{
    public Order Order { get; } = order;

    public int Id =>
        Order.Id;

    public int Code =>
        Order.Code;

    public string ProductsSummary =>
        string.Join(
            ", ",
            Order.OrderProducts.Select(
                x =>
                    $"{x.Product.SKU} × {x.Quantity}"));

    public string StatusName =>
        Order.OrderStatus.Name;

    public string PickupPointAddress =>
        $"{Order.PickupPoint.PostalIndex}, г. " +
        $"{Order.PickupPoint.City}, ул. " +
        $"{Order.PickupPoint.Street}, д. " +
        $"{Order.PickupPoint.BuildingNumber}";

    public string CustomerName =>
        $"{Order.User.Surname} " +
        $"{Order.User.Name} " +
        $"{Order.User.Patronymic}";

    public DateOnly OrderDate =>
        Order.Date;

    public DateOnly DeliveryDate =>
        Order.DeliveryDate;
}