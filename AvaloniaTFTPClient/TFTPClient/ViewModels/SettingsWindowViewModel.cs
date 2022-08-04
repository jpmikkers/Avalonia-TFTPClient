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
using Avalonia.Data.Converters;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Controls;

namespace UIClient.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        private int _blockSize;
        private int _timeout = 0;
        private int _retries = 0;
        private int _ttl = 1;
        private bool _dontFragment;

        public int BlockSize { get => _blockSize; set => this.RaiseAndSetIfChanged(ref _blockSize, value); }
        public bool DontFragment { get => _dontFragment; set => this.RaiseAndSetIfChanged(ref _dontFragment, value); }
        public int Timeout { get => _timeout; set => this.RaiseAndSetIfChanged(ref _timeout, value); }
        public int Retries { get => _retries; set => this.RaiseAndSetIfChanged(ref _retries, value); }
        public int Ttl { get => _ttl; set => this.RaiseAndSetIfChanged(ref _ttl, value); }

        public ReactiveCommand<Unit, TFTPClient.Settings?> CommandOK { get; }
        public ReactiveCommand<Unit, TFTPClient.Settings?> CommandCancel { get; }

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
            CommandOK = ReactiveCommand.Create<TFTPClient.Settings?>(() => { 
                return new TFTPClient.Settings { 
                    BlockSize = _blockSize, 
                    DontFragment = _dontFragment, 
                    Retries = _retries, 
                    ResponseTimeout = TimeSpan.FromMilliseconds( _timeout), 
                    Ttl = (short)_ttl 
                }; 
            });
            CommandCancel = ReactiveCommand.Create<TFTPClient.Settings?>(() => { return null; });
        }
        public SettingsWindowViewModel() : this(null)
        {
        }
    }

    public class TextCaseConverter : IValueConverter
    {
        public static readonly TextCaseConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is int sourceNumber && targetType.IsAssignableTo(typeof(string)))
            {
                return sourceNumber.ToString();
            }
            // converter used for the wrong type
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is string sourceText && targetType.IsAssignableTo(typeof(int)))
            {
                var filtered = string.Join("",sourceText.Where(c => Char.IsDigit(c)).Select(c => new string(c,1)));
                if(int.TryParse(filtered, out var actualValue))
                {
                    return actualValue;
                }
            }
            // converter used for the wrong type
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }
    }
}
