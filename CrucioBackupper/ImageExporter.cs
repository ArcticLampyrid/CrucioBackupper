using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CrucioBackupper.Model;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CrucioBackupper;

public sealed class ImageExporter
{
    public const int DefaultLogicalWidth = 240;
    private const int StoryPadding = 0;
    private const int DialogSpacing = 0;
    private const int StoryHeaderSpacing = 4;
    private const int CollectionNameSpacing = 12;
    private const int BytesPerPixel = 4;
    public const double DefaultScale = 4.0;

    private static readonly DialogControlBuilder.RenderOptions offscreenRenderOptions = new() { IsOffscreen = true };

    private readonly CollectionModel collectionModel;
    private readonly Func<int, StoryModel> storyLoader;
    private readonly DialogControlBuilder viewBuilder;
    private readonly double scale;
    private readonly int logicalWidth;
    private readonly IImageExportFormat format;

    public ImageExporter(
        CollectionModel collectionModel,
        DialogControlBuilder.ResourceProvider resourceProvider,
        Func<int, StoryModel> storyLoader,
        double scale = DefaultScale,
        int logicalWidth = DefaultLogicalWidth,
        IImageExportFormat? format = null)
    {
        this.collectionModel = collectionModel ?? throw new ArgumentNullException(nameof(collectionModel));
        this.storyLoader = storyLoader ?? throw new ArgumentNullException(nameof(storyLoader));
        viewBuilder = new DialogControlBuilder(resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider)), offscreenRenderOptions);
        this.scale = scale > 0 ? scale : throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be greater than 0.");
        this.logicalWidth = logicalWidth > 0 ? logicalWidth : throw new ArgumentOutOfRangeException(nameof(logicalWidth), "Logical width must be greater than 0.");
        this.format = format ?? ImageExportFormats.Png;
    }

    public async Task ExportAsync(ZipArchive targetArchive, Func<int, int, Task>? reportProgress = null)
    {
        ArgumentNullException.ThrowIfNull(targetArchive);

        var reportTask = reportProgress?.Invoke(0, collectionModel.Stories.Count);
        if (reportTask != null)
        {
            await reportTask;
        }

        var extension = format.Extension;

        for (var i = 0; i < collectionModel.Stories.Count; i++)
        {
            var story = collectionModel.Stories[i];

            // Due to limitations of RenderTargetBitmap, rendering must be done in the UI thread
            var slices = RenderStorySlices(story);

            // Compose & encode the image in a background thread
            await Task.Run(async () =>
            {
                try
                {
                    using var storyBitmap = ComposeStoryBitmap(slices);
                    using var targetStream = targetArchive.CreateEntry($"{story.Seq}.{extension}", CompressionLevel.NoCompression).Open();
                    await format.SaveAsync(storyBitmap, targetStream);
                }
                finally
                {
                    foreach (var slice in slices)
                    {
                        slice.Bitmap.Dispose();
                    }
                }
            });

            reportTask = reportProgress?.Invoke(i + 1, collectionModel.Stories.Count);
            if (reportTask != null)
            {
                await reportTask;
            }
        }
    }

    private IReadOnlyList<RenderedSlice> RenderStorySlices(BasicStoryModel story)
    {
        // When rendering large bitmaps at once, RenderTargetBitmap may fail or produce incomplete results without throwing exceptions.
        // So we render each dialog separately and then combine them, which is more reliable even if it may use more memory temporarily.
        var storyModel = storyLoader(story.Seq);
        var slices = new List<RenderedSlice>(storyModel.Dialogs.Count + 2)
        {
            RenderSlice(CreateHeaderTextBlock($"第 {story.Seq} 话", 22, FontWeight.Bold), StoryHeaderSpacing),
            RenderSlice(CreateHeaderTextBlock($"《{collectionModel.Name}》", 13, FontWeight.Normal), CollectionNameSpacing)
        };

        foreach (var dialog in storyModel.Dialogs)
        {
            slices.Add(RenderSlice(viewBuilder.RenderDialog(dialog), DialogSpacing));
        }

        // Remove extra spacing after the last dialog.
        slices[^1] = new RenderedSlice
        {
            Bitmap = slices[^1].Bitmap,
            SpacingAfterPixels = 0
        };

        return slices;
    }

    private RenderedSlice RenderSlice(Control content, int spacingAfterDip)
    {
        var host = new Border
        {
            Width = logicalWidth,
            Background = Brushes.White,
            Child = content
        };
        return new RenderedSlice
        {
            Bitmap = RenderControlToBitmap(host, scale),
            SpacingAfterPixels = ToPixels(spacingAfterDip)
        };
    }

    private WriteableBitmap ComposeStoryBitmap(IReadOnlyList<RenderedSlice> slices)
    {
        var contentWidth = 0;
        var contentHeight = 0;
        foreach (var slice in slices)
        {
            contentWidth = Math.Max(contentWidth, slice.Bitmap.PixelSize.Width);
            contentHeight = checked(contentHeight + slice.Bitmap.PixelSize.Height + slice.SpacingAfterPixels);
        }

        var paddingPixels = ToPixels(StoryPadding);
        var pixelWidth = checked(contentWidth + paddingPixels * 2);
        var pixelHeight = checked(contentHeight + paddingPixels * 2);

        var output = new WriteableBitmap(
            new PixelSize(pixelWidth, pixelHeight),
            new Vector(96 * scale, 96 * scale),
            PixelFormats.Bgra8888,
            AlphaFormat.Premul);

        using var framebuffer = output.Lock();
        FillFramebufferWithWhite(framebuffer);

        var currentY = paddingPixels;
        foreach (var slice in slices)
        {
            var currentX = paddingPixels + Math.Max(0, (contentWidth - slice.Bitmap.PixelSize.Width) / 2);
            CopyBitmap(framebuffer, slice.Bitmap, currentX, currentY);
            currentY = checked(currentY + slice.Bitmap.PixelSize.Height + slice.SpacingAfterPixels);
        }

        return output;
    }

    private int ToPixels(double dip)
    {
        return Math.Max(0, (int)Math.Round(dip * scale));
    }

    private static TextBlock CreateHeaderTextBlock(string text, double fontSize, FontWeight fontWeight)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = fontSize,
            FontWeight = fontWeight,
            HorizontalAlignment = HorizontalAlignment.Center
        };
    }

    private static void FillFramebufferWithWhite(ILockedFramebuffer framebuffer)
    {
        var bytesPerPixel = framebuffer.Format.BitsPerPixel / 8;
        if (bytesPerPixel != BytesPerPixel)
        {
            throw new NotSupportedException($"Unsupported pixel format with {framebuffer.Format.BitsPerPixel} bits per pixel.");
        }

        var row = new byte[framebuffer.RowBytes];
        for (var x = 0; x < row.Length; x += BytesPerPixel)
        {
            row[x] = 0xFF;
            row[x + 1] = 0xFF;
            row[x + 2] = 0xFF;
            row[x + 3] = 0xFF;
        }

        for (var y = 0; y < framebuffer.Size.Height; y++)
        {
            Marshal.Copy(row, 0, IntPtr.Add(framebuffer.Address, checked(y * framebuffer.RowBytes)), row.Length);
        }
    }

    private static void CopyBitmap(ILockedFramebuffer target, Bitmap source, int targetX, int targetY)
    {
        if (targetX < 0 || targetY < 0)
        {
            throw new ArgumentOutOfRangeException($"Invalid target location ({targetX}, {targetY}).");
        }

        var sourceSize = source.PixelSize;
        if (targetX + sourceSize.Width > target.Size.Width || targetY + sourceSize.Height > target.Size.Height)
        {
            throw new ArgumentOutOfRangeException($"Bitmap exceeds target bounds at ({targetX}, {targetY}).");
        }

        var offset = checked(targetY * target.RowBytes + targetX * BytesPerPixel);
        var address = IntPtr.Add(target.Address, offset);
        var bufferSize = checked(target.RowBytes * (target.Size.Height - targetY) - targetX * BytesPerPixel);
        source.CopyPixels(new PixelRect(sourceSize), address, bufferSize, target.RowBytes);
    }

    private static RenderTargetBitmap RenderControlToBitmap(Control control, double scale)
    {
        control.Measure(Size.Infinity);
        control.Arrange(new Rect(control.DesiredSize));

        var pixelWidth = Math.Max(1, (int)Math.Ceiling(control.Bounds.Width * scale));
        var pixelHeight = Math.Max(1, (int)Math.Ceiling(control.Bounds.Height * scale));

        var bitmap = new RenderTargetBitmap(new PixelSize(pixelWidth, pixelHeight), new Vector(96 * scale, 96 * scale));
        bitmap.Render(control);
        return bitmap;
    }

    private struct RenderedSlice
    {
        public RenderTargetBitmap Bitmap;
        public int SpacingAfterPixels;
    }
}
