using Avalonia;
using Avalonia.Controls.Primitives;

namespace Baksteen.Avalonia.Controls.Fluent;

public class SpinnerControl : TemplatedControl
{
    public static readonly StyledProperty<double> SpinnerThicknessProperty =
        AvaloniaProperty.Register<SpinnerControl, double>(nameof(SpinnerThickness), 50);

    public double SpinnerThickness
    {
        get => GetValue(SpinnerThicknessProperty);
        set => SetValue(SpinnerThicknessProperty, value);
    }
}