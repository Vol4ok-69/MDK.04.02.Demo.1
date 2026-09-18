using System.Windows.Controls;
using WpfApp.ViewModels;

namespace WpfApp.Pages;

public partial class ProductEditPage : Page
{
    public ProductEditPage(ProductEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}