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

namespace CrucioBackupper
{
    class CrucioDownloader
    {
        private readonly CrucioApi api;
        private readonly string collectionUuid;
        private readonly ZipArchive target;

        private HashSet<string> ImageSet = new HashSet<string>();
        private Dictionary<string, string> AudioMap = new Dictionary<string, string>();
        private Dictionary<string, string> VideoMap = new Dictionary<string, string>();
        private static JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
        {
            IgnoreNullValues = true,
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

        private async Task DownloadResource(string type, string uuid, string ext, string url)
        {
            using var targetStream = target.CreateEntry($"{type}/{uuid}.{ext}", CompressionLevel.NoCompression).Open();
            using var networkStream = await api.HttpRequest(new HttpRequestMessage(HttpMethod.Get, url));
            await networkStream.CopyToAsync(targetStream);
        }

        private async Task DownloadResource()
        {
            var tasks = ImageSet.Select(x => DownloadResource("Image", x, "webp", CrucioApi.GetImageUrl(x)))
                .Concat(AudioMap.Select(x => DownloadResource("Audio", x.Key, "m4a", x.Value)))
                .Concat(VideoMap.Select(x => DownloadResource("Video", x.Key, "mp4", x.Value)));
            foreach (var item in tasks)
            {
                try
                {
                    await item;
                }
                catch (Exception)
                {
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
            string coverUuid = null;
            var storiesDetail = stories.Select(x => api.GetStoryDetail(x.Uuid)).ToList();
            var dealogInfos = stories.Select(x => api.GetAllDialog(x)).ToList();
            for (int i = 0; i < stories.Count; i++)
            {
                StoryBrief storyBrief = stories[i];
                if (coverUuid == null)
                {
                    coverUuid = storyBrief.CoverUuid;
                }
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
                        Dialogs = dialogInfo.Data.Dialogs.Select(x => {
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
                                if(string.IsNullOrEmpty(uuid))
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
                                if(!string.IsNullOrEmpty(x.Video.CoverImageUuid))
                                {
                                    ImageSet.Add(x.Video.CoverImageUuid);
                                }
                            }
                            return result;
                        }).ToList()
                    };
                    await WriteStory(storyModel);
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Failed to download story #{storyBrief.Index}({storyBrief.Uuid}): {e}");
                    throw;
                }
            }
            var collectionModel = new CollectionModel()
            {
                Name = collectionDetail.Data.Collections[0].Name,
                Desc = collectionDetail.Data.Collections[0].Desc,
                StoryCount = Math.Max(collectionDetail.Data.Collections[0].StoryCount, stories.Count),
                CoverUuid = coverUuid,
                Stories = stories.Select(x => new BasicStoryModel()
                {
                    Seq = x.Index + 1,
                    Title = x.Title
                }).ToList()
            };
            ImageSet.Add(coverUuid);
            await WriteCollection(collectionModel);
            await DownloadResource();
        }
    }
}
