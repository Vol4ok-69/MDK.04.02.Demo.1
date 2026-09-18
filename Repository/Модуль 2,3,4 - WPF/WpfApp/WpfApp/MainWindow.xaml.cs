using WpfApp.Services;

namespace WpfApp;

public partial class MainWindow
{
    private readonly NavigationService _navigationService;
    private readonly SessionService _sessionService;

    public MainWindow()
    {
        InitializeComponent();

        _sessionService = new SessionService();

        _navigationService = new NavigationService(MainFrame, _sessionService);

        ShowAuthorizationPage();
    }

    public void ShowAuthorizationPage()
    {
        _sessionService.Clear();
        _navigationService.NavigateToAuthorization();
    }

    public void ShowProductsPage()
    {
        _navigationService.NavigateToProducts();
    }
}