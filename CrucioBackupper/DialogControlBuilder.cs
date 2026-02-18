using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CrucioBackupper.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrucioBackupper;

public sealed class DialogControlBuilder
{
    public interface ResourceProvider
    {
        Bitmap? LoadImage(string uuid);
        Task SaveAvatarAsync(string avatarUuid);
        Task SaveImageAsync(string imageUuid);
        Task SaveAudioAsync(string audioUuid);
        Task SaveVideoAsync(string videoUuid);
        Task PlayAudioAsync(string audioUuid);
        Task PlayVideoAsync(string videoUuid);
    }

    public sealed class RenderOptions
    {
        public bool IsOffscreen { get; init; }
    }

    private readonly ResourceProvider resourceProvider;
    private readonly RenderOptions renderOptions;
    private readonly IImage audioIcon;

    public DialogControlBuilder(
        ResourceProvider resourceProvider,
        RenderOptions? renderOptions = null)
    {
        this.resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
        this.renderOptions = renderOptions ?? new RenderOptions();
        audioIcon = ResolveAudioIcon();
    }

    public Control RenderDialog(DialogModel dialog)
    {
        var content = dialog.Type switch
        {
            "image" => CreateImageMessageControl(dialog),
            "audio" => CreateAudioMessageControl(dialog),
            "video" => CreateVideoMessageControl(dialog),
            _ => CreateTextMessageControl(dialog)
        };
        var transparentBackground = dialog.Type == "image" || dialog.Type == "video";
        return dialog.Character.Role switch
        {
            0 => CreateSystemChatMessageControl(content, transparentBackground),
            1 => CreateRightChatMessageControl(dialog, content, transparentBackground),
            2 => CreateLeftChatMessageControl(dialog, content, transparentBackground),
            _ => throw new NotSupportedException($"Unsupported character role: {dialog.Character.Role}")
        };
    }

    private Control CreateLeftChatMessageControl(DialogModel dialog, Control content, bool transparentBackground = false)
    {
        return CreateChatMessageControl(dialog, content, false, transparentBackground ? Brushes.Transparent : Brushes.WhiteSmoke);
    }

    private Control CreateRightChatMessageControl(DialogModel dialog, Control content, bool transparentBackground = false)
    {
        return CreateChatMessageControl(dialog, content, true, transparentBackground ? Brushes.Transparent : Brushes.LightGreen);
    }

    private Control CreateChatMessageControl(DialogModel dialog, Control content, bool alignRight, IBrush messageBackground)
    {
        var dockPanel = new DockPanel
        {
            HorizontalAlignment = alignRight ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Margin = new Thickness(5)
        };

        var avatar = new Image
        {
            Source = resourceProvider.LoadImage(dialog.Character.AvatarUuid),
            Width = 36,
            Height = 36,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = alignRight ? new Thickness(2, 0, 0, 0) : new Thickness(0, 0, 2, 0),
            Stretch = Stretch.UniformToFill,
            Clip = new EllipseGeometry(new Rect(0, 0, 36, 36)),
            ContextMenu = CreateContextMenu(
                "导出头像",
                () => resourceProvider.SaveAvatarAsync(dialog.Character.AvatarUuid))
        };

        DockPanel.SetDock(avatar, alignRight ? Dock.Right : Dock.Left);
        dockPanel.Children.Add(avatar);

        var characterName = new TextBlock
        {
            Text = dialog.Character.Name,
            HorizontalAlignment = alignRight ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Foreground = Brushes.DarkGray,
            Margin = new Thickness(0, 0, 0, 2)
        };

        DockPanel.SetDock(characterName, Dock.Top);
        dockPanel.Children.Add(characterName);

        dockPanel.Children.Add(new Border
        {
            Background = messageBackground,
            CornerRadius = new CornerRadius(5),
            ClipToBounds = true,
            HorizontalAlignment = alignRight ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Child = content
        });

        return dockPanel;
    }

