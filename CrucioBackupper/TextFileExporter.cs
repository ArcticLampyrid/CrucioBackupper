﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using CrucioBackupper.Model;
using Serilog;

namespace CrucioBackupper
{
    public class TextFileExporter(ZipArchive txtPack, ZipArchive dignPack)
    {
        private readonly ILogger logger = Log.ForContext<TextFileExporter>();
        private readonly ZipArchive txtPack = txtPack;
        private readonly ZipArchive dignPack = dignPack;
        public async Task Export()
        {
            await ExportContentText();
            foreach (var entity in dignPack.Entries)
            {
                var fullName = entity.FullName.Replace('\\', '/');
                if (fullName.EndsWith('/'))
                {
                    continue;
                }
                if (fullName.StartsWith("Image/") || fullName.StartsWith("Audio/") || fullName.StartsWith("Video/"))
                {
                    using var targetStream = txtPack.CreateEntry(fullName).Open();
                    using var sourceStream = entity.Open();
                    await sourceStream.CopyToAsync(targetStream);
                }
            }
        }
        public async Task ExportContentText()
        {
            using var writer = new StreamWriter(txtPack.CreateEntry("Content.txt").Open(), Encoding.UTF8);
            CollectionModel collectionModel;
            var manifestEntry = dignPack.GetEntry("Manifest.json") ?? throw new Exception("Manifest file not found");
            using (var collectionStream = manifestEntry.Open())
            {
                collectionModel = (await JsonSerializer.DeserializeAsync<CollectionModel>(collectionStream))!;
            }
            await writer.WriteLineAsync(collectionModel.Name);
            var desc = collectionModel.Desc
                .Replace("\r\n", "[换行]")
                .Replace("\n", "[换行]")
                .Replace("\r", "[换行]");
            await writer.WriteLineAsync(desc);
            await writer.WriteLineAsync($"[封面:{collectionModel.CoverUuid}] // Exported by CrucioBackupper");
            for (int i = 0; i < collectionModel.StoryCount; i++)
            {
                StoryModel storyModel;
                try
                {
                    var entry = dignPack.GetEntry($"Story/{i + 1}.json") ?? throw new Exception("Story file not found");
                    using var storyStream = entry.Open();
                    storyModel = (await JsonSerializer.DeserializeAsync<StoryModel>(storyStream))!;
                }
                catch (Exception e)
                {
                    logger.Error(e, "无法导出章节 #{Seq}", i + 1);
                    await writer.WriteLineAsync($"#{i + 1}");
                    await writer.WriteLineAsync("右=Crucio，左=Backupper");
                    await writer.WriteLineAsync("旁白：故事文件损坏，无法导出");
                    continue;
                }
                await writer.WriteAsync("#");
                if (string.IsNullOrWhiteSpace(storyModel.Title))
                {
                    await writer.WriteLineAsync($"{i + 1}");
                }
                else
                {
                    await writer.WriteLineAsync(storyModel.Title);
                }

                var rightRole = string.Join("，", storyModel.Dialogs.Select(d => d.Character).Where(c => c.Role == 1).Select(x => x.Name).Distinct());
                var leftRole = string.Join("，", storyModel.Dialogs.Select(d => d.Character).Where(c => c.Role == 2).Select(x => x.Name).Distinct());
                await writer.WriteLineAsync($"右={rightRole}，左={leftRole}");
                foreach (var dialog in storyModel.Dialogs)
                {
                    if (dialog.Character.Role == 0)
                    {
                        await writer.WriteAsync("旁白：");
                    }
                    else
                    {
                        await writer.WriteAsync($"{dialog.Character.Name}：");
                    }
                    switch (dialog.Type)
                    {
                        case "image":
                            await writer.WriteLineAsync($"[图片:{dialog.Image?.Uuid ?? "未知"}]");
                            break;
                        case "audio":
                            await writer.WriteLineAsync($"[音频:{dialog.Audio?.Uuid ?? "未知"}]");
                            break;
                        case "video":
                            if (dialog.Video != null && dialog.Video.CoverImageUuid != null)
                            {
                                await writer.WriteLineAsync($"[视频:{dialog.Video.Uuid}, 封面={dialog.Video.CoverImageUuid}]");
                            }
                            else
                            {
                                await writer.WriteLineAsync($"[视频:{dialog.Video?.Uuid ?? "未知"}]");
                            }
                            break;
                        default:
                            var text = dialog.Text ?? ""
                                .Replace("\r\n", "[换行]")
                                .Replace("\n", "[换行]")
                                .Replace("\r", "[换行]");
                            await writer.WriteLineAsync(text);
                            break;
                    }
                }
            }
        }
    }
}
