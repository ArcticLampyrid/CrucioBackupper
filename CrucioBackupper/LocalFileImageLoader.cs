using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrucioBackupper;

public sealed class LocalFileImageLoader : IDisposable
{
    private readonly Dictionary<string, Bitmap?> cache = new(StringComparer.Ordinal);

    public Bitmap? Load(string path)
    {
        if (cache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        if (!File.Exists(path))
        {
            cache[path] = null;
            return null;
        }

        using var stream = File.OpenRead(path);
        var bitmap = new Bitmap(stream);
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
