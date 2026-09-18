using System.Windows.Controls;
using WpfApp.ViewModels;

namespace WpfApp.Pages;

public partial class OrderEditPage : Page
{
    public OrderEditPage(OrderEditViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}