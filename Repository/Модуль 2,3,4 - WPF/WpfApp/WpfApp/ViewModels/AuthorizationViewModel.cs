using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Windows.Input;
using WpfApp.Helpers;
using WpfApp.Services;

namespace WpfApp.ViewModels;

public class AuthorizationViewModel : ViewModelBase
{
    private readonly NavigationService _navigationService;
    private readonly SessionService _sessionService;

    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;

    public string Login
    {
        get => _login;
        set
        {
            if (_login == value)
                return;

            _login = value;

            OnPropertyChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (_password == value)
                return;

            _password = value;

            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value)
                return;

            _errorMessage = value;

            OnPropertyChanged();
        }
    }

    public ICommand LoginCommand { get; }

    public ICommand GuestCommand { get; }

    public AuthorizationViewModel(
        NavigationService navigationService,
        SessionService sessionService)
    {
        _navigationService = navigationService;
        _sessionService = sessionService;

        LoginCommand =
            new RelayCommand(LoginUser);

        GuestCommand =
            new RelayCommand(LoginAsGuest);
    }

    private void LoginUser()
    {
        ErrorMessage = string.Empty;

        string login =
            Login.Trim();

        if (string.IsNullOrWhiteSpace(login))
        {
            ErrorMessage =
                "Введите логин.";

            return;
        }

        if (string.IsNullOrWhiteSpace(
                Password))
        {
            ErrorMessage =
                "Введите пароль.";

            return;
        }

        try
        {
            using var db =
                App.CreateDbContext();

            var user =
                db.Users
                    .AsNoTracking()
                    .Include(x => x.Role)
                    .FirstOrDefault(
                        x =>
                            x.Login == login &&
                            x.Password == Password);

            if (user == null)
            {
                ErrorMessage =
                    "Неверный логин или пароль.";

                Password = string.Empty;

                return;
            }

            _sessionService.SetUser(user);

            _navigationService.NavigateToProducts();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            ErrorMessage =
                "Не удалось выполнить авторизацию. Проверьте подключение к базе данных.";
        }
    }

    private void LoginAsGuest()
    {
        ErrorMessage = string.Empty;

        _sessionService.SetGuest();

        _navigationService.NavigateToProducts();
    }
}