using Serilog.Events;
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace CrucioBackupper.LogViewer
{
    public class LogEventLevelToBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is LogEventLevel level)
            {
                return level switch
                {
                    LogEventLevel.Verbose => new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)),// Dark gray
                    LogEventLevel.Debug => new SolidColorBrush(Color.FromRgb(0x6A, 0x5A, 0xCD)),// Slate blue
                    LogEventLevel.Information => new SolidColorBrush(Color.FromRgb(0x00, 0x64, 0x00)),// Dark green
                    LogEventLevel.Warning => new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00)),// Orange
                    LogEventLevel.Error => new SolidColorBrush(Color.FromRgb(0x8B, 0x00, 0x00)),// Dark red
                    LogEventLevel.Fatal => new SolidColorBrush(Color.FromRgb(0x80, 0x00, 0x00)),// Maroon
                    _ => Brushes.Black,
                };
            }

            return Brushes.Black;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
