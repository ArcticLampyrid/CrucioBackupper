using System.Collections.ObjectModel;
using Avalonia.Threading;

namespace CrucioBackupper.LogViewer
{
    public class LogViewModel
    {
        public ObservableCollection<LogEntity> Entities { get; } = [];

        public virtual void Add(LogEntity entity)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => Entities.Add(entity), DispatcherPriority.Background);
                return;
            }

            Entities.Add(entity);
        }

        public virtual void Clear()
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => Entities.Clear(), DispatcherPriority.Background);
                return;
            }

            Entities.Clear();
        }
    }
}
