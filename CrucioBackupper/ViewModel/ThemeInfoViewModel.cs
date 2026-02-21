using Avalonia;
using Avalonia.Styling;
using CrucioBackupper.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CrucioBackupper.ViewModel;

public sealed class ThemeInfoViewModel : INotifyPropertyChanged
{
    public const string ThemeModeLight = "light";
    public const string ThemeModeAuto = "auto";
    public const string ThemeModeDark = "dark";

    private static readonly IReadOnlyList<SelectableOption> ThemeModeOptions =
    [
        new SelectableOption(ThemeModeLight, "亮色"),
        new SelectableOption(ThemeModeAuto, "自动"),
        new SelectableOption(ThemeModeDark, "暗色"),
    ];

    private string? themeMode;

    private ThemeInfoViewModel()
    {
    }

    public static ThemeInfoViewModel Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<SelectableOption> Options => ThemeModeOptions;

    public string? ThemeMode
    {
        get => themeMode;
        set
        {
            var normalizedThemeMode = NormalizeThemeMode(value);
            if (string.Equals(themeMode, normalizedThemeMode, StringComparison.Ordinal))
            {
                return;
            }

            themeMode = normalizedThemeMode;
            ApplyThemeMode(themeMode);
            SaveThemeMode(themeMode);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ThemeMode)));
        }
    }

    public void Init()
    {
        var normalizedThemeMode = NormalizeThemeMode(Settings.Default.ThemeMode);
        themeMode = normalizedThemeMode;
        ApplyThemeMode(normalizedThemeMode);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ThemeMode)));
    }

    private static string NormalizeThemeMode(string? rawThemeMode)
    {
        if (string.Equals(rawThemeMode, ThemeModeLight, StringComparison.OrdinalIgnoreCase))
        {
            return ThemeModeLight;
        }

        if (string.Equals(rawThemeMode, ThemeModeDark, StringComparison.OrdinalIgnoreCase))
        {
            return ThemeModeDark;
        }

        return ThemeModeAuto;
    }

    private static void ApplyThemeMode(string normalizedThemeMode)
    {
        if (Application.Current == null)
        {
            return;
        }

        Application.Current.RequestedThemeVariant = normalizedThemeMode switch
        {
            ThemeModeLight => ThemeVariant.Light,
            ThemeModeDark => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }

    private static void SaveThemeMode(string normalizedThemeMode)
    {
        if (string.Equals(Settings.Default.ThemeMode, normalizedThemeMode, StringComparison.Ordinal))
        {
            return;
        }

        Settings.Default.ThemeMode = normalizedThemeMode;
        Settings.Default.Save();
    }
}
