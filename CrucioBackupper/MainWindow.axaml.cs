using CrucioNetwork;
using CrucioBackupper.ViewModel;
using AsyncImageLoader;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using CrucioBackupper.LogViewer;
using CrucioBackupper.Properties;

namespace CrucioBackupper;

public partial class MainWindow : Window
{
    private readonly CrucioApi api = CrucioApi.Default;
    private readonly LoginViewModel loginViewModel;

    public MainWindow()
    {
        InitializeComponent();

        var logViewModel = new LogViewModel();
        var log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Sink(new SerilogViewerSink(null, logViewModel))
            .CreateLogger();

        LogViewer.DataContext = logViewModel;
        Log.Logger = log;
        Log.Information("CrucioBackupper 已启动");

        loginViewModel = new LoginViewModel(api);
        loginViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(LoginViewModel.IsLoggedIn))
            {
                try
                {
                    Settings.Default.Token = api.GetToken();
                    Settings.Default.Save();
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "无法记住登录状态，保存 Token 失败");
                }
            }

            Dispatcher.UIThread.Post(ApplyLoginState);
        };

        VersionTextBlock.Content = AboutViewModel.Instance.VersionDesc;
        ApplyLoginState();

        _ = TryRestoreLoginAsync();
#if !DEBUG
        _ = CheckForUpdatesAsync();
#endif
    }

    private async Task TryRestoreLoginAsync()
    {
        try
        {
            var token = Settings.Default.Token;
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            await loginViewModel.LoginViaToken(token);
            Settings.Default.Token = api.GetToken();
            Settings.Default.Save();
            Dispatcher.UIThread.Post(ApplyLoginState);
            if (loginViewModel.IsLoggedIn)
            {
                Log.Information("自动登录成功");
            }
        }
        catch (Exception exception)
        {
            Log.Warning(exception, "自动登录失败");
        }
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var checker = new GitHubUpdateChecker("ArcticLampyrid", "CrucioBackupper");
            var htmlUrl = await checker.CheckForUpdatesAsync();
            if (string.IsNullOrWhiteSpace(htmlUrl))
            {
                return;
            }

            Dispatcher.UIThread.Post(async () =>
            {
                var shouldOpen = await MessageBoxManager.GetMessageBoxStandard("版本更新", "发现新版本，是否打开下载页面？", ButtonEnum.YesNo).ShowWindowDialogAsync(this) == ButtonResult.Yes;
                if (shouldOpen)
                {
                    OpenUrl(htmlUrl);
                }
            });
        }
        catch (Exception exception)
        {
            Log.Warning(exception, "自动检查更新失败");
        }
    }

    private void ApplyLoginState()
    {
        LoggedOutPanel.IsVisible = !loginViewModel.IsLoggedIn;
        LoggedInPanel.IsVisible = loginViewModel.IsLoggedIn;
        LoggedInNameText.Text = loginViewModel.Nickname ?? string.Empty;
        VipTagText.Text = loginViewModel.IsSvip
            ? "（SVIP）"
            : (loginViewModel.IsVip ? "（VIP）" : string.Empty);
        ImageLoader.SetSource(LoggedInAvatar, loginViewModel.AvatarUrl);
        LoggedInAvatar.Clip = new Avalonia.Media.EllipseGeometry(new Avalonia.Rect(0, 0, 18, 18));
    }

    private async void SearchButton_Click(object? sender, RoutedEventArgs e)
    {
        await PerformSearchAsync();
    }

    private async Task PerformSearchAsync()
    {
        try
        {
            var result = await api.Search(SearchContentTextBox.Text ?? string.Empty);
            if (result.HasError)
            {
                await MessageBoxManager.GetMessageBoxStandard("搜索", $"搜索失败：(Code: {result.Code}) {result.Msg}", ButtonEnum.Ok).ShowWindowDialogAsync(this);
                return;
            }

            var source = new List<CollectionViewModel>();
            var collectionMap = result.Data.Collections.ToDictionary(x => x.Uuid);
            var storyMap = result.Data.Stories.ToDictionary(x => x.Uuid);
            var userMap = result.Data.Users?.ToDictionary(x => x.Uuid) ?? [];

            foreach (var uuid in result.Data.SearchStoryUuids.List)
            {
                var story = storyMap[uuid];
                var item = collectionMap[story.CollectionUuid];
                var viewModel = new CollectionViewModel
                {
                    Name = item.Name,
                    Desc = item.Desc,
                    StoryCount = item.StoryCount,
                    Uuid = item.Uuid,
                    ClickCount = item.ClickCount,
                    LikeCount = item.LikeCount,
                    ShareUuid = item.ShareUuid,
                    CoverUuid = story.CoverUuid,
                    IsVideo = story.IsVideoType
                };

                if (userMap.TryGetValue(story.AuthorUuid, out var author))
                {
                    viewModel.Author = author.Name;
                }

                if (string.IsNullOrWhiteSpace(viewModel.Author))
                {
                    viewModel.Author = "未知";
                }

                source.Add(viewModel);
            }

            SearchResultListView.ItemsSource = source;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "搜索失败");
            await MessageBoxManager.GetMessageBoxStandard("搜索", $"搜索失败：{exception.Message}", ButtonEnum.Ok).ShowWindowDialogAsync(this);
        }
    }

    private void SearchResultListView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SearchResultListView.SelectedItem is CollectionViewModel selected)
        {
            CollectionUuidTextBox.Text = selected.Uuid;
        }
    }

    private async void DownloadButton_Click(object? sender, RoutedEventArgs e)
    {
        var collectionUuid = CollectionUuidTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(collectionUuid))
        {
            await MessageBoxManager.GetMessageBoxStandard("下载", "请先选定需要下载的目标", ButtonEnum.Ok).ShowWindowDialogAsync(this);
            return;
        }

        if (!loginViewModel.IsLoggedIn)
        {
            var shouldContinue = await MessageBoxManager.GetMessageBoxStandard(
                Title ?? "下载",
                "检测到您当前未登录，将无法保存需要解锁的章节部分。\n是否继续？",
                ButtonEnum.YesNo).ShowWindowDialogAsync(this) == ButtonResult.Yes;
            if (!shouldContinue)
            {
                return;
            }
        }

        var fileName = string.Empty;
        if (SearchResultListView.ItemsSource is List<CollectionViewModel> viewModels)
        {
            var selectedItem = viewModels.FirstOrDefault(x => x.Uuid == collectionUuid);
            if (selectedItem != null)
            {
                fileName = $"{selectedItem.Name} by {selectedItem.Author}.dign";
            }
        }

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "保存备份文件",
            SuggestedFileName = fileName,
            FileTypeChoices =
            [
                new FilePickerFileType("Dialogue Novel 文件")
                {
                    Patterns = ["*.dign"]
                }
            ],
            DefaultExtension = ".dign"
        });

        var path = file?.TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            DownloadButton.IsEnabled = false;
            await Task.Run(async () =>
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using var target = ZipFile.Open(path, ZipArchiveMode.Create);
                await new CrucioDownloader(api, collectionUuid, target).Download();
            });
            await MessageBoxManager.GetMessageBoxStandard("下载", "下载完成，可用本软件的“查看备份”功能查阅。", ButtonEnum.Ok).ShowWindowDialogAsync(this);
        }
        catch (Exception exception)
        {
            Log.Error(exception, "下载对话小说 {Uuid} 失败", collectionUuid);
            await MessageBoxManager.GetMessageBoxStandard("下载", $"下载失败：{exception.Message}", ButtonEnum.Ok).ShowWindowDialogAsync(this);
        }
        finally
        {
            DownloadButton.IsEnabled = true;
        }
    }

    private async void ReadBackupButton_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "打开备份文件",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Dialogue Novel 文件")
                {
                    Patterns = ["*.dign"]
                }
            ]
        });

        var path = files.Count == 0 ? null : files[0].TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            var dignReader = new DignReader(path);
            dignReader.Show();
        }
        catch (Exception exception)
        {
            Log.Error(exception, "打开对话小说文件失败");
            await MessageBoxManager.GetMessageBoxStandard("查看备份", $"打开文件失败：{exception}", ButtonEnum.Ok).ShowWindowDialogAsync(this);
        }
    }

    private async void SsoQr_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new SsoQrDialog();
        await dialog.ShowDialog(this);

        var validSsoInfo = dialog.ValidSsoInfo;
        if (validSsoInfo?.User != null && !string.IsNullOrWhiteSpace(dialog.Token))
        {
            await loginViewModel.LoginViaToken(dialog.Token);
            ApplyLoginState();
            await MessageBoxManager.GetMessageBoxStandard("登录成功", $"欢迎您，{validSsoInfo.User.Name}（{validSsoInfo.User.AuthorTypeText}）", ButtonEnum.Ok).ShowWindowDialogAsync(this);
        }
    }

    private void Window_DragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.None;
        if (TryGetSingleDignPath(e.DataTransfer, out _))
        {
            e.DragEffects = DragDropEffects.Link;
        }
        e.Handled = true;
    }

    private void Window_Drop(object? sender, DragEventArgs e)
    {
        if (!TryGetSingleDignPath(e.DataTransfer, out var path) || path == null)
        {
            return;
        }

        try
        {
            var dignReader = new DignReader(path);
            dignReader.Show();
        }
        catch (Exception exception)
        {
            Log.Error(exception, "打开对话小说文件失败");
        }
    }

    private static bool TryGetSingleDignPath(IDataTransfer dataTransfer, out string? path)
    {
        path = null;
        if (!dataTransfer.Contains(DataFormat.File))
        {
            return false;
        }

        var files = dataTransfer.TryGetFiles()?.ToList();
        if (files == null || files.Count != 1)
        {
            return false;
        }

        path = files[0].TryGetLocalPath();
        return !string.IsNullOrWhiteSpace(path) && path.EndsWith(".dign", StringComparison.OrdinalIgnoreCase);
    }

    private void LogoutButton_Click(object? sender, RoutedEventArgs e)
    {
        loginViewModel.Logout();
        ApplyLoginState();
    }

    private async void LoginViaTokenButton_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new LoginViaTokenDialog();
        await dialog.ShowDialog(this);
        if (!string.IsNullOrWhiteSpace(dialog.Token))
        {
            await loginViewModel.LoginViaToken(dialog.Token);
            ApplyLoginState();
        }
    }

    private async void CopyTokenButton_Click(object? sender, RoutedEventArgs e)
    {
        var token = api.GetToken() ?? "未登录";
        if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(token);
        }
        await MessageBoxManager.GetMessageBoxStandard("复制 Token", "Token 已复制到剪贴板", ButtonEnum.Ok).ShowWindowDialogAsync(this);
    }

    private void OpenReleasePageButton_Click(object? sender, RoutedEventArgs e)
    {
        OpenUrl("https://github.com/ArcticLampyrid/CrucioBackupper/releases");
    }

    private void OpenHomePageButton_Click(object? sender, RoutedEventArgs e)
    {
        OpenUrl("https://github.com/ArcticLampyrid/CrucioBackupper");
    }

    private static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = url
        });
    }
}
