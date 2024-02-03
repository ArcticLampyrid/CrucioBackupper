using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CrucioBackupper.LogViewer
{
    public class LogViewModel
    {
        public ObservableCollection<LogEntity> Entities { get; } = [];

        public virtual void Add(LogEntity entity)
        {
            if (Application.Current is null || Application.Current.Dispatcher is null)
            {
                Entities.Add(entity);
                return;
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => Entities.Add(entity));
        }

        public virtual void Clear()
        {
            if (Application.Current is null || Application.Current.Dispatcher is null)
            {
                Entities.Clear();
                return;
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => Entities.Clear());
        }
    }
}
