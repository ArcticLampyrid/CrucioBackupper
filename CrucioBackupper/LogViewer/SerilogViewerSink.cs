using CrucioBackupper.ViewModel;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrucioBackupper.LogViewer
{
    public class SerilogViewerSink(IFormatProvider? formatProvider, LogViewModel viewModel) : ILogEventSink
    {
        private readonly IFormatProvider? formatProvider = formatProvider;
        private readonly LogViewModel viewModel = viewModel;
        public void Emit(LogEvent logEvent)
        {
            var entity = new LogEntity
            {
                Timestamp = logEvent.Timestamp,
                Level = logEvent.Level,
                Message = logEvent.RenderMessage(formatProvider),
                Exception = logEvent.Exception,
            };
            viewModel.Add(entity);
        }
    }
}
