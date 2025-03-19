using Avalonia;
using Avalonia.Controls;

namespace Baksteen.Avalonia.Tools.WindowCloser;

/// <summary>
/// WindowCloser attached behavior.
/// </summary>
public static class WindowCloser
{
    public static readonly AvaloniaProperty<bool> IsCloseCancelProperty = AvaloniaProperty.RegisterAttached<Button, bool>(
        "IsCloseCancel",
        typeof(WindowCloser),
        false,
        false);

    public static readonly AvaloniaProperty<bool> IsCloseOkayProperty = AvaloniaProperty.RegisterAttached<Button, bool>(
        "IsCloseOkay",
        typeof(WindowCloser),
        false,
        false);

    public static readonly AvaloniaProperty<object?> DialogResultProperty = AvaloniaProperty.RegisterAttached<Button, object?>(
    "DialogResult",
    typeof(WindowCloser));

    public static bool GetIsCloseOkay(AvaloniaObject obj)
    {
        return obj.GetValue(IsCloseOkayProperty) as bool? ?? false;
    }

    public static void SetIsCloseOkay(AvaloniaObject obj, bool value)
    {
        obj.SetValue(IsCloseOkayProperty, value);
        RegisterHooks(obj, value);
    }

    public static bool GetIsCloseCancel(AvaloniaObject obj)
    {
        return obj.GetValue(IsCloseCancelProperty) as bool? ?? false;
    }

    public static void SetIsCloseCancel(AvaloniaObject obj, bool value)
    {
        obj.SetValue(IsCloseCancelProperty, value);
        RegisterHooks(obj, value);
    }

    public static object? GetDialogResult(AvaloniaObject obj)
    {
        return obj.GetValue(DialogResultProperty);
    }

    public static void SetDialogResult(AvaloniaObject obj, object? value)
    {
        obj.SetValue(DialogResultProperty, value);
    }

    private static void RegisterHooks(AvaloniaObject obj, bool value)
    {
        if(obj is Button button)
        {
            if(value)
            {
                button.Unloaded -= Button_Unloaded;
                button.Unloaded += Button_Unloaded;

                button.Click -= Button_Click;
                button.Click += Button_Click;
            }
            else
            {
                button.Unloaded -= Button_Unloaded;
                button.Click -= Button_Click;
            }
        }
    }

    private static void Button_Unloaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if(sender is Button button)
        {
            button.Click -= Button_Click;
            button.Unloaded -= Button_Unloaded;
        }
    }

    private static void Button_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if(sender is Button button)
        {
            if(Window.GetTopLevel(button) is Window window)
            {
                if(GetIsCloseCancel(button))
                {
                    window.Close(null);
                }
                else
                {
                    if(button.IsSet(DialogResultProperty))
                    {
                        window.Close(button.GetValue(DialogResultProperty));
                    }
                    else
                    {
                        window.Close(window.DataContext);
                    }
                }
            }
        }
    }
}
