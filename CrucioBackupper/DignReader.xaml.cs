﻿using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using CrucioBackupper.Model;
using CrucioBackupper.ViewModel;
using Microsoft.Win32;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Windows.Threading;

namespace CrucioBackupper
{
    /// <summary>
    /// DignReader.xaml 的交互逻辑
    /// </summary>
    public partial class DignReader : Window
    {
        private readonly string resourceDirectory;
        private readonly CollectionModel collectionModel;
        private readonly ProgressViewModel exportProgressModel;

        private readonly DataTemplate leftChatMessageTemplate;
        private readonly DataTemplate rightChatMessageTemplate;
        private readonly DataTemplate systemChatMessageTemplate;
        private readonly DataTemplate textMessageContentTemplate;
        private readonly DataTemplate imageMessageContentTemplate;
        private readonly DataTemplate audioMessageContentTemplate;
        private readonly DataTemplate videoMessageContentTemplate;
        private readonly ZipArchive archive;
        private readonly HashSet<string> extractedFiles = [];
        private static readonly JsonSerializerOptions serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

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
                archive.GetEntry(relativePath)?.ExtractToFile(result, true);
            }
            return result;
        }

        public DignReader(string fileName)
        {
            try
            {
                this.archive = ZipFile.OpenRead(fileName);
                do
                {
                    resourceDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
                } while (Directory.Exists(resourceDirectory));
                Directory.CreateDirectory(resourceDirectory);

                var manifestFile = GetContentFilePath("Manifest.json");
                if (!File.Exists(manifestFile))
                {
                    throw new InvalidOperationException("Manifest.json not found");
                }
                collectionModel = JsonSerializer.Deserialize<CollectionModel>(File.ReadAllText(manifestFile, Encoding.UTF8), serializerOptions)!;
                InitializeComponent();

                leftChatMessageTemplate = (DataTemplate)this.FindResource("LeftChatMessageTemplate");
                rightChatMessageTemplate = (DataTemplate)this.FindResource("RightChatMessageTemplate");
                systemChatMessageTemplate = (DataTemplate)this.FindResource("SystemChatMessageTemplate");
                textMessageContentTemplate = (DataTemplate)this.FindResource("TextMessageContentTemplate");
                imageMessageContentTemplate = (DataTemplate)this.FindResource("ImageMessageContentTemplate");
                audioMessageContentTemplate = (DataTemplate)this.FindResource("AudioMessageContentTemplate");
                videoMessageContentTemplate = (DataTemplate)this.FindResource("VideoMessageContentTemplate");

                CatalogListView.ItemsSource = collectionModel.Stories;

                if (CatalogListView.Items.Count > 0)
                {
                    CatalogListView.SelectedIndex = 0;
                }

                var collectionViewModel = new BasicCollectionViewModel()
                {
                    CoverUuid = collectionModel.CoverUuid,
                    Desc = collectionModel.Desc,
                    Name = collectionModel.Name,
                    StoryCount = collectionModel.StoryCount
                };
                collectionViewModel.UseCustomCoverUrl(GetContentFilePath($"Image/{collectionModel.CoverUuid}.webp"));
                IntroductionTabItem.DataContext = collectionViewModel;

                exportProgressModel = new();
                ExportProgressArea.DataContext = exportProgressModel;
            }
            catch (Exception)
            {
                Close();
                throw;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                archive.Dispose();
                Directory.Delete(resourceDirectory, true);
            }
            catch (Exception exception)
            {
                Trace.TraceError($"Failed to delete resource directory \"{resourceDirectory}\": {exception}");
            }
        }

        private void AddDialogsToUICollection(UIElementCollection collection, int seq)
        {
            var storyModel = JsonSerializer.Deserialize<StoryModel>(File.ReadAllText(GetContentFilePath($"Story/{seq}.json"), Encoding.UTF8), serializerOptions)!;
            foreach (var dialog in storyModel.Dialogs)
            {
                FrameworkElement content;
                switch (dialog.Type)
                {
                    case "image":
                        content = (FrameworkElement)imageMessageContentTemplate.LoadContent();
                        if (dialog.Image == null)
                        {
                            throw new InvalidOperationException("Image dialog without image content");
                        }
                        content.DataContext = GetContentFilePath($"Image/{dialog.Image.Uuid}.webp");
                        break;
                    case "audio":
                        content = (FrameworkElement)audioMessageContentTemplate.LoadContent();
                        if (dialog.Audio == null)
                        {
                            throw new InvalidOperationException("Audio dialog without audio content");
                        }
                        content.DataContext = new AudioMessageContentViewModel()
                        {
                            Duration = dialog.Audio.Duration,
                            Uuid = dialog.Audio.Uuid
                        };
                        break;
                    case "video":
                        content = (FrameworkElement)videoMessageContentTemplate.LoadContent();
                        if (dialog.Video == null)
                        {
                            throw new InvalidOperationException("Video dialog without video content");
                        }
                        content.DataContext = new VideoMessageContentViewModel()
                        {
                            Duration = dialog.Video.Duration,
                            Uuid = dialog.Video.Uuid,
                            CoverPath = GetContentFilePath($"Image/{dialog.Video.CoverImageUuid}.webp")
                        };
                        break;
                    default:
                        content = (FrameworkElement)textMessageContentTemplate.LoadContent();
                        content.DataContext = dialog.Text;
                        break;
                }
                var chatMessageViewModel = new ChatMessageViewModel()
                {
                    AvatarPath = GetContentFilePath($"Image/{dialog.Character.AvatarUuid}.webp"),
                    CharacterName = dialog.Character.Name,
                    Content = content
                };
                var chatMessageTemplate = dialog.Character.Role switch
                {
                    0 => systemChatMessageTemplate,
                    1 => rightChatMessageTemplate,
                    2 => leftChatMessageTemplate,
                    _ => throw new NotSupportedException(),
                };
                var chatMessageControl = (FrameworkElement)chatMessageTemplate.LoadContent();
                chatMessageControl.DataContext = chatMessageViewModel;
                collection.Add(chatMessageControl);
            }
        }


        private void CatalogListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (CatalogListView.SelectedItem as BasicStoryModel);

            DialogStackPanel.Children.Clear();
            if (selected == null)
            {
                return;
            }
            AddDialogsToUICollection(DialogStackPanel.Children, selected.Seq);
            DialogScrollViewer.ScrollToVerticalOffset(0);
        }

        private void PlayAudioButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: AudioMessageContentViewModel dataContext })
            {
                new MediaPlayer(new Uri(GetContentFilePath($"Audio/{dataContext.Uuid}.m4a"))).ShowDialog();
            }
        }

        private void PlayVideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: VideoMessageContentViewModel dataContext })
            {
                new MediaPlayer(new Uri(GetContentFilePath($"Audio/{dataContext.Uuid}.mp4"))).ShowDialog();
            }
        }

        private void DialogScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;
            if (e.Delta > 0)
            {
                ZoomSlider.Value += ZoomSlider.LargeChange;
            }
            else if (e.Delta < 0)
            {
                ZoomSlider.Value -= ZoomSlider.LargeChange;
            }
            e.Handled = true;
        }

        private void SaveImageMessageMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: string path })
            {
                return;
            }
            var dialog = new SaveFileDialog()
            {
                Filter = "WebP图像文件(*.webp)|*.webp",
                FileName = Path.GetFileName(path)
            };
            if (dialog.ShowDialog().GetValueOrDefault(false))
            {
                File.Copy(path, dialog.FileName);
            }
        }

        private void SaveAvatarMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: ChatMessageViewModel dataContext })
            {
                return;
            }
            var path = dataContext.AvatarPath;
            var dialog = new SaveFileDialog()
            {
                Filter = "WebP图像文件(*.webp)|*.webp",
                FileName = Path.GetFileName(path)
            };
            if (dialog.ShowDialog().GetValueOrDefault(false))
            {
                File.Copy(path, dialog.FileName);
            }
        }

        private void SaveAudioMessageMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: AudioMessageContentViewModel dataContext })
            {
                return;
            }
            var path = GetContentFilePath($"Audio/{dataContext.Uuid}.m4a");
            var dialog = new SaveFileDialog()
            {
                Filter = "MPEG-4 Audio文件(*.m4a)|*.m4a",
                FileName = Path.GetFileName(path)
            };
            if (dialog.ShowDialog().GetValueOrDefault(false))
            {
                File.Copy(path, dialog.FileName);
            }
        }

        private void SaveVideoMessageMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: VideoMessageContentViewModel dataContext })
            {
                return;
            }
            var path = GetContentFilePath($"Video/{dataContext.Uuid}.mp4");
            var dialog = new SaveFileDialog()
            {
                Filter = "MPEG-4 Video文件(*.mp4)|*.mp4",
                FileName = Path.GetFileName(path)
            };
            if (dialog.ShowDialog().GetValueOrDefault(false))
            {
                File.Copy(path, dialog.FileName);
            }
        }

        private void SaveCoverMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: BasicCollectionViewModel dataContext })
            {
                return;
            }
            var path = dataContext.CoverUrl;
            var dialog = new SaveFileDialog()
            {
                Filter = "WebP图像文件(*.webp)|*.webp",
                FileName = Path.GetFileName(path)
            };
            if (dialog.ShowDialog().GetValueOrDefault(false))
            {
                File.Copy(path, dialog.FileName);
            }
        }

        private async void ExportAsTXT_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "请注意，TXT 格式无法保留对话体的完整结构信息，可能会造成信息丢失，亦不可用于导入得到 dign 格式。"
                + Environment.NewLine
                + "是否继续？", "导出", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            var collectionViewModel = IntroductionTabItem.DataContext as BasicCollectionViewModel;
            var dialog = new SaveFileDialog()
            {
                Filter = "ZIP压缩文件(*.zip)|*.zip",
                FileName = $"{collectionViewModel?.Name ?? "神秘作品"}.zip"
            };
            if (dialog.ShowDialog().GetValueOrDefault(false))
            {
                if (File.Exists(dialog.FileName))
                {
                    File.Delete(dialog.FileName);
                }
                using var txtPack = ZipFile.Open(dialog.FileName, ZipArchiveMode.Create);
                await new TextFileExporter(txtPack, archive).Export();
                MessageBox.Show(this, "导出完成", "导出", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void ExportAsPNG_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "请注意，PNG 格式无法逆向转换为文本格式或 *.dign 格式。"
                + Environment.NewLine
                + "是否继续？", "导出", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                ExportAsPNG.IsEnabled = false;
                ExportProgressArea.Visibility = Visibility.Visible;
                exportProgressModel.SetProgress(0, collectionModel.StoryCount);

                var collectionViewModel = IntroductionTabItem.DataContext as BasicCollectionViewModel;
                var dialog = new SaveFileDialog()
                {
                    Filter = "ZIP压缩文件(*.zip)|*.zip",
                    FileName = $"{collectionViewModel?.Name ?? "神秘作品"}.zip"
                };
                if (!dialog.ShowDialog().GetValueOrDefault(false))
                {
                    return;
                }
                if (File.Exists(dialog.FileName))
                {
                    File.Delete(dialog.FileName);
                }
                using var pngPack = ZipFile.Open(dialog.FileName, ZipArchiveMode.Create);
                using var cacheStream = new MemoryStream();
                for (int i = 0; i < collectionModel.StoryCount; i++)
                {
                    var panel = new StackPanel()
                    {
                        Orientation = Orientation.Vertical,
                        Width = 240,
                        Background = Brushes.White,
                    };

                    panel.Children.Add(new TextBlock()
                    {
                        Text = $"第 {i + 1} 话",
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 4),
                        HorizontalAlignment = HorizontalAlignment.Center
                    });
                    panel.Children.Add(new TextBlock()
                    {
                        Text = $"《{collectionModel.Name}》",
                        FontSize = 12,
                        Margin = new Thickness(0, 0, 0, 12),
                        HorizontalAlignment = HorizontalAlignment.Center
                    });
                    AddDialogsToUICollection(panel.Children, i + 1);
                    panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    panel.Arrange(new Rect(panel.DesiredSize));
                    panel.UpdateLayout();
                    panel.Dispatcher.Invoke(DispatcherPriority.ContextIdle, () => { });

                    double dpi = 384;
                    var bitmap = new RenderTargetBitmap(
                        (int)Math.Ceiling(panel.ActualWidth * (dpi / 96)),
                        (int)Math.Ceiling(panel.ActualHeight * (dpi / 96)),
                        dpi,
                        dpi,
                        PixelFormats.Pbgra32);
                    bitmap.Render(panel);

                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    cacheStream.SetLength(0);
                    encoder.Save(cacheStream);

                    using var targetStream = pngPack.CreateEntry($"{i + 1}.png").Open();
                    cacheStream.Seek(0, SeekOrigin.Begin);
                    await cacheStream.CopyToAsync(targetStream);

                    exportProgressModel.SetProgress(i + 1, collectionModel.StoryCount);
                }
            }
            finally
            {
                ExportAsPNG.IsEnabled = true;
                ExportProgressArea.Visibility = Visibility.Hidden;
            }
            MessageBox.Show(this, "导出完成", "导出", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
