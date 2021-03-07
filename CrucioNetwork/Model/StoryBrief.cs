using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class StoryBrief
    {
        [JsonPropertyName("author_uuid")]
        public string AuthorUuid { get; set; }
        [JsonPropertyName("blur_cover_uuid")]
        public string BlurCoverUuid { get; set; }
        [JsonPropertyName("collection_uuid")]
        public string CollectionUuid { get; set; }
        [JsonPropertyName("content_type")]
        public string ContentType { get; set; }
        [JsonPropertyName("cover_dominant_color")]
        public string CoverDominantColor { get; set; }
        [JsonPropertyName("cover_uuid")]
        public string CoverUuid { get; set; }
        [JsonPropertyName("desc")]
        public string Desc { get; set; }
        [JsonPropertyName("dialog_count")]
        public int DialogCount { get; set; }
        [JsonPropertyName("index")]
        public int Index { get; set; }
        [JsonPropertyName("is_locked")]
        public bool IsLocked { get; set; }
        [JsonPropertyName("is_video_type")]
        public bool IsVideoType { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("style")]
        public int Style { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
        [JsonPropertyName("writer_uuids")]
        public List<string> WriterUuids { get; set; }
    }
}