using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class DialogBrief
    {
        [JsonPropertyName("audio_comment_count")]
        public int AudioCommentCount { get; set; }
        [JsonPropertyName("character_uuid")]
        public string CharacterUuid { get; set; }
        [JsonPropertyName("comment_count")]
        public int CommentCount { get; set; }
        [JsonPropertyName("index")]
        public int Index { get; set; }
        [JsonPropertyName("like_count")]
        public int LikeCount { get; set; }
        [JsonPropertyName("liked")]
        public bool Liked { get; set; }
        [JsonPropertyName("show_comment_icon")]
        public bool ShowCommentIcon { get; set; }
        [JsonPropertyName("story_uuid")]
        public string StoryUuid { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
        [JsonPropertyName("video_comment_count")]
        public int VideoCommentCount { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("image")]
        public ImageInfo Image { get; set; }
        [JsonPropertyName("audio")]
        public AudioInfo Audio { get; set; }
        [JsonPropertyName("video")]
        public VideoInfo Video { get; set; }
        [JsonPropertyName("red_packet")]
        public RedPacketInfo RedPacket { get; set; }
        [JsonPropertyName("audio_clip")]
        public AudioClipInfo AudioClip { get; set; }
        [JsonPropertyName("video_clip")]
        public VideoClipInfo VideoClip { get; set; }
    }
}
