using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using CrucioBackupper.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrucioBackupper.Controls;

public partial class OptionSelectorControl : UserControl
{
    public static readonly StyledProperty<IReadOnlyList<SelectableOption>> OptionsProperty =
        AvaloniaProperty.Register<OptionSelectorControl, IReadOnlyList<SelectableOption>>(
            nameof(Options),
            Array.Empty<SelectableOption>());

    public static readonly StyledProperty<string?> SelectedValueProperty =
        AvaloniaProperty.Register<OptionSelectorControl, string?>(
            nameof(SelectedValue),
            default,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<OptionSelectorControl, SelectableOption?> SelectedOptionProperty =
        AvaloniaProperty.RegisterDirect<OptionSelectorControl, SelectableOption?>(
            nameof(SelectedOption),
            selector => selector.SelectedOption,
            (selector, option) => selector.SelectedOption = option,
            defaultBindingMode: BindingMode.TwoWay);

    private bool isSyncingSelection;
    private SelectableOption? selectedOption;

    static OptionSelectorControl()
    {
        OptionsProperty.Changed.AddClassHandler<OptionSelectorControl>((selector, _) => selector.SyncSelectedOptionFromValue());
        SelectedValueProperty.Changed.AddClassHandler<OptionSelectorControl>((selector, _) => selector.SyncSelectedOptionFromValue());
        SelectedOptionProperty.Changed.AddClassHandler<OptionSelectorControl>((selector, _) => selector.SyncSelectedValueFromOption());
    }

    public OptionSelectorControl()
    {
        InitializeComponent();
    }

    public IReadOnlyList<SelectableOption> Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public string? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public SelectableOption? SelectedOption
    {
        get => selectedOption;
        set => SetAndRaise(SelectedOptionProperty, ref selectedOption, value);
    }

    private void SyncSelectedOptionFromValue()
    {
        if (isSyncingSelection)
        {
            return;
        }

        var target = Options.FirstOrDefault(option => string.Equals(option.Value, SelectedValue, StringComparison.Ordinal));
        if (Equals(target, SelectedOption))
        {
            return;
        }

        isSyncingSelection = true;
        SelectedOption = target;
        isSyncingSelection = false;
    }

    private void SyncSelectedValueFromOption()
    {
        if (isSyncingSelection)
        {
            return;
        }

        var nextValue = SelectedOption?.Value;
        if (string.Equals(SelectedValue, nextValue, StringComparison.Ordinal))
        {
            return;
        }

        isSyncingSelection = true;
        SelectedValue = nextValue;
        isSyncingSelection = false;
    }
}
