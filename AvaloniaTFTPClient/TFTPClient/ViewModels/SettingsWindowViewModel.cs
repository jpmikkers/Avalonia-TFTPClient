using System;
using Baksteen.Net.TFTP.Client;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static Baksteen.Net.TFTP.Client.TFTPClient;

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

    public SettingsWindowViewModel(TFTPClient.Settings? settings)
    {
        if(settings is not null)
        {
            Settings = settings;
        }
    }

    public SettingsWindowViewModel() : this(null)
    {
    }

    public TFTPClient.Settings Settings {
        get => new()
        {
            BlockSize = BlockSize,
            DontFragment = DontFragment,
            Retries = Retries,
            ResponseTimeout = TimeSpan.FromMilliseconds(Timeout),
            Ttl = (short)Ttl
        };

        set {
            DontFragment = value.DontFragment;
            BlockSize = value.BlockSize;
            Retries = value.Retries;
            Timeout = (int)value.ResponseTimeout.TotalMilliseconds;
            Ttl = value.Ttl;
        }
    }
}
