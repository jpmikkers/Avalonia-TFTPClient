using Avalonia;
using Avalonia.Markup.Xaml;
using UIClient.ViewModels;
using System;
using Avalonia.Controls;
namespace UIClient.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if(DataContext is SettingsWindowViewModel viewModel)
        {
            viewModel.InteractionOnClose = x => Close(x);
        }
    }
}
