using System.Windows.Controls;
using WpfApp.Pages;
using WpfApp.ViewModels;

namespace WpfApp.Services;

public class NavigationService(
    Frame frame,
    SessionService sessionService)
{
    private readonly Frame _frame = frame;
    private readonly SessionService _sessionService = sessionService;

    public void NavigateToAuthorization()
    {
        ClearHistory();

        var viewModel =
            new AuthorizationViewModel(
                this,
                _sessionService);

        _frame.Navigate(
            new AuthorizationPage(viewModel));
    }

    public void NavigateToProducts()
    {
        ClearHistory();

        var viewModel =
            new ProductsViewModel(
                this,
                _sessionService);

        _frame.Navigate(
            new ProductsPage(viewModel));
    }

    public void NavigateToProductEdit(
        int? productId = null)
    {
        if (!_sessionService.IsAdmin)
            return;

        var viewModel =
            new ProductEditViewModel(
                this,
                productId);

        _frame.Navigate(
            new ProductEditPage(viewModel));
    }

    public void NavigateToOrders()
    {
        if (!_sessionService.IsManager &&
            !_sessionService.IsAdmin)
        {
            return;
        }

        var viewModel =
            new OrdersViewModel(
                this,
                _sessionService);

        _frame.Navigate(
            new OrdersPage(viewModel));
    }

    public void NavigateToOrderEdit(
        int? orderId = null,
        Action? onSaved = null)
    {
        if (!_sessionService.IsAdmin)
            return;

        var viewModel =
            new OrderEditViewModel(
                this,
                orderId,
                onSaved);

        _frame.Navigate(
            new OrderEditPage(viewModel));
    }

    public void GoBack()
    {
        if (_frame.CanGoBack)
            _frame.GoBack();
    }

    public void ClearHistory()
    {
        while (_frame.CanGoBack)
            _frame.RemoveBackEntry();
    }
}