using Avalonia.Controls;
using Avalonia.Threading;
using System.Timers;
using Avalonia.Interactivity;
using Avalonia.Dialogs;
using Avalonia.ReactiveUI;
using UIClient.ViewModels;
using Avalonia.Win32;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Baksteen.Net.TFTP.Client;
using System;
using System.Reflection;
using Avalonia.Platform.Storage;
using System.Linq;
using Avalonia.Platform.Storage.FileIO;

namespace UIClient.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.InteractionOpenFile.RegisterHandler(DoShowOpenFileDialogAsync)));
            this.WhenActivated(d => d(ViewModel!.InteractionSaveFile.RegisterHandler(DoShowSaveFileDialogAsync)));
            this.WhenActivated(d => d(ViewModel!.InteractionShowError.RegisterHandler(DoShowErrorAsync)));
            this.WhenActivated(d => d(ViewModel!.InteractionShowSettings.RegisterHandler(DoShowSettingsAsync)));
            //this.Title = $"{Assembly.GetEntryAssembly()!.GetName().Version}";
        }

        private async Task DoShowErrorAsync(InteractionContext<string, Unit> ic)
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Error", ic.Input);
            await messageBoxStandardWindow.ShowDialog(this);
            ic.SetOutput(Unit.Default);
        }

        private async Task DoShowSettingsAsync(InteractionContext<SettingsWindowViewModel, TFTPClient.Settings?> ic)
        {
            var dialog = new SettingsWindow();
            dialog.DataContext = ic.Input;
            var result = await dialog.ShowDialog<TFTPClient.Settings?>(this);
            ic.SetOutput(result);
        }

        private async Task DoShowOpenFileDialogAsync(InteractionContext<Unit,string?> ic)
        {
            var files = await StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions { 
                    AllowMultiple = false, 
                    FileTypeFilter = new[] { FilePickerFileTypes.All },
                    Title = "Select file to upload.." 
                }
            );

            if(files.Count > 0)
            {
                if(files[0].TryGetUri(out var uri))
                {
                    ic.SetOutput(uri.LocalPath);
                }
            }
            else
            {
                ic.SetOutput(null);
            }
        }

        private async Task DoShowSaveFileDialogAsync(InteractionContext<Unit, string?> ic)
        {
            var file = await StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions { 
                    ShowOverwritePrompt = true,     // doesn't work most of the time for managed mode..
                    Title = "Save file as..",
                    FileTypeChoices = new[] { FilePickerFileTypes.All }
                }
            );

            if(file != null)
            {
                if(file.TryGetUri(out var uri))
                {
                    ic.SetOutput(uri.LocalPath);
                }
            }
            else
            {
                ic.SetOutput(null);
            }
        }
    }
}
