using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIClient.DialogCloser;

namespace UIClient.ViewModels;

public partial class ErrorWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Error";

    [ObservableProperty]
    private string _message = "Message";

    [ObservableProperty]
    private string _details = "Details";

    public ErrorWindowViewModel() { }

    [RelayCommand]
    private Task DoOk(IDialogCloser closer)
    {
        closer.Close();
        return Task.CompletedTask;
    }
}
