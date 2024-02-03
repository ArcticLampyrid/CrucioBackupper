using CrucioNetwork;
using CrucioNetwork.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CrucioBackupper
{
    /// <summary>
    /// SsoQrDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SsoQrDialog : Window
    {
        private readonly DispatcherTimer loopValidTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 1)
        };
        public SsoQrInfo? QrInfo { get; private set; }
        public ValidSsoInfo? ValidSsoInfo { get; private set; }
        public string? Token { get; private set; }

        public SsoQrDialog()
        {
            InitializeComponent();

            loopValidTimer.Tick += LoopValidTimer_Tick;
            Closed += (sender, e) =>
            {
                loopValidTimer.Stop();
            };

            Task.Run(async () =>
            {
                var qrInfo = await CrucioApi.Default.RequireSsoQrInfo();
                if (!qrInfo.HasError)
                {
                    Dispatcher.Invoke(() =>
                    {
                        this.QrInfo = qrInfo.Data;
                        this.DataContext = this.QrInfo;
                        this.loopValidTimer.Start();
                    });
                }
            });
        }


        private async void LoopValidTimer_Tick(object? sender, EventArgs e)
        {
            var (x, y) = await CrucioApi.Default.ValidSsoQrInfo(QrInfo!);
            if (!x.HasError)
            {
                loopValidTimer.Stop();
                ValidSsoInfo = x.Data;
                Token = y;
                Close();
            }
        }
    }
}
