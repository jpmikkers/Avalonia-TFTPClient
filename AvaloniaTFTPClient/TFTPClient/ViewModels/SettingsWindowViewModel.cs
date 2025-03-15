using System;
using Baksteen.Net.TFTP.Client;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UIClient.ViewModels;

public partial class SettingsWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _blockSize;

    [ObservableProperty]
    private int _timeout = 0;

    [ObservableProperty]
    private int _retries = 0;

    [ObservableProperty]
    private int _ttl = 1;

    [ObservableProperty]
    private bool _dontFragment;

    public Action<TFTPClient.Settings?> InteractionOnClose = x => { };

    public SettingsWindowViewModel(TFTPClient.Settings? settings)
    {
        if(settings != null)
        {
            _dontFragment = settings.DontFragment;
            _blockSize = settings.BlockSize;
            _retries = settings.Retries;
            _timeout = (int)settings.ResponseTimeout.TotalMilliseconds;
            _ttl = settings.Ttl;
        }
    }

    public SettingsWindowViewModel() : this(null)
    {
    }

    [RelayCommand]
    private void Okay()
    {
        InteractionOnClose(new TFTPClient.Settings
        {
            BlockSize = BlockSize,
            DontFragment = DontFragment,
            Retries = Retries,
            ResponseTimeout = TimeSpan.FromMilliseconds(Timeout),
            Ttl = (short)Ttl
        });
    }

    [RelayCommand]
    private void Cancel()
    {
        InteractionOnClose(null);
    }
}
