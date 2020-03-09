using System;
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
using Newtonsoft.Json;
using CrucioBackupper.ViewModel;
using Microsoft.Win32;

namespace CrucioBackupper
{
    /// <summary>
    /// DignReader.xaml 的交互逻辑
    /// </summary>
    public partial class DignReader : Window
    {
        private string resourceDirectory;
        private CollectionModel collectionModel;

        private DataTemplate leftChatMessageTemplate;
        private DataTemplate rightChatMessageTemplate;
        private DataTemplate systemChatMessageTemplate;
        private DataTemplate textMessageContentTemplate;
        private DataTemplate imageMessageContentTemplate;
        private DataTemplate audioMessageContentTemplate;
        private DataTemplate videoMessageContentTemplate;
        private readonly ZipArchive archive;
        private readonly HashSet<string> extractedFiles = new HashSet<string>();

        public string GetContentFilePath(string relativePath)
        {
            var result = Path.GetFullPath(Path.Combine(resourceDirectory, relativePath));
            if (!extractedFiles.Contains(relativePath))
            {
                extractedFiles.Add(relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(result));
                archive.GetEntry(relativePath)?.ExtractToFile(result, true);
            }
            return result;
        }

        public DignReader(string fileName)
        {
            this.archive = ZipFile.OpenRead(fileName);
            do
            {
                resourceDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            } while (Directory.Exists(resourceDirectory));
            Directory.CreateDirectory(resourceDirectory);

            collectionModel = JsonConvert.DeserializeObject<CollectionModel>(File.ReadAllText(GetContentFilePath("Manifest.json"), Encoding.UTF8));
            InitializeComponent();

            leftChatMessageTemplate = this.FindResource("LeftChatMessageTemplate") as DataTemplate;
            rightChatMessageTemplate = this.FindResource("RightChatMessageTemplate") as DataTemplate;
            systemChatMessageTemplate = this.FindResource("SystemChatMessageTemplate") as DataTemplate;
            textMessageContentTemplate = this.FindResource("TextMessageContentTemplate") as DataTemplate;
            imageMessageContentTemplate = this.FindResource("ImageMessageContentTemplate") as DataTemplate;
            audioMessageContentTemplate = this.FindResource("AudioMessageContentTemplate") as DataTemplate;
            videoMessageContentTemplate = this.FindResource("VideoMessageContentTemplate") as DataTemplate;

            CatalogListView.ItemsSource = collectionModel.Stories;

            if (CatalogListView.Items.Count > 0)
            {
                CatalogListView.SelectedIndex = 0;
            }

            IntroductionTabItem.DataContext = new BasicCollectionViewModel()
            {
                CoverUuid = collectionModel.CoverUuid,
                Desc = collectionModel.Desc,
                Name = collectionModel.Name,
                StoryCount = collectionModel.StoryCount
            };
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                Directory.Delete(resourceDirectory, true);
            }
            catch (Exception)
            {
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

            var storyModel = JsonConvert.DeserializeObject<StoryModel>(File.ReadAllText(GetContentFilePath($"Story/{selected.Seq}.json"), Encoding.UTF8));
            foreach (var dialog in storyModel.Dialogs)
            {
                FrameworkElement content;
                switch (dialog.Type)
                {
                    case "image":
                        content = (imageMessageContentTemplate.LoadContent() as FrameworkElement);
                        content.DataContext = GetContentFilePath($"Image/{dialog.Image.Uuid}.webp");
                        break;
                    case "audio":
                        content = (audioMessageContentTemplate.LoadContent() as FrameworkElement);
                        content.DataContext = new AudioMessageContentViewModel()
                        {
                            Duration = dialog.Audio.Duration,
                            Uuid = dialog.Audio.Uuid
                        };
                        break;
                    case "video":
                        content = (videoMessageContentTemplate.LoadContent() as FrameworkElement);
                        content.DataContext = new VideoMessageContentViewModel()
                        {
                            Duration = dialog.Video.Duration,
                            Uuid = dialog.Video.Uuid,
                            CoverPath = GetContentFilePath($"Image/{dialog.Video.CoverImageUuid}.webp")
                        };
                        break;
                    default:
                        content = (textMessageContentTemplate.LoadContent() as FrameworkElement);
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
                var chatMessageControl = chatMessageTemplate.LoadContent() as FrameworkElement;
                chatMessageControl.DataContext = chatMessageViewModel;
                DialogStackPanel.Children.Add(chatMessageControl);
            }
            DialogScrollViewer.ScrollToVerticalOffset(0);
        }

        private void PlayAudioButton_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (sender as Button).DataContext as AudioMessageContentViewModel;
            new MediaPlayer(new Uri(GetContentFilePath($"Audio/{dataContext.Uuid}.m4a"))).ShowDialog();
        }

        private void PlayVideoButton_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (sender as Button).DataContext as VideoMessageContentViewModel;
            new MediaPlayer(new Uri(GetContentFilePath($"Video/{dataContext.Uuid}.mp4"))).ShowDialog();
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
            var path = (sender as FrameworkElement).DataContext as string;
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
            var dataContext = (sender as FrameworkElement).DataContext as ChatMessageViewModel;
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
            var dataContext = (sender as FrameworkElement).DataContext as AudioMessageContentViewModel;
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
            var dataContext = (sender as FrameworkElement).DataContext as VideoMessageContentViewModel;
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
    }
}
