using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using Baksteen.Net.TFTP.Client;
using System.Net.Sockets;
using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UIClient.ViewModels;

// use NotifyDataErrorInfo for mvvmct validation, see https://github.com/AvaloniaUI/Avalonia/issues/8397
// then on Apply/OK buttons you can bind IsEnabled to !HasErrors
public partial class MainWindowViewModel : ObservableValidator
{
    [ObservableProperty]
    [Required(AllowEmptyStrings = false, ErrorMessage = "This field is required")]
    [NotifyDataErrorInfo]
    private string _server = "localhost:69";

    [ObservableProperty]
    private string _status = " ";

    [ObservableProperty]
    private bool _isAutoGenerateNames = true;

    [ObservableProperty]
    private bool _isDownload = true;

    [ObservableProperty]
    private double _progress = 0.0;

    [ObservableProperty]
    private string _remoteDir = "";

    [ObservableProperty]
    [Required(AllowEmptyStrings = false, ErrorMessage = "This field is required")]
    [NotifyDataErrorInfo]
    private string _remoteFile = "";

    [ObservableProperty]
    [Required(AllowEmptyStrings = false, ErrorMessage = "This field is required")]
    [NotifyDataErrorInfo]
    private string _localFile = "";

    [ObservableProperty]
    private bool _isBusy = false;

    public bool IsIndeterminateProgress { get; set; } = false;

    partial void OnIsAutoGenerateNamesChanged(bool value) => GenerateRemoteFile();

    partial void OnRemoteDirChanged(string value) => GenerateRemoteFile();

    partial void OnLocalFileChanged(string value) => GenerateRemoteFile();

    public TFTPClient.Settings Settings { get; set; } = new TFTPClient.Settings();

    public Func<Task<string?>> InteractionOpenFile { get; set; } = () => Task.FromResult<string?>(null);

    public Func<TFTPClient.Settings, Task<TFTPClient.Settings?>> InteractionShowSettings { get; set; } = _ => Task.FromResult<TFTPClient.Settings?>(null);
    public Func<Task<string?>> InteractionSaveFile { get; set; } = () => Task.FromResult<string?>(null);

    public Func<string, Task> InteractionShowError { get; set;} = _ => Task.CompletedTask;

    private void GenerateRemoteFile()
    {
        if(IsAutoGenerateNames)
        {
            RemoteFile = Path.Combine(RemoteDir, Path.GetFileName(LocalFile));
        }
    }

    [RelayCommand]
    private async Task DoDownloadUpload()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(LocalFile))
            {
                await InteractionShowError("Please enter a valid local filename");
                return;
            }

            if(string.IsNullOrWhiteSpace(RemoteFile))
            {
                await InteractionShowError("Please enter a valid remote filename");
                return;
            }

            Progress = 0.0;
            Status = $"Starting {(IsDownload ? "download" : "upload")} ...";

            var settings = new TFTPClient.Settings
            {
                OnProgress = OnProgress,
                ProgressInterval = TimeSpan.FromMilliseconds(500),
                BlockSize = Settings.BlockSize,
                DontFragment = Settings.DontFragment,
                ResponseTimeout = Settings.ResponseTimeout,
                Retries = Settings.Retries,
                Ttl = Settings.Ttl
            };

            try
            {
                IsBusy = true;
                var endpoint = await ResolveServer(Server);

                if(IsDownload)
                {
                    await TFTPClient.DownloadAsync(endpoint, LocalFile, RemoteFile, settings);
                    Status = $"Download of '{RemoteFile}' complete.";
                }
                else
                {
                    await TFTPClient.UploadAsync(endpoint, LocalFile, RemoteFile, settings);
                    Status = $"Upload of '{RemoteFile}' complete.";
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
        catch(Exception ex)
        {
            Status = $"Error: '{ex.Message}'";
        }
    }

    /// <summary>
    /// Parses/resolves a string to an endpoint, supporting the following formats:
    /// x.x.x.x                 -> 127.0.0.1
    /// x.x.x.x:p               -> 127.0.0.1:69
    /// xxxx:xxxx:xxxx:xxxx     -> fe80::6982:bedb:3ffd:5741
    /// [xxxx:xxxx:xxxx:xxxx]:p -> [fe80::6982:bedb:3ffd:5741]:69
    /// hostname                -> localhost
    /// hostname:p              -> localhost:69
    /// </summary>
    /// <param name="server">string to parse</param>
    /// <returns>the IPEndPoint or null</returns>
    private static async Task<IPEndPoint> ResolveServer(string server)
    {
        // nested functions are cool
        static int ParseIntDefault(string str, int def)
        {
            return int.TryParse(str, out var val) ? val : def;
        }

        IPEndPoint? result = null;
        IPAddress? address = null;
        int port = 69;

        // attempt to parse it as a ipv6 address
        var parts = server.Split(["[", "]:"], StringSplitOptions.RemoveEmptyEntries);

        if(parts.Length > 0 && IPAddress.TryParse(parts[0], out address))
        {
            if(parts.Length > 1)
            {
                port = ParseIntDefault(parts[1], 69);
            }

            result = new IPEndPoint(address, port);
        }
        else
        {
            // no luck, try it as a ipv4 address
            parts = server.Split([":"], StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length > 0)
            {
                if(parts.Length > 1)
                {
                    port = ParseIntDefault(parts[1], 69);
                }

                if (IPAddress.TryParse(parts[0], out address))
                {
                    result = new IPEndPoint(address, port);
                }
                else
                {
                    // still nothing, resolve the hostname
                    var addressList = (await Dns.GetHostEntryAsync(parts[0])).AddressList;

                    // prefer ipv4 addresses, fall back to ipv6
                    address = addressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault() ??
                                addressList.Where(x => x.AddressFamily == AddressFamily.InterNetworkV6).FirstOrDefault();

                    if(address != null)
                    {
                        result = new IPEndPoint(address, port);
                    }
                }
            }
        }

        if(result == null)
        {
            throw new ArgumentException("Couldn't resolve the hostname or IP address");
        }

        return result;
    }

    private void OnProgress(object? sender, TFTPClient.ProgressEventArgs e)
    {
        Progress = (e.TransferSize > 0) ? (int)(100.0 * e.Transferred / e.TransferSize) : 0;
        Status = $"({e.Transferred}/{((e.TransferSize >= 0) ? e.TransferSize.ToString() : "?")} bytes) {(e.IsUpload ? "Uploading" : "Downloading")} '{e.Filename}'";
    }

    [RelayCommand]
    private async Task DoSelectFile()
    {
        if(IsDownload)
        {
            var fn = await InteractionSaveFile();
            if(fn != null)
            {
                LocalFile = fn;
            }
        }
        else
        {
            var fn = await InteractionOpenFile();
            if(fn != null)
            {
                LocalFile = fn;
            }
        }
    }

    [RelayCommand]
    private async Task DoSettings()
    {
        var result = await InteractionShowSettings(Settings);

        if(result != null)
        {
            Settings = result;
        }
    }

    public MainWindowViewModel() : base() 
    {
        ValidateAllProperties();
    }
}
