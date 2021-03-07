using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class VideoInfo
    {
        [JsonPropertyName("available")]
        public bool Available { get; set; }
        [JsonPropertyName("cover_image_uuid")]
        public string CoverImageUuid { get; set; }
        [JsonPropertyName("duration")]
        public long Duration { get; set; }
        [JsonPropertyName("fallback_play_url")]
        public string FallbackPlayUrl { get; set; }
        [JsonPropertyName("in_process")]
        public bool InProcess { get; set; }
        [JsonPropertyName("original_height")]
        public int OriginalHeight { get; set; }
        [JsonPropertyName("original_width")]
        public int OriginalWidth { get; set; }
        [JsonPropertyName("play_url")]
        public string PlayUrl { get; set; }
        [JsonPropertyName("share_url")]
        public string ShareUrl { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        public string GetVideoPlayUrl()
        {
            return string.IsNullOrEmpty(PlayUrl) ? FallbackPlayUrl : PlayUrl;
        }
    }
}
