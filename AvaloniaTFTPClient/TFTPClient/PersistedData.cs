using System;
using Baksteen.Net.TFTP.Client;

namespace UIClient
{
    public class MySavedState
    {
        public bool IsDownload { get; set; } = true;
        public bool IsAutoGenerateNames { get; set; }
        public string Server { get; set; } = String.Empty;
        public string RemoteFile { get; set; } = String.Empty;
        public string RemoteDir { get; set; } = String.Empty;
        public string LocalFile { get; set; } = String.Empty;
        public MySavedSettings Settings { get; set; } = MySavedSettings.FromTFTPSettings(new TFTPClient.Settings());
    }

    public class MySavedSettings
    {
        public int BlockSize { get; set; }
        public bool DontFragment { get; set; }
        public int Timeout { get; set; }
        public int Retries { get; set; }
        public int Ttl { get; set; }

        public static MySavedSettings FromTFTPSettings(TFTPClient.Settings settings)
        {
            return new MySavedSettings
            {
                BlockSize = settings.BlockSize,
                DontFragment = settings.DontFragment,
                Retries = settings.Retries,
                Timeout = (int)settings.ResponseTimeout.TotalMilliseconds,
                Ttl = settings.Ttl
            };
        }

        public TFTPClient.Settings ToTFTPSettings()
        {
            return new TFTPClient.Settings()
            {
                BlockSize = BlockSize,
                DontFragment = DontFragment,
                ResponseTimeout = TimeSpan.FromMilliseconds(Timeout),
                Retries = Retries,
                Ttl = (short)Ttl
            };
        }
    }
}
