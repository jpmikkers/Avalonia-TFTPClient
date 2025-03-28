using UIClient.ViewModels;
using System.Threading.Tasks;
using Baksteen.Net.TFTP.Client;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using System;
using System.Linq;
namespace UIClient.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        //this.Title = $"{Assembly.GetEntryAssembly()!.GetName().Version}";
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if(DataContext is MainWindowViewModel viewModel)
        {
            viewModel.InteractionOpenFile = DoShowOpenFileDialogAsync;
            viewModel.InteractionSaveFile = DoShowSaveFileDialogAsync;
            viewModel.InteractionShowError = DoShowErrorAsync;
            viewModel.InteractionShowSettings = DoShowSettingsAsync;
        }
    }

    private async Task DoShowErrorAsync(Exception ex)
    {
        var dialog = new ErrorWindow() 
        {
            DataContext = new ErrorWindowViewModel 
            {
                Details = ex.ToString(), 
                Message = ex.Message, 
                Title = "TFTPClient Error" 
            } 
        };
        await dialog.ShowDialog(this);
    }

    private async Task<TFTPClient.Settings?> DoShowSettingsAsync(TFTPClient.Settings settings)
    {
        var dialog = new SettingsWindow
        {
            DataContext = new SettingsWindowViewModel() 
            { 
                Settings = settings 
            }
        };
        return (await dialog.ShowDialog<SettingsWindowViewModel?>(this))?.Settings;
    }

    private async Task<string?> DoShowOpenFileDialogAsync()
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.All },
                Title = "Select file to upload.."
            }
        );

        return files.FirstOrDefault()?.TryGetLocalPath();
    }

    private async Task<string?> DoShowSaveFileDialogAsync()
    {
        var file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                ShowOverwritePrompt = true,     // doesn't work most of the time for managed mode..
                Title = "Save file as..",
                FileTypeChoices = [FilePickerFileTypes.All]
            }
        );

        return file?.Path.LocalPath;
    }
}
