using System;
using System.Collections.Generic;
using System.Text;
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
    /// MediaPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class MediaPlayer : Window
    {
        private readonly DispatcherTimer timer;
        public MediaPlayer(Uri source)
        {
            InitializeComponent();
            Player.Source = source;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            PositionSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
            PositionSlider.Value = Player.Position.TotalSeconds;
            PositionLabel.Content = string.Format(
                "{0:00}:{1:00} / {2:00}:{3:00}",
                (long)Player.Position.TotalSeconds / 60,
                (long)Player.Position.TotalSeconds % 60,
                (long)Player.NaturalDuration.TimeSpan.TotalSeconds / 60,
                (long)Player.NaturalDuration.TimeSpan.TotalSeconds % 60);
            timer.Start();
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            this.Close();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            PositionSlider.Value = Player.Position.TotalSeconds;
            PositionLabel.Content = string.Format(
                "{0:00}:{1:00} / {2:00}:{3:00}",
                (long)Player.Position.TotalSeconds / 60,
                (long)Player.Position.TotalSeconds % 60,
                (long)Player.NaturalDuration.TimeSpan.TotalSeconds / 60,
                (long)Player.NaturalDuration.TimeSpan.TotalSeconds % 60);
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.Position = TimeSpan.FromSeconds(PositionSlider.Value);
        }
    }
}
