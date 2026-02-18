using Serilog.Events;
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace CrucioBackupper.LogViewer
{
    public class LogEventLevelToBrushConverter : IValueConverter
    {
        private static readonly IBrush VerboseBrush = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
        private static readonly IBrush DebugBrush = new SolidColorBrush(Color.FromRgb(0x6A, 0x5A, 0xCD));
        private static readonly IBrush InformationBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x64, 0x00));
        private static readonly IBrush WarningBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00));
        private static readonly IBrush ErrorBrush = new SolidColorBrush(Color.FromRgb(0x8B, 0x00, 0x00));
        private static readonly IBrush FatalBrush = new SolidColorBrush(Color.FromRgb(0x80, 0x00, 0x00));

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is LogEventLevel level)
            {
                return level switch
                {
                    LogEventLevel.Verbose => VerboseBrush,
                    LogEventLevel.Debug => DebugBrush,
                    LogEventLevel.Information => InformationBrush,
                    LogEventLevel.Warning => WarningBrush,
                    LogEventLevel.Error => ErrorBrush,
                    LogEventLevel.Fatal => FatalBrush,
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
