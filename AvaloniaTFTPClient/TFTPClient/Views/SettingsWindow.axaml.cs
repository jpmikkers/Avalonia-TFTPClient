using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using UIClient.ViewModels;
using ReactiveUI;
using System;
namespace UIClient.Views;

public partial class SettingsWindow : ReactiveWindow<SettingsWindowViewModel>
{
    public SettingsWindow()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.CommandOK.Subscribe(Close)));
        this.WhenActivated(d => d(ViewModel!.CommandCancel.Subscribe(Close)));
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
