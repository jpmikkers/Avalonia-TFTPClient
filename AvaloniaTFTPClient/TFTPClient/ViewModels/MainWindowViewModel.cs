using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Net;
using System.Linq;
using Baksteen.Net.TFTP.Client;
using System.Net.Sockets;
using System.ComponentModel.DataAnnotations;

namespace UIClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _server = "localhost:69";
        private string _status = " ";
        private bool _isAutoGenerateNames = true;
        private bool _isDownload = true;
        private double _progress = 12.0;
        private string _remoteDir = "";
        private string _remoteFile = "";
        private string _localFile = "";
        private bool _isBusy = false;

        public bool IsBusy{ get => _isBusy; set => this.RaiseAndSetIfChanged(ref _isBusy,value); }

        public bool IsDownload { get => _isDownload; set => _isDownload = value; }

        public bool IsAutoGenerateNames
        {
            get => _isAutoGenerateNames;
            set
            {
                if(value != _isAutoGenerateNames)
                {
                    this.RaiseAndSetIfChanged(ref _isAutoGenerateNames, value);
                    if(!_isAutoGenerateNames)
                    {
                        RemoteDir = String.Empty;
                    }
                    else
                    {
                        GenerateRemoteFile();
                    }
                }
            }
        }

        public double Progress { get => _progress; set => this.RaiseAndSetIfChanged(ref _progress, value); }

        public string Status
        {
            get => _status;
            set
            {
                this.RaiseAndSetIfChanged(ref _status, value);
            }
        }

        public bool IsIndeterminateProgress { get; set; } = false;

        [Required(AllowEmptyStrings = false, ErrorMessage = "This field is required")]
        public string Server
        {
            get => _server;
            set
            {
                this.RaiseAndSetIfChanged(ref _server, value);
            }
        }

        public string RemoteDir
        {
            get => _remoteDir;
            set
            {
                if(_remoteDir != value)
                {
                    this.RaiseAndSetIfChanged(ref _remoteDir, value);
                    GenerateRemoteFile();
                }
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "This field is required")]
        public string RemoteFile 
        { 
            get => _remoteFile; 
            set { 
                this.RaiseAndSetIfChanged(ref _remoteFile, value); 
            } 
        }

        [Required(AllowEmptyStrings =false, ErrorMessage = "This field is required")]
        public string LocalFile 
        { 
            get => _localFile; 
            set {
                if(_localFile != value)
                {
                    this.RaiseAndSetIfChanged(ref _localFile, value);
                    GenerateRemoteFile();
                }
            } 
        }

        public TFTPClient.Settings Settings { get; set; } = new TFTPClient.Settings();

        public ReactiveCommand<Unit, Unit> CommandDownloadUpload { get; }
        public ReactiveCommand<Unit, Unit> CommandSelectFile { get; }
        public ReactiveCommand<Unit, Unit> CommandSettings { get; }

        public Interaction<Unit, string?> InteractionOpenFile { get; }
        public Interaction<Unit, string?> InteractionSaveFile { get; }
        public Interaction<string, Unit> InteractionShowError { get; }
        public Interaction<SettingsWindowViewModel, TFTPClient.Settings?> InteractionShowSettings { get; }

        private void GenerateRemoteFile()
        {
            if(IsAutoGenerateNames)
            {
                RemoteFile = Path.Combine(_remoteDir, Path.GetFileName(_localFile));
            }
        }

        private async Task DoDownloadUpload()
        {
            try
            {
                if(string.IsNullOrWhiteSpace(LocalFile))
                {
                    await InteractionShowError.Handle("Please enter a valid local filename");
                    return;
                }

                if(string.IsNullOrWhiteSpace(RemoteFile))
                {
                    await InteractionShowError.Handle("Please enter a valid remote filename");
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
                return int.TryParse(str, out int val) ? val : def;
            }

            IPEndPoint? result = null;
            IPAddress? address = null;
            int port = 69;

            // attempt to parse it as a ipv6 address
            var parts = server.Split(new string[] { "[", "]:" }, StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length > 0 && IPAddress.TryParse(parts[0], out address))
            {
                if(parts.Length > 1) port = ParseIntDefault(parts[1], 69);
                result = new IPEndPoint(address, port);
            }
            else
            {
                // no luck, try it as a ipv4 address
                parts = server.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                if(parts.Length > 0)
                {
                    if(parts.Length > 1) port = ParseIntDefault(parts[1], 69);

                    if(IPAddress.TryParse(parts[0], out address))
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
            //if(InvokeRequired)
            //{
            //    Invoke(new EventHandler<TFTPClient.ProgressEventArgs>(OnProgress), sender, e);
            //}
            //else
            //{
            //    toolStripStatusLabel1.Text = $"({e.Transferred}/{((e.TransferSize >= 0) ? e.TransferSize.ToString() : "?")} bytes) {(e.IsUpload ? "Uploading" : "Downloading")} '{e.Filename}'";
            //    toolStripProgressBar1.Value = (e.TransferSize > 0) ? (int)(100.0 * e.Transferred / e.TransferSize) : 0;
            //}
        }

        private async Task DoSelectFile()
        {
            if(IsDownload)
            {
                var fn = await InteractionSaveFile.Handle(Unit.Default);
                if(fn != null)
                {
                    LocalFile = fn;
                }
            }
            else
            {
                var fn = await InteractionOpenFile.Handle(Unit.Default);
                if(fn != null)
                {
                    LocalFile = fn;
                }
            }
        }

        private async Task DoSettings()
        {
            var settingsWindowViewModel = new SettingsWindowViewModel(Settings);
            var result = await InteractionShowSettings.Handle(settingsWindowViewModel);

            if(result != null)
            {
                Settings = result;
            }
        }

        public MainWindowViewModel()
        {
            CommandDownloadUpload = ReactiveCommand.CreateFromTask(DoDownloadUpload);
            CommandSelectFile = ReactiveCommand.CreateFromTask(DoSelectFile);
            CommandSettings = ReactiveCommand.CreateFromTask(DoSettings);
            InteractionOpenFile = new Interaction<Unit, string?>();
            InteractionSaveFile = new Interaction<Unit, string?>();
            InteractionShowError = new Interaction<string, Unit>();
            InteractionShowSettings = new Interaction<SettingsWindowViewModel, TFTPClient.Settings?>();
        }
    }
}
