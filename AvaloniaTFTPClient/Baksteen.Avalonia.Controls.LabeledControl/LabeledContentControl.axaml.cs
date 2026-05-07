using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Linq;

namespace Baksteen.Avalonia.Controls.Fluent;

public class LabeledContentControl : ContentControl
{
    public static readonly StyledProperty<string?> HeaderProperty =
    AvaloniaProperty.Register<LabeledContentControl, string?>(nameof(Header));

    private Label? _headerLabel;
    private ContentPresenter? _contentPresenter;

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _headerLabel = e.NameScope.Find<Label>("PART_HeaderLabel");
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if(_headerLabel != null && _contentPresenter != null)
        {
            var firstFocusable = _contentPresenter.GetVisualDescendants()
                .OfType<Control>()
                .Where(c => c.Focusable && c.IsEnabled && c.IsEffectivelyVisible)
                .OrderBy(c => c.TabIndex)
                .FirstOrDefault();

            _headerLabel.Target = firstFocusable;
        }
    }
}