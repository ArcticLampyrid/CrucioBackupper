using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class DialogBrief
    {
        /// <summary>
        /// Use <c>double</c> since the server will send something like <c>123.0</c>.
        /// </summary>
        [JsonPropertyName("audio_comment_count")]
        public double AudioCommentCount { get; set; }
        [JsonPropertyName("character_uuid")]
        public string CharacterUuid { get; set; }
        /// <summary>
        /// Use <c>double</c> since the server will send something like <c>123.0</c>.
        /// </summary>
        [JsonPropertyName("comment_count")]
        public double CommentCount { get; set; }
        [JsonPropertyName("index")]
        public int Index { get; set; }
        /// <summary>
        /// Use <c>double</c> since the server will send something like <c>123.0</c>.
        /// </summary>
        [JsonPropertyName("like_count")]
        public double LikeCount { get; set; }
        [JsonPropertyName("liked")]
        public bool Liked { get; set; }
        [JsonPropertyName("show_comment_icon")]
        public bool ShowCommentIcon { get; set; }
        [JsonPropertyName("story_uuid")]
        public string StoryUuid { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
        /// <summary>
        /// Use <c>double</c> since the server will send something like <c>123.0</c>.
        /// </summary>
        [JsonPropertyName("video_comment_count")]
        public double VideoCommentCount { get; set; }

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
