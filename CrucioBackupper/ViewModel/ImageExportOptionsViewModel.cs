using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CrucioBackupper.ViewModel;

public readonly record struct ImageExportOptions(double Scale, int LogicalWidth, IImageExportFormat Format);

public sealed class ImageExportOptionsViewModel : INotifyPropertyChanged
{
    public const int MinLogicalWidth = 1;

    private double scale = ImageExporter.DefaultScale;
    private decimal logicalWidth = ImageExporter.DefaultLogicalWidth;
    private string selectedFormat = ImageExportFormats.Png.DisplayName;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<string> FormatOptions { get; } = ImageExportFormats.DisplayNames;

    public double Scale
    {
        get => scale;
        set
        {
            scale = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scale)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScaleDescription)));
        }
    }

    public string ScaleDescription => $"{Scale:0}x";

    public decimal LogicalWidth
    {
        get => logicalWidth;
        set
        {
            if (logicalWidth == value)
            {
                return;
            }

            logicalWidth = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogicalWidth)));
        }
    }

    public string SelectedFormat
    {
        get => selectedFormat;
        set
        {
            if (selectedFormat == value)
            {
                return;
            }

            selectedFormat = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFormat)));
        }
    }

    public bool TryGetValidatedOptions(out ImageExportOptions options, out string invalidReason)
    {
        if (LogicalWidth < MinLogicalWidth || LogicalWidth > int.MaxValue)
        {
            options = default;
            invalidReason = "“逻辑宽度”必须是大于 0 的整数。";
            return false;
        }

        if (decimal.Truncate(LogicalWidth) != LogicalWidth)
        {
            options = default;
            invalidReason = "“逻辑宽度”必须是大于 0 的整数。";
            return false;
        }

        var logicalWidth = decimal.ToInt32(LogicalWidth);

        if (!ImageExportFormats.TryGetByDisplayName(SelectedFormat, out var format))
        {
            options = default;
            invalidReason = "“格式”必须是受支持的图像格式。";
            return false;
        }

        options = new ImageExportOptions(Scale, logicalWidth, format);
        invalidReason = string.Empty;
        return true;
    }
}
