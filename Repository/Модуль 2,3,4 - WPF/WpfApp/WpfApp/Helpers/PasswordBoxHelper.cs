using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Helpers;

public static class PasswordBoxHelper
{
    public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.RegisterAttached
    (
        "BoundPassword",
        typeof(string),
        typeof(PasswordBoxHelper),
        new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnBoundPasswordChanged)
    );

    public static readonly DependencyProperty BindPasswordProperty = DependencyProperty.RegisterAttached
    (
        "BindPassword",
        typeof(bool),
        typeof(PasswordBoxHelper),
        new PropertyMetadata(false, OnBindPasswordChanged)
    );

    private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached
    (
        "IsUpdating",
        typeof(bool),
        typeof(PasswordBoxHelper)
    );

    public static void SetBoundPassword(DependencyObject element, string value) => element.SetValue(BoundPasswordProperty, value);


    public static string GetBoundPassword(DependencyObject element) => (string)element.GetValue(BoundPasswordProperty);


    public static void SetBindPassword(DependencyObject element, bool value) => element.SetValue(BindPasswordProperty, value);


    public static bool GetBindPassword(DependencyObject element) => (bool)element.GetValue(BindPasswordProperty);

    private static void OnBoundPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox passwordBox)
            return;

        passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

        if (!(bool)passwordBox.GetValue(IsUpdatingProperty))
            passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;


        passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
    }

    private static void OnBindPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox passwordBox)
            return;


        if ((bool)e.OldValue)
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;


        if ((bool)e.NewValue)
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;

    }

    private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
            return;


        passwordBox.SetValue(IsUpdatingProperty, true);
        SetBoundPassword(passwordBox, passwordBox.Password);
        passwordBox.SetValue(IsUpdatingProperty, false);
    }
}