using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
