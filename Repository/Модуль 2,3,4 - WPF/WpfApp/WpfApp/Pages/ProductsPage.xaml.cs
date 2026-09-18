using System.Windows.Controls;
using WpfApp.ViewModels;

namespace WpfApp.Pages;

public partial class ProductsPage : Page
{
    public ProductsPage(ProductsViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}