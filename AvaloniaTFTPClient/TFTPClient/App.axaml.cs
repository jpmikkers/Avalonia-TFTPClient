using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using UIClient.ViewModels;
using UIClient.Views;
namespace UIClient;

public partial class App : Application
{
    private readonly SuspensionDriver suspensionDriver = new("AvaloniaTFTPClient");

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT

            desktop.MainWindow = new MainWindow
            {
                DataContext = suspensionDriver.LoadState(),
            };

            desktop.ShutdownRequested += Desktop_ShutdownRequested;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Desktop_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        if(sender is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if(desktop.MainWindow?.DataContext is MainWindowViewModel vm)
            {
                suspensionDriver.SaveState(vm);
            }
        }
    }
}
