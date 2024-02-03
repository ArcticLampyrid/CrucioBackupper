using CrucioNetwork;
using CrucioNetwork.Model;
using CrucioBackupper.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Serilog;

namespace CrucioBackupper
{
    class CrucioDownloader
    {
        private readonly ILogger logger = Log.ForContext<CrucioDownloader>();
        private readonly CrucioApi api;
        private readonly string collectionUuid;
        private readonly ZipArchive target;

        private readonly HashSet<string> ImageSet = [];
        private readonly Dictionary<string, string> AudioMap = [];
        private readonly Dictionary<string, string> VideoMap = [];
        private static readonly JsonSerializerOptions serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        public CrucioDownloader(CrucioApi api, string collectionUuid, ZipArchive target)
        {
            this.api = api ?? throw new ArgumentNullException(nameof(api));
            this.collectionUuid = collectionUuid ?? throw new ArgumentNullException(nameof(collectionUuid));
            this.target = target ?? throw new ArgumentNullException(nameof(target));
        }

        private async Task WriteStory(StoryModel model)
        {
            using var stream = target.CreateEntry($"Story/{model.Seq}.json").Open();
            await JsonSerializer.SerializeAsync(stream, model, serializerOptions);
        }

        private async Task WriteCollection(CollectionModel model)
        {
            using var stream = target.CreateEntry("Manifest.json").Open();
            await JsonSerializer.SerializeAsync(stream, model, serializerOptions);
        }

        private record ResourceInfo(string Type, string Uuid, string Ext, string Url)
        {
            public string Type { get; } = Type;
            public string Uuid { get; } = Uuid;
            public string Ext { get; } = Ext;
            public string Url { get; } = Url;
        }

        private async Task CopyResourceToMemoryStream(ResourceInfo resource, MemoryStream memoryStream)
        {
            memoryStream.SetLength(0);
            using var networkStream = await api.HttpRequest(new HttpRequestMessage(HttpMethod.Get, resource.Url));
            await networkStream.CopyToAsync(memoryStream);
        }

        private async Task DownloadResource()
        {
            var resources = ImageSet.Select(x => new ResourceInfo("Image", x, "webp", CrucioApi.GetImageUrl(x)))
                .Concat(AudioMap.Select(x => new ResourceInfo("Audio", x.Key, "m4a", x.Value)))
                .Concat(VideoMap.Select(x => new ResourceInfo("Video", x.Key, "mp4", x.Value)))
                .ToList();

            const int maxParallel = 10;
            var cacheStream = new MemoryStream[maxParallel];
            var tasks = new Task?[maxParallel];
            for (int i = 0; i < maxParallel; i++)
            {
                cacheStream[i] = new MemoryStream();
            }
            try
            {
                for (int i = 0; i < resources.Count; i += maxParallel)
                {
                    int maxTask = Math.Min(maxParallel, resources.Count - i);
                    for (int j = 0; j < maxTask; j++)
                    {
                        tasks[j] = CopyResourceToMemoryStream(resources[i + j], cacheStream[j]);
                    }
                    for (int j = 0; j < maxTask; j++)
                    {
                        if ((i + j) % Math.Max(resources.Count / 5, 1) == 0)
                        {
                            logger.Information("正在下载资源 {Start} / {Total}", i + 1, resources.Count);
                        }
                        try
                        {
                            await tasks[j]!;
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "无法下载资源 {Type}/{Uuid}", resources[i + j].Type, resources[i + j].Uuid);
                            continue;
                        }
                        tasks[j] = null;
                        var resource = resources[i + j];
                        using var targetStream = target.CreateEntry($"{resource.Type}/{resource.Uuid}.{resource.Ext}").Open();
                        cacheStream[j].Seek(0, SeekOrigin.Begin);
                        await cacheStream[j].CopyToAsync(targetStream);
                    }
                }
            }
            finally
            {
                for (int j = 0; j < maxParallel; j++)
                {
                    cacheStream[j].Dispose();
                }
            }
        }

        public async Task Download()
        {
            var collectionDetail = await api.GetCollectionDetail(collectionUuid);
            var stories = collectionDetail.Data.Stories
                .Where(x => x.CollectionUuid == collectionUuid)
                .OrderBy(x => x.Index)
                .ToList();
            string? coverUuid = null;
            var storiesDetail = stories.Select(x => api.GetStoryDetail(x.Uuid)).ToList();
            var dealogInfos = stories.Select(x => api.GetAllDialog(x)).ToList();
            for (int i = 0; i < stories.Count; i++)
            {
                StoryBrief storyBrief = stories[i];
                coverUuid ??= storyBrief.CoverUuid;
                try
                {
                    var storyDetail = await storiesDetail[i];
                    var dialogInfo = await dealogInfos[i];
                    storyDetail.MakeSureNoError();
                    dialogInfo.MakeSureNoError();

                    var characterMap = storyDetail.Data.StoriesEx[0].Characters.DistinctBy(x => x.Uuid).ToDictionary(
                        x => x.Uuid,
                        x => new CharacterModel()
                        {
                            AvatarUuid = x.AvatarUuid,
                            Name = x.Name,
                            Role = x.Role
                        });
                    foreach (var x in characterMap.Values)
                    {
                        ImageSet.Add(x.AvatarUuid);
                    }
                    var storyModel = new StoryModel()
                    {
                        Seq = storyBrief.Index + 1,
                        Title = storyBrief.Title,
                        Dialogs = dialogInfo.Data.Dialogs.Select(x =>
                        {
                            var result = new DialogModel()
                            {
                                Type = x.Type,
                                Text = x.Text,
                                Character = characterMap[x.CharacterUuid]
                            };
                            if (x.Image != null)
                            {
                                result.Image = new ImageModel()
                                {
                                    Height = x.Image.Height,
                                    Width = x.Image.Width,
                                    Uuid = x.Image.Uuid
                                };
                                ImageSet.Add(x.Image.Uuid);
                            }
                            if (x.Audio != null)
                            {
                                var uuid = x.Audio.Uuid;
                                if (string.IsNullOrEmpty(uuid))
                                {
                                    uuid = new Uri(x.Audio.Url).AbsolutePath.Substring(1);
                                }
                                result.Audio = new AudioModel()
                                {
                                    Duration = x.Audio.Duration,
                                    Uuid = uuid
                                };
                                AudioMap.TryAdd(uuid, x.Audio.Url);
                            }
                            if (x.Video != null)
                            {
                                var uuid = x.Video.Uuid;
                                result.Video = new VideoModel()
                                {
                                    Duration = x.Video.Duration,
                                    OriginalHeight = x.Video.OriginalHeight,
                                    OriginalWidth = x.Video.OriginalWidth,
                                    CoverImageUuid = x.Video.CoverImageUuid,
                                    Uuid = uuid
                                };
                                VideoMap.TryAdd(uuid, x.Video.GetVideoPlayUrl());
                                if (!string.IsNullOrEmpty(x.Video.CoverImageUuid))
                                {
                                    ImageSet.Add(x.Video.CoverImageUuid);
                                }
                            }
                            return result;
                        }).ToList()
                    };
                    await WriteStory(storyModel);
                    logger.Information("下载章节 #{Seq}({Uuid}) 成功", storyBrief.Index + 1, storyBrief.Uuid);
                }
                catch (Exception e)
                {
                    logger.Error(e, "无法下载章节 #{Seq}({Uuid})", storyBrief.Index + 1, storyBrief.Uuid);
                    continue;
                }
            }
            var collectionModel = new CollectionModel()
            {
                Name = collectionDetail.Data.Collections[0].Name,
                Desc = collectionDetail.Data.Collections[0].Desc,
                StoryCount = Math.Max(collectionDetail.Data.Collections[0].StoryCount, stories.Count),
                CoverUuid = coverUuid ?? string.Empty,
                Stories = stories.Select(x => new BasicStoryModel()
                {
                    Seq = x.Index + 1,
                    Title = x.Title
                }).ToList()
            };
            if (!string.IsNullOrEmpty(coverUuid))
            {
                ImageSet.Add(coverUuid);
            }
            await WriteCollection(collectionModel);
            await DownloadResource();
        }
    }
}
