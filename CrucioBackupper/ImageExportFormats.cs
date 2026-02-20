using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NeoSolve.ImageSharp.AVIF;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace CrucioBackupper;

public interface IImageExportFormat
{
    string DisplayName { get; }
    string Extension { get; }
    Task SaveAsync(Bitmap source, Stream targetStream);
}

public static class ImageExportFormats
{
    public static IImageExportFormat Png { get; } = new ImageSharpImageExportFormat("PNG", "png", static () => new PngEncoder());
    public static IImageExportFormat AvifCQLevel18 { get; } = new ImageSharpImageExportFormat("AVIF (CQ Level 18)", "avif", static () => new AVIFEncoder { CQLevel = 18, Lossless = false });
    public static IImageExportFormat JpegQuality80 { get; } = new ImageSharpImageExportFormat("JPEG (Quality 80)", "jpg", static () => new JpegEncoder { Quality = 80 });
    public static IImageExportFormat Bmp { get; } = new ImageSharpImageExportFormat("BMP (Uncompressed)", "bmp", static () => new BmpEncoder());

    public static IReadOnlyList<IImageExportFormat> All { get; } = [Png, AvifCQLevel18, JpegQuality80, Bmp];
    public static IReadOnlyList<string> DisplayNames { get; } = [.. All.Select(f => f.DisplayName)];
    private static readonly IReadOnlyDictionary<string, IImageExportFormat> formatMap = All.ToDictionary(f => f.DisplayName, f => f, StringComparer.Ordinal);

    public static bool TryGetByDisplayName(string displayName, out IImageExportFormat format)
    {
        ArgumentNullException.ThrowIfNull(displayName);

        if (formatMap.TryGetValue(displayName, out var mappedFormat))
        {
            format = mappedFormat;
            return true;
        }

        format = Png;
        return false;
    }
}

internal sealed class ImageSharpImageExportFormat : IImageExportFormat
{
    private const int BytesPerPixel = 4;
    private readonly Func<IImageEncoder> encoderFactory;

    public ImageSharpImageExportFormat(string displayName, string extension, Func<IImageEncoder> encoderFactory)
    {
        DisplayName = !string.IsNullOrWhiteSpace(displayName)
            ? displayName
            : throw new ArgumentException("Display name cannot be null or empty.", nameof(displayName));
        Extension = !string.IsNullOrWhiteSpace(extension)
            ? extension
            : throw new ArgumentException("Extension cannot be null or empty.", nameof(extension));
        this.encoderFactory = encoderFactory ?? throw new ArgumentNullException(nameof(encoderFactory));
    }

    public string DisplayName { get; }
    public string Extension { get; }

    public async Task SaveAsync(Bitmap source, Stream targetStream)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(targetStream);

        var encoder = encoderFactory();
        var sourceSize = source.PixelSize;
        var stride = checked(sourceSize.Width * BytesPerPixel);
        var bufferSize = checked(stride * sourceSize.Height);
        var pixelBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            using var framebuffer = new ArrayLockedFramebuffer(pixelBuffer, sourceSize, source.Dpi, stride);
            source.CopyPixels(framebuffer, AlphaFormat.Unpremul);
            await SavePixelBufferAsImageSharp(source, targetStream, encoder, pixelBuffer, bufferSize);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(pixelBuffer);
        }
    }

    private static async Task SavePixelBufferAsImageSharp(Bitmap source, Stream targetStream, IImageEncoder encoder, byte[] pixelBuffer, int bufferSize)
    {
        using var image = ImageSharpImage.LoadPixelData<Bgra32>(pixelBuffer.AsSpan(0, bufferSize), source.PixelSize.Width, source.PixelSize.Height);
        image.Metadata.HorizontalResolution = source.Dpi.X;
        image.Metadata.VerticalResolution = source.Dpi.Y;
        await image.SaveAsync(targetStream, encoder);
    }

    private sealed class ArrayLockedFramebuffer : ILockedFramebuffer
    {
        private readonly GCHandle pinHandle;

        public ArrayLockedFramebuffer(byte[] buffer, PixelSize size, Vector dpi, int rowBytes)
        {
            pinHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Size = size;
            Dpi = dpi;
            RowBytes = rowBytes;
        }

        public IntPtr Address => pinHandle.AddrOfPinnedObject();
        public PixelSize Size { get; }
        public int RowBytes { get; }
        public Vector Dpi { get; }
        public PixelFormat Format => PixelFormats.Bgra8888;

        public void Dispose()
        {
            if (pinHandle.IsAllocated)
            {
                pinHandle.Free();
            }
        }
    }
}
