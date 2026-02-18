using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrucioBackupper.LogViewer
{
    public record LogEntity
    {
        public required DateTimeOffset Timestamp { get; init; }
        public required LogEventLevel Level { get; init; }
        public required string Message { get; init; }
        public Exception? Exception { get; init; }
        public string ExceptionMessage => Exception?.Message ?? string.Empty;
        public bool HasException => Exception != null;
    }
}