    private static Control CreateSystemChatMessageControl(Control content, bool transparentBackground = false)
    {
        return new Border
        {
            Background = transparentBackground ? Brushes.Transparent : Brushes.LightGray,
            CornerRadius = new CornerRadius(5),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5),
            Padding = new Thickness(0),
            Child = content
        };
    }

    private Control CreateTextMessageControl(DialogModel dialog)
    {
        var text = dialog.Text ?? string.Empty;
        var control = new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brushes.Black,
            Background = Brushes.Transparent,
            Padding = new Thickness(5)
        };
        control.ContextMenu = CreateContextMenu(
            "复制",
            async () =>
            {
                if (TopLevel.GetTopLevel(control)?.Clipboard is { } clipboard)
                {
                    await clipboard.SetTextAsync(text);
                }
            });
        return control;
    }

    private Control CreateImageMessageControl(DialogModel dialog)
    {
        if (dialog.Image == null)
        {
            throw new InvalidOperationException($"Image dialog without image content");
        }

        return new Image
        {
            Source = resourceProvider.LoadImage(dialog.Image.Uuid),
            MaxWidth = 256,
            Stretch = Stretch.Uniform,
            ContextMenu = CreateContextMenu(
                "导出图片",
                () => resourceProvider.SaveImageAsync(dialog.Image.Uuid))
        };
    }

    private Control CreateAudioMessageControl(DialogModel dialog)
    {
        if (dialog.Audio == null)
        {
            throw new InvalidOperationException($"Audio dialog without audio content");
        }

        var layout = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Margin = new Thickness(5),
        };
        layout.Children.Add(new Image
        {
            Source = audioIcon,
            Width = 20
        });
        layout.Children.Add(new TextBlock
        {
            Text = FormatDuration(dialog.Audio.Duration),
            VerticalAlignment = VerticalAlignment.Center
        });

        if (renderOptions.IsOffscreen)
        {
            return layout;
        }
        else
        {
            var button = new Button
            {
                BorderThickness = new Thickness(0),
                ContextMenu = CreateContextMenu(
                    "导出音频",
                    () => resourceProvider.SaveAudioAsync(dialog.Audio.Uuid)),
                Content = layout
            };
            button.Click += async (_, _) => await resourceProvider.PlayAudioAsync(dialog.Audio.Uuid);
            return button;
        }
    }

    private Control CreateVideoMessageControl(DialogModel dialog)
    {
        if (dialog.Video == null)
        {
            throw new InvalidOperationException($"Video dialog without video content");
        }


        var layout = new Grid() { Margin = new Thickness(5) };
        layout.Children.Add(new Image
        {
            Source = resourceProvider.LoadImage(dialog.Video.CoverImageUuid),
            MaxWidth = 256,
            Stretch = Stretch.Uniform
        });

        layout.Children.Add(new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x7F, 0x0A, 0x0A, 0x0A)),
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                Children =
                {
                    new TextBlock
                    {
                        Text = "视频",
                        Foreground = Brushes.White,
                        Margin = new Thickness(4, 2)
                    },
                    new TextBlock
                    {
                        Text = FormatDuration(dialog.Video.Duration),
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(4, 2),
                        [Grid.ColumnProperty] = 1
                    }
                }
            }
        });

        if (renderOptions.IsOffscreen)
        {
            return layout;
        }
        else
        {
            var button = new Button
            {
                BorderThickness = new Thickness(0),
                Padding = new Thickness(5),
                ContextMenu = CreateContextMenu(
                    "导出视频",
                    () => resourceProvider.SaveVideoAsync(dialog.Video.Uuid)),
                Content = layout
            };
            button.Click += async (_, _) => await resourceProvider.PlayVideoAsync(dialog.Video.Uuid);
            return button;
        }
    }

    private ContextMenu? CreateContextMenu(string header, Func<Task> callback)
    {
        if (renderOptions.IsOffscreen)
        {
            return null;
        }

        var menuItem = new MenuItem { Header = header };
        menuItem.Click += async (_, _) => await callback();
        return new ContextMenu { ItemsSource = new[] { menuItem } };
    }

    private static string FormatDuration(long duration)
    {
        return duration >= 60000
            ? string.Format("{0}′{1}″", duration / 60000, (duration % 60000) / 1000)
            : string.Format("{0}″", duration / 1000);
    }

    private static IImage ResolveAudioIcon()
    {
        if (Application.Current?.FindResource("Icon_Audio") is IImage icon)
        {
            return icon;
        }

        throw new InvalidOperationException("音频图标资源不存在：Icon_Audio");
    }
}
