using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class SearchResult
    {
        [JsonPropertyName("collections")]
        public List<CollectionBrief> Collections { get; set; }
        [JsonPropertyName("stories")]
        public List<StoryBrief> Stories { get; set; }
        [JsonPropertyName("tags")]
        public List<TagBrief> Tags { get; set; }
        [JsonPropertyName("users")]
        public List<UserBrief> Users { get; set; }
        [JsonPropertyName("xusers")]
        public List<UserBriefEx> UsersEx { get; set; }
        [JsonPropertyName("search_story_uuids")]
        public UuidListInfo SearchStoryUuids { get; set; }
        [JsonPropertyName("search_tag_uuids")]
        public UuidListInfo SearchTagUuids { get; set; }
        [JsonPropertyName("search_user_uuids")]
        public UuidListInfo SearchUserUuids { get; set; }
        [JsonPropertyName("subscribed_tag_uuids")]
        public List<string> SubscribedTagUuids { get; set; }
        [JsonPropertyName("tag_info_texts")]
        public Dictionary<string, string> TagInfoTexts { get; set; }
        [JsonPropertyName("user_invite_codes")]
        public Dictionary<string, string> UserInviteCodes { get; set; }
    }
}