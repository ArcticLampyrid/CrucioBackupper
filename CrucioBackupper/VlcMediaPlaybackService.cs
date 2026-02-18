using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CrucioBackupper;

internal sealed class VlcMediaPlaybackService : IDisposable
{
    private static readonly object VlcInitLock = new();
    private static bool isVlcInitialized;

    private readonly object sessionsLock = new();
    private readonly HashSet<VlcPlaybackSession> activeSessions = [];
    private bool isDisposed;

    public void Play(string path)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("媒体文件不存在", path);
        }

        var session = VlcPlaybackSession.Start(path);

        lock (sessionsLock)
        {
            if (isDisposed)
            {
                session.Dispose();
                throw new ObjectDisposedException(nameof(VlcMediaPlaybackService));
            }

            activeSessions.Add(session);
        }

        _ = session.Completion.ContinueWith(
            _ => RemoveAndDispose(session),
            TaskScheduler.Default);
    }

    public void Dispose()
    {
        VlcPlaybackSession[] sessionsToDispose;

        lock (sessionsLock)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            sessionsToDispose = [.. activeSessions];
            activeSessions.Clear();
        }

        foreach (var session in sessionsToDispose)
        {
            session.Dispose();
        }
    }

    private void RemoveAndDispose(VlcPlaybackSession session)
    {
        var shouldDispose = false;

        lock (sessionsLock)
        {
            if (activeSessions.Remove(session))
            {
                shouldDispose = true;
            }
        }

        if (shouldDispose)
        {
            session.Dispose();
        }
    }

    private static void EnsureVlcInitialized()
    {
        lock (VlcInitLock)
        {
            if (isVlcInitialized)
            {
                return;
            }

            Core.Initialize();
            isVlcInitialized = true;
        }
    }

    private sealed class VlcPlaybackSession : IDisposable
    {
        private readonly LibVLC libVlc;
        private readonly LibVLCSharp.Shared.MediaPlayer player;
        private readonly Media media;
        private readonly TaskCompletionSource<bool> completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private bool isDisposed;

        public Task Completion => completionSource.Task;

        private VlcPlaybackSession(string path)
        {
            libVlc = new LibVLC("--quiet");
            player = new LibVLCSharp.Shared.MediaPlayer(libVlc);
            media = new Media(libVlc, new Uri(path));

            player.EndReached += OnPlaybackFinished;
            player.EncounteredError += OnPlaybackFinished;
            player.Stopped += OnPlaybackFinished;
        }

        public static VlcPlaybackSession Start(string path)
        {
            EnsureVlcInitialized();

            var session = new VlcPlaybackSession(path);
            if (!session.player.Play(session.media))
            {
                session.Dispose();
                throw new InvalidOperationException("VLC 无法启动媒体播放。");
            }

            return session;
        }

        private void OnPlaybackFinished(object? sender, EventArgs e)
        {
            completionSource.TrySetResult(true);
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            player.EndReached -= OnPlaybackFinished;
            player.EncounteredError -= OnPlaybackFinished;
            player.Stopped -= OnPlaybackFinished;

            try
            {
                if (player.IsPlaying)
                {
                    player.Stop();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Failed to stop VLC player during dispose: {0}", ex.Message);
            }

            media.Dispose();
            player.Dispose();
            libVlc.Dispose();
            completionSource.TrySetResult(true);
        }
    }
}
