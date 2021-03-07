using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class UserBriefEx
    {
        [JsonPropertyName("follower_count")]
        public int FollowerCount { get; set; }
        [JsonPropertyName("following_count")]
        public int FollowingCount { get; set; }
        [JsonPropertyName("is_follower")]
        public bool IsFollower { get; set; }
        [JsonPropertyName("is_following")]
        public bool IsFollowing { get; set; }
        [JsonPropertyName("is_in_blacklist")]
        public bool IsInBlacklist { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
    }
}