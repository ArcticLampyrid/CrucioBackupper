using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Themes.Simple;
using CrucioBackupper.Model;
using CrucioBackupper.ViewModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CrucioBackupper;

public partial class DignReader : Window
{
    private readonly string resourceDirectory;
    private readonly CollectionModel collectionModel;
    private readonly ProgressViewModel exportProgressModel;
    private readonly DialogsViewBuilder.ResourceProvider dialogsResourceProvider;
    private readonly ZipArchive? archive;
    private readonly HashSet<string> extractedFiles = [];
    private readonly LocalFileImageLoader imageLoader = new();
    private readonly VlcMediaPlaybackService mediaPlaybackService = new();

    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    private static readonly DialogsViewBuilder.RenderOptions interactiveRenderOptions = new();
    private static readonly DialogsViewBuilder.RenderOptions offscreenRenderOptions = new() { IsOffscreen = true };

    public DignReader() : this(fileName: null)
    {
    }

    public DignReader(string? fileName)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                archive = ZipFile.OpenRead(fileName);
            }

            InitializeComponent();

            do
            {
                resourceDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            } while (Directory.Exists(resourceDirectory));
            Directory.CreateDirectory(resourceDirectory);

            dialogsResourceProvider = CreateDialogsResourceProvider();
            exportProgressModel = new ProgressViewModel();
            ExportProgressArea.DataContext = exportProgressModel;

            var manifestFile = GetContentFilePath("Manifest.json");
            if (File.Exists(manifestFile))
            {
                collectionModel = JsonSerializer.Deserialize<CollectionModel>(File.ReadAllText(manifestFile, Encoding.UTF8), serializerOptions) ?? throw new Exception("无法解析 Manifest.json");
            }
            else
            {
                collectionModel = new CollectionModel
                {
                    Name = "未知藏品",
                    Desc = "无法加载资源包，可能是因为文件损坏或格式不受支持。",
                    CoverUuid = string.Empty,
                    StoryCount = 0,
                    Stories = []
                };
            }

            CollectionNameText.Text = collectionModel.Name;
            CollectionMetaText.Text = $"共 {collectionModel.StoryCount} 话";
            CollectionDescText.Text = collectionModel.Desc;

            var coverPath = GetContentFilePath($"Image/{collectionModel.CoverUuid}.webp");
            CollectionCoverImage.Source = imageLoader.Load(coverPath);
            var coverMenuItem = new MenuItem { Header = "导出封面" };
            coverMenuItem.Click += async (_, _) => await SaveFileCopyAsync(coverPath, "WebP图像文件", "*.webp");
            CollectionCoverImage.ContextMenu = new ContextMenu { ItemsSource = new[] { coverMenuItem } };

            CatalogListView.ItemsSource = collectionModel.Stories;
            if (CatalogListView.ItemCount > 0)
            {
                CatalogListView.SelectedIndex = 0;
            }
        }
        catch
        {
            Close();
            throw;
        }
    }

    public string GetContentFilePath(string relativePath)
    {
        var result = Path.GetFullPath(Path.Combine(resourceDirectory, relativePath));
        if (extractedFiles.Add(relativePath))
        {
            var directory = Path.GetDirectoryName(result);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            archive?.GetEntry(relativePath)?.ExtractToFile(result, true);
        }

        return result;
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        try
        {
            mediaPlaybackService.Dispose();
            imageLoader.Dispose();
            archive?.Dispose();
            Directory.Delete(resourceDirectory, true);
        }
        catch (Exception exception)
        {
            Trace.TraceError($"Failed to delete resource directory \"{resourceDirectory}\": {exception}");
        }
    }

    private static StoryModel LoadStoryModel(string storyPath)
    {
        return JsonSerializer.Deserialize<StoryModel>(File.ReadAllText(storyPath, Encoding.UTF8), serializerOptions)
            ?? throw new InvalidOperationException($"读取章节失败: {storyPath}");
    }

    private DialogsViewBuilder.ResourceProvider CreateDialogsResourceProvider()
    {
        return new DignReaderDialogsResourceProvider(this);
    }

    private sealed class DignReaderDialogsResourceProvider(DignReader owner) : DialogsViewBuilder.ResourceProvider
    {
        public Bitmap? LoadImage(string uuid)
        {
            return owner.imageLoader.Load(owner.GetContentFilePath($"Image/{uuid}.webp"));
        }

        public Task SaveAvatarAsync(string avatarUuid)
        {
            return owner.SaveFileCopyAsync(owner.GetContentFilePath($"Image/{avatarUuid}.webp"), "WebP图像文件", "*.webp");
        }

        public Task SaveImageAsync(string imageUuid)
        {
            return owner.SaveFileCopyAsync(owner.GetContentFilePath($"Image/{imageUuid}.webp"), "WebP图像文件", "*.webp");
        }

        public Task SaveAudioAsync(string audioUuid)
        {
            return owner.SaveFileCopyAsync(owner.GetContentFilePath($"Audio/{audioUuid}.m4a"), "MPEG-4 Audio文件", "*.m4a");
        }

        public Task SaveVideoAsync(string videoUuid)
        {
            return owner.SaveFileCopyAsync(owner.GetContentFilePath($"Video/{videoUuid}.mp4"), "MPEG-4 Video文件", "*.mp4");
        }

        public Task PlayAudioAsync(string audioUuid)
        {
            return owner.OpenMediaAsync(owner.GetContentFilePath($"Audio/{audioUuid}.m4a"), "音频播放");
        }

        public Task PlayVideoAsync(string videoUuid)
        {
            return owner.OpenMediaAsync(owner.GetContentFilePath($"Video/{videoUuid}.mp4"), "视频播放");
        }
    }

    private void AddDialogsToUICollection(Controls collection, BasicStoryModel story, bool isOffscreen)
    {
        var storyModel = LoadStoryModel(GetContentFilePath($"Story/{story.Seq}.json"));
        var builder = new DialogsViewBuilder(
            dialogsResourceProvider,
            story,
            storyModel.Dialogs,
            isOffscreen ? offscreenRenderOptions : interactiveRenderOptions);

        builder.AddTo(collection);
    }

    private async Task OpenMediaAsync(string path, string title)
    {
        try
        {
            mediaPlaybackService.Play(path);
        }
        catch (Exception exception)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "媒体播放",
                $"无法使用 VLC 播放“{title}”：{exception.Message}\n请确认已安装 VLC / libvlc。Arch Linux 可使用 pacman -S vlc 安装。",
                ButtonEnum.Ok).ShowWindowDialogAsync(this);
        }
    }

    private async Task SaveFileCopyAsync(string sourcePath, string fileTypeName, string pattern)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "导出文件",
            SuggestedFileName = Path.GetFileName(sourcePath),
            FileTypeChoices =
            [
                new FilePickerFileType(fileTypeName)
                {
                    Patterns = [pattern]
                }
            ],
            DefaultExtension = pattern.TrimStart('*')
        });

        var targetPath = file?.TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return;
        }

        File.Copy(sourcePath, targetPath, overwrite: true);
    }

    private void CatalogListView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        DialogStackPanel.Children.Clear();
        if (CatalogListView.SelectedItem is not BasicStoryModel selected)
        {
            CurrentStoryText.Text = string.Empty;
            return;
        }

        CurrentStoryText.Text = selected.DisplayName;
        AddDialogsToUICollection(DialogStackPanel.Children, selected, isOffscreen: false);
        DialogScrollViewer.Offset = new Vector(0, 0);
    }

    private void DialogScrollViewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            return;
        }

        if (e.Delta.Y > 0)
        {
            ZoomSlider.Value = Math.Min(ZoomSlider.Maximum, ZoomSlider.Value + ZoomSlider.LargeChange);
        }
        else if (e.Delta.Y < 0)
        {
            ZoomSlider.Value = Math.Max(ZoomSlider.Minimum, ZoomSlider.Value - ZoomSlider.LargeChange);
        }

        e.Handled = true;
    }

    private async void ExportAsTXT_Click(object? sender, RoutedEventArgs e)
    {
        if (archive is null)
        {
            await MessageBoxManager.GetMessageBoxStandard("导出", "无法导出：无有效资源", ButtonEnum.Ok).ShowWindowDialogAsync(this);
            return;
        }

        var shouldContinue = await MessageBoxManager.GetMessageBoxStandard(
            "导出",
            "请注意，TXT 格式无法保留对话体的完整结构信息，可能会造成信息丢失，亦不可用于导入得到 dign 格式。\n是否继续？",
            ButtonEnum.YesNo).ShowWindowDialogAsync(this) == ButtonResult.Yes;

        if (!shouldContinue)
        {
            return;
        }

        var txtFile = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "导出 TXT（ZIP）",
            SuggestedFileName = $"{collectionModel.Name}.zip",
            FileTypeChoices =
            [
                new FilePickerFileType("ZIP压缩文件")
                {
                    Patterns = ["*.zip"]
                }
            ],
            DefaultExtension = ".zip"
        });

        var path = txtFile?.TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var txtPack = ZipFile.Open(path, ZipArchiveMode.Create);
        await new TextFileExporter(txtPack, archive).Export();
        await MessageBoxManager.GetMessageBoxStandard("导出", "导出完成", ButtonEnum.Ok).ShowWindowDialogAsync(this);
    }

    private async void ExportAsPNG_Click(object? sender, RoutedEventArgs e)
    {
        var shouldContinue = await MessageBoxManager.GetMessageBoxStandard(
            "导出",
            "请注意，PNG 格式无法逆向转换为文本格式或 *.dign 格式。\n是否继续？",
            ButtonEnum.YesNo).ShowWindowDialogAsync(this) == ButtonResult.Yes;

        if (!shouldContinue)
        {
            return;
        }

        var pngFile = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "导出 PNG（ZIP）",
            SuggestedFileName = $"{collectionModel.Name}.zip",
            FileTypeChoices =
            [
                new FilePickerFileType("ZIP压缩文件")
                {
                    Patterns = ["*.zip"]
                }
            ],
            DefaultExtension = ".zip"
        });

        var path = pngFile?.TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            ExportAsPNG.IsEnabled = false;
            ExportProgressArea.IsVisible = true;

            var total = collectionModel.Stories?.Count ?? collectionModel.StoryCount;
            exportProgressModel.SetProgress(0, total);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using var pngPack = ZipFile.Open(path, ZipArchiveMode.Create);
            var stories = collectionModel.Stories ?? [];
            for (var i = 0; i < stories.Count; i++)
            {
                var story = stories[i];
                var exportControl = BuildStoryExportControl(story, i + 1);
                var pngBytes = RenderControlToPng(exportControl, 2.0);

                using var targetStream = pngPack.CreateEntry($"{story.Seq}.png").Open();
                await targetStream.WriteAsync(pngBytes, 0, pngBytes.Length);

                exportProgressModel.SetProgress(i + 1, total);
                await Task.Yield();
            }

            await MessageBoxManager.GetMessageBoxStandard("导出", "导出完成", ButtonEnum.Ok).ShowWindowDialogAsync(this);
        }
        catch (Exception exception)
        {
            await MessageBoxManager.GetMessageBoxStandard("导出", $"导出失败：{exception.Message}", ButtonEnum.Ok).ShowWindowDialogAsync(this);
        }
        finally
        {
            ExportAsPNG.IsEnabled = true;
            ExportProgressArea.IsVisible = false;
        }
    }

    private Control BuildStoryExportControl(BasicStoryModel story, int seq)
    {
        var panel = new StackPanel
        {
            Width = 340,
            Spacing = 8
        };
        var root = new Border
        {
            Background = Brushes.White,
            Padding = new Thickness(12),
            Child = panel
        };
        root.Styles.Add(new SimpleTheme());
        panel.Children.Add(new TextBlock
        {
            Text = $"第 {seq} 话",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        panel.Children.Add(new TextBlock
        {
            Text = $"《{collectionModel.Name}》",
            FontSize = 13,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 6)
        });

        AddDialogsToUICollection(panel.Children, story, isOffscreen: true);
        return root;
    }

    private static byte[] RenderControlToPng(Control control, double scale)
    {
        control.Measure(Size.Infinity);
        control.Arrange(new Rect(control.DesiredSize));

        var pixelWidth = Math.Max(1, (int)Math.Ceiling(control.Bounds.Width * scale));
        var pixelHeight = Math.Max(1, (int)Math.Ceiling(control.Bounds.Height * scale));

        using var bitmap = new RenderTargetBitmap(new PixelSize(pixelWidth, pixelHeight), new Vector(96 * scale, 96 * scale));
        bitmap.Render(control);

        using var stream = new MemoryStream();
        bitmap.Save(stream);
        return stream.ToArray();
    }
}
