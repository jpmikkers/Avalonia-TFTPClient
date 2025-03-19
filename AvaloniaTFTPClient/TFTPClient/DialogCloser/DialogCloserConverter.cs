using System;
using Avalonia.Data.Converters;
using System.Globalization;
using Avalonia.Controls;

namespace UIClient.DialogCloser;

/// <summary>
/// ValueConverter that converts a Window to an <see cref="IDialogCloser"/>. With the following XAML declaration you can
/// then bind buttons to a command that accepts an IDialogCloser interface that lets you close the window/dialog from the
/// viewmodel:
/// 
/// <example>
///     
///     <code>
///         &lt;Button
///             IsDefault="True"
///             Command="{CompiledBinding DoOkCommand}"
///             CommandParameter="{CompiledBinding RelativeSource={RelativeSource AncestorType=Window}, Converter={StaticResource dialogCloserConverter}}"&gt;
///             OK
///         &lt;/Button&gt;
///     </code>
///     
///     And the following command in the viewmodel:
///     
///     <code>
///         [RelayCommand]
///         private async Task DoOk(IDialogCloser closer)
///         {
///             closer.Close(this);
///             await Task.CompletedTask;
///         }
///     </code>
/// </example>
/// </summary>
public class DialogCloserConverter : IValueConverter
{
    private class Wrapper(Window window) : IDialogCloser
    {
        public void Close() => window.Close();
        public void Close(object? result) => window.Close(result);
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is Window source)
        {
            return new Wrapper(source); 
        }
        throw new InvalidCastException();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}
