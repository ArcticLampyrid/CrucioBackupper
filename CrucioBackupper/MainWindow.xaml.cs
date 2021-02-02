using CrucioBackupper.Crucio;
using CrucioBackupper.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrucioBackupper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await CrucioApi.Search(SearchContentTextBox.Text);
            var source = new List<CollectionViewModel>();
            var firstStoryMap = result.Data.Stories.ToDictionary(x => x.CollectionUuid);
            foreach (var item in result.Data.Collections.OrderByDescending(x => x.ClickCount))
            {
                var viewModel = new CollectionViewModel()
                {
                    Name = item.Name,
                    Desc = item.Desc,
                    StoryCount = item.StoryCount,
                    Uuid = item.Uuid,
                    ClickCount = item.ClickCount,
                    LikeCount = item.LikeCount,
                    ShareUuid = item.ShareUuid
                };
                try
                {
                    var firstStory = firstStoryMap[item.Uuid];
                    viewModel.CoverUuid = firstStory.CoverUuid;
                    viewModel.IsVideo = firstStory.IsVideoType;
                }
                catch (Exception)
                {
                }
                source.Add(viewModel);
            }
            SearchResultListView.ItemsSource = source;
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var collectionUuid = CollectionUuidTextBox.Text;
            if (string.IsNullOrWhiteSpace(collectionUuid))
            {
                MessageBox.Show("请先选定需要下载的目标");
                return;
            }
            var dialog = new SaveFileDialog()
            {
                Filter = "Dialogue Novel文件(*.dign)|*.dign"
            };
            if (!dialog.ShowDialog().GetValueOrDefault(false))
            {
                MessageBox.Show("操作取消");
                return;
            }
            var path = dialog.FileName;
            try
            {
                DownloadButton.IsEnabled = false;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                CrucioApi.SetToken(TokenTextBox.Text);
                using var target = ZipFile.Open(path, ZipArchiveMode.Create);
                await new CrucioDownloader(collectionUuid, target).Download();
            }
            finally 
            {
                DownloadButton.IsEnabled = true;
            }
            MessageBox.Show("下载完成，下载的Dialogue Novel文件(*.dign)可用本软件的 查看备份 功能查阅。");
        }

        private void ReadBackupButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "Dialogue Novel文件(*.dign)|*.dign"
            };
            if (!dialog.ShowDialog().GetValueOrDefault(false))
            {
                MessageBox.Show("操作取消");
                return;
            }
            new DignReader(dialog.FileName).Show();
        }

        private void FixImageProblem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("图片显示异常可能是由于您的电脑缺少WebP解码器，点击 确定 将开始安装解码器");
            try
            {
                var coderInstaller = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "WebpCodecSetup.exe");
                var processInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = coderInstaller,
                    Arguments = "/s"
                };
                Process.Start(processInfo);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"启动安装程序失败：{exception}");
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true, 
                FileName = link.NavigateUri.AbsoluteUri
            });
        }
    }
}
