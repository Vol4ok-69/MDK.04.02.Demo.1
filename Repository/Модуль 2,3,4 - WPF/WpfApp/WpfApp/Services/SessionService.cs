using WpfApp.Models;

namespace WpfApp.Services;

public class SessionService
{
    public User? CurrentUser { get; private set; }

    public bool IsGuest { get; private set; }

    public bool IsAuthenticated =>
        CurrentUser != null;

    public bool IsAdmin =>
        CurrentUser?.RoleId == RoleIds.Administrator;

    public bool IsManager =>
        CurrentUser?.RoleId == RoleIds.Manager;

    public bool IsClient =>
        CurrentUser?.RoleId == RoleIds.Client;

    public string FullName =>
        CurrentUser == null
            ? "Гость"
            : $"{CurrentUser.Surname} {CurrentUser.Name} {CurrentUser.Patronymic}";

    public string RoleName =>
        CurrentUser?.Role?.Name ?? "Гость";

    public void SetUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        CurrentUser = user;
        IsGuest = false;
    }

    public void SetGuest()
    {
        CurrentUser = null;
        IsGuest = true;
    }

    public void Clear()
    {
        CurrentUser = null;
        IsGuest = false;
    }
}