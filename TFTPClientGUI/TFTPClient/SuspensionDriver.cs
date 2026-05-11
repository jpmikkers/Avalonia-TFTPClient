using System;
using UIClient.ViewModels;
using System.Text.Json;
using System.IO;
namespace UIClient;

public class SuspensionDriver(string applicationName)
{
    private JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };

    private string GetLocalApplicationFolder()
    {
        var result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), applicationName);
        if(!Directory.Exists(result))
        {
            Directory.CreateDirectory(result);
        }
        return result;
    }

    // on linux: ~/.local/share/AvaloniaTFTPClient/State.json
    // on windows: %LocalAppData%\AvaloniaTFTPClient\State.json
    private string StatePath => Path.Combine(GetLocalApplicationFolder(), "State.json");

    public MainWindowViewModel LoadState()
    {
        try
        {
            using var stream = File.OpenRead(StatePath);
            var state = JsonSerializer.Deserialize<MySavedState>(stream);

            if(state != null)
            {
                return new()
                {
                    IsAutoGenerateNames = state.IsAutoGenerateNames,
                    IsDownload = state.IsDownload,
                    Server = state.Server,
                    RemoteDir = state.RemoteDir,
                    LocalFile = state.LocalFile,
                    RemoteFile = state.RemoteFile,
                    Settings = state.Settings.ToTFTPSettings()
                };
            }
        }
        catch
        {
        }
        return new MainWindowViewModel();
    }

    public void SaveState(MainWindowViewModel vm)
    {
        try
        {
            using var stream = File.Create(StatePath);
            jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
            JsonSerializer.Serialize(stream,
                new MySavedState
                {
                    Server = vm.Server,
                    IsDownload = vm.IsDownload,
                    LocalFile = vm.LocalFile,
                    RemoteFile = vm.RemoteFile,
                    RemoteDir = vm.RemoteDir,
                    IsAutoGenerateNames = vm.IsAutoGenerateNames,
                    Settings = MySavedSettings.FromTFTPSettings(vm.Settings)
                },
                jsonSerializerOptions);
        }
        catch
        {
        }
    }
}
