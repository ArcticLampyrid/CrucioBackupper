using Avalonia.Media.Imaging;
using BernhardHaus.Collections.WeakDictionary;
using System;
using System.IO;

namespace CrucioBackupper;

public sealed class LocalFileImageLoader : IDisposable
{
    private readonly WeakDictionary<string, Bitmap> cache = [];

    public Bitmap? Load(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        if (cache.TryGetValue(path, out var bitmap) && bitmap is not null)
        {
            return bitmap;
        }

        using var stream = File.OpenRead(path);
        bitmap = new Bitmap(stream);
        cache[path] = bitmap;
        return bitmap;
    }

    public void Dispose()
    {
        foreach (var bitmap in cache.Values)
        {
            bitmap?.Dispose();
        }

        cache.Clear();
    }
}
