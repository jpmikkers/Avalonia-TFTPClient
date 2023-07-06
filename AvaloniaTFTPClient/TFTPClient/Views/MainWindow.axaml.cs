using Avalonia.Controls;
using Avalonia.Threading;
using System.Timers;
using Avalonia.Interactivity;
using Avalonia.Dialogs;
using Avalonia.ReactiveUI;
using UIClient.ViewModels;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Baksteen.Net.TFTP.Client;
using Avalonia.Platform.Storage;
using MessageBox.Avalonia.DTO;

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
            var messageBoxStandardWindow = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
                new MessageBoxStandardParams { 
                    ContentTitle = "Error", 
                    ContentMessage = ic.Input 
                });
            await messageBoxStandardWindow.ShowWindowDialogAsync(this);
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
                    Title = "Select file" 
                });
            
            if(files!=null && files.Count>0)
            {
                ic.SetOutput(files[0].Path.AbsolutePath);
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
                    ShowOverwritePrompt = true, 
                    Title = "Save as.." 
                });

            if(file != null)
            {
                ic.SetOutput(file.Path.AbsolutePath);
            }
            else
            {
                ic.SetOutput(null);
            }
        }
    }
}
