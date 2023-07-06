using Avalonia;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using UIClient.ViewModels;

namespace UIClient.Views
{
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
    }
}
