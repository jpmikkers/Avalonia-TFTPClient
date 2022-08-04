using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using UIClient.ViewModels;
using System.Text.Json;
using System.IO;
using Baksteen.Net.TFTP.Client;

namespace UIClient
{
    public class SuspensionDriver : ISuspensionDriver
    {
        string ApplicationName = "AvaloniaTFTPClient";

        private string GetLocalApplicationFolder()
        {
            var result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), ApplicationName);
            if(!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            return result;
        }

        // on linux: ~/.local/share/AvaloniaTFTPClient/State.json
        // on windows: %LocalAppData%\AvaloniaTFTPClient\State.json
        private string StatePath => Path.Combine(GetLocalApplicationFolder(), "State.json");

        public IObservable<Unit> InvalidateState()
        {
            return Observable.Return(Unit.Default);
        }

        public IObservable<object> LoadState()
        {
            try
            {
                using(var stream = File.OpenRead(StatePath))
                {
                    var state = JsonSerializer.Deserialize<MySavedState>(stream);

                    if(state != null)
                    {
                        return Observable.Return(new MainWindowViewModel()
                        {
                            IsAutoGenerateNames = state.IsAutoGenerateNames,
                            IsDownload = state.IsDownload,
                            Server = state.Server,
                            RemoteDir = state.RemoteDir,
                            LocalFile = state.LocalFile,
                            RemoteFile = state.RemoteFile,
                            Settings = state.Settings.ToTFTPSettings()
                        });
                    }
                }
            }
            catch
            {
            }
            return Observable.Return(new MainWindowViewModel());
        }

        public IObservable<Unit> SaveState(object state)
        {
            try
            {
                if(state is MainWindowViewModel viewModel)
                {
                    using(var stream = File.Create(StatePath))
                    {
                        JsonSerializer.Serialize(stream,
                            new MySavedState
                            {
                                Server = viewModel.Server,
                                IsDownload = viewModel.IsDownload,
                                LocalFile = viewModel.LocalFile,
                                RemoteFile = viewModel.RemoteFile,
                                RemoteDir = viewModel.RemoteDir,
                                IsAutoGenerateNames = viewModel.IsAutoGenerateNames,
                                Settings = MySavedSettings.FromTFTPSettings(viewModel.Settings)
                            },
                            new JsonSerializerOptions { WriteIndented = true });
                    }
                }
            }
            catch
            {
            }
            return Observable.Return(Unit.Default);
        }
    }
}
