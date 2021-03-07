using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class UserBrief
    {
        [JsonPropertyName("avatar_uuid")]
        public string AvatarUuid { get; set; }
        [JsonPropertyName("avatar_widget_image_uuid")]
        public string AvatarWidgetImageUuid { get; set; }
        [JsonPropertyName("avatar_widget_square_image_uuid")]
        public string AvatarWidgetSquareImageUuid { get; set; }
        [JsonPropertyName("badges")]
        public List<int> Badges { get; set; }
        [JsonPropertyName("gender")]
        public string Gender { get; set; }
        [JsonPropertyName("is_editor")]
        public bool IsEditor { get; set; }
        [JsonPropertyName("is_official")]
        public bool IsOfficial { get; set; }
        [JsonPropertyName("is_vip")]
        public bool IsVip { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("profile_image_uuid")]
        public string ProfileImageUuid { get; set; }
        [JsonPropertyName("signature")]
        public string Signature { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
    }
}