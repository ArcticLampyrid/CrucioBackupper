using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrucioBackupper.ViewModel
{
    public class ProgressViewModel : INotifyPropertyChanged
    {
        public int Current { get; private set; }
        public int Total { get; private set; }
        public double Progress => Total <= 0 ? 1 : (double)Current / Total;
        public event PropertyChangedEventHandler? PropertyChanged;
        public void SetProgress(int current, int total)
        {
            Current = current;
            Total = total;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Current)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Total)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
        }
    }
}
