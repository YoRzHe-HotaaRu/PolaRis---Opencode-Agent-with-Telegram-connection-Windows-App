using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using PolaRis.ViewModels;

namespace PolaRis;

public partial class MainWindow : Window
{
    private bool _isClosing;

    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;

        Loaded += async (s, e) =>
        {
            if (App.StartMinimized)
            {
                WindowState = WindowState.Minimized;
                Hide();
            }

            if (DataContext is MainViewModel vm)
            {
                await vm.StartAllCommand.ExecuteAsync(null);
            }
        };
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is MainViewModel vm)
        {
            vm.TrayService.ShowWindowRequested += (s, args) => Dispatcher.Invoke(ShowWindow);
        }
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (!_isClosing)
        {
            var vm = DataContext as MainViewModel;
            if (vm?.MinimizeToTray == true)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }

    private void ShowWindow_Click(object sender, RoutedEventArgs e)
    {
        ShowWindow();
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.NavigateToDashboardCommand.Execute(null);

        DashboardControl.Visibility = Visibility.Visible;
        SettingsControl.Visibility = Visibility.Collapsed;
        DashboardBtn.Style = (Style)FindResource("AccentButton");
        SettingsBtn.Style = (Style)FindResource("SecondaryButton");
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.NavigateToSettingsCommand.Execute(null);

        DashboardControl.Visibility = Visibility.Collapsed;
        SettingsControl.Visibility = Visibility.Visible;
        SettingsBtn.Style = (Style)FindResource("AccentButton");
        DashboardBtn.Style = (Style)FindResource("SecondaryButton");
    }

    private void StartAll_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            _ = vm.StartAllCommand.ExecuteAsync(null);
    }

    private void StopAll_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            _ = vm.StopAllCommand.ExecuteAsync(null);
    }

    private async void Exit_Click(object sender, RoutedEventArgs e)
    {
        _isClosing = true;

        if (DataContext is MainViewModel vm)
            await vm.ShutdownAsync();

        TrayIcon.Dispose();
        Application.Current.Shutdown();
    }
}

public class ViewToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string currentView && parameter is string targetView)
            return currentView == targetView ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
