using Avalonia.Controls;
using Avalonia.Threading;
using System.Timers;
using Avalonia.Interactivity;
using Avalonia.Dialogs;
using Avalonia.ReactiveUI;
using UIClient.ViewModels;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Baksteen.Net.TFTP.Client;

namespace UIClient.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        Timer timer;   

        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.InteractionOpenFile.RegisterHandler(DoShowOpenFileDialogAsync)));
            this.WhenActivated(d => d(ViewModel!.InteractionSaveFile.RegisterHandler(DoShowSaveFileDialogAsync)));
            this.WhenActivated(d => d(ViewModel!.InteractionShowError.RegisterHandler(DoShowErrorAsync)));
            this.WhenActivated(d => d(ViewModel!.InteractionShowSettings.RegisterHandler(DoShowSettingsAsync)));
            timer = new Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 100;
            timer.Start();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            //Dispatcher.UIThread.Post(() =>
            //{
            //    if(MyGrid.IsMeasureValid)
            //    {
            //        MinWidth = MyGrid.DesiredSize.Width + 120.0;
            //        MinHeight = MyGrid.DesiredSize.Height + 50.0;
            //    }
            //});
        }

        private async Task DoShowErrorAsync(InteractionContext<string, Unit> ic)
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Error", ic.Input);
            await messageBoxStandardWindow.ShowDialog(this);
            ic.SetOutput(Unit.Default);
        }

        private async Task DoShowSettingsAsync(InteractionContext<SettingsWindowViewModel, TFTPClient.Settings?> ic)
        {
            var dialog = new SettingsWindow();
            dialog.DataContext = ic.Input;
            var result = await dialog.ShowDialog<TFTPClient.Settings?>(this);
            ic.SetOutput(result);
        }

        private async Task DoShowOpenFileDialogAsync(InteractionContext<Unit,string?> ic)
        {
            var files=await new OpenFileDialog().ShowAsync(this);
            if(files!=null && files.Length>0)
            {
                ic.SetOutput(files[0]);
            }
            else
            {
                ic.SetOutput(null);
            }
        }

        private async Task DoShowSaveFileDialogAsync(InteractionContext<Unit, string?> ic)
        {
            ic.SetOutput(await new SaveFileDialog().ShowAsync(this));
        }
    }
}
