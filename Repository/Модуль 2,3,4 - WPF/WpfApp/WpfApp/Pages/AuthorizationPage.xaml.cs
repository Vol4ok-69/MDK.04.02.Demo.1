using System.Windows.Controls;
using WpfApp.ViewModels;

namespace WpfApp.Pages;

public partial class AuthorizationPage : Page
{
    public AuthorizationPage(AuthorizationViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}