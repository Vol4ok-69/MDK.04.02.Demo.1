using System.Windows.Controls;
using WpfApp.ViewModels;

namespace WpfApp.Pages;

public partial class OrdersPage : Page
{
    public OrdersPage(OrdersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}