using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class EditorUserInfo
    {
        [JsonPropertyName("author_type")]
        public int AuthorType { get; set; }

        [JsonPropertyName("author_type_text")]
        public string AuthorTypeText { get; set; }

        [JsonPropertyName("avatar_uuid")]
        public string AvatarUuid { get; set; }

        [JsonPropertyName("is_vip")]
        public bool IsVip { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("show_author_bill")]
        public bool ShowAuthorBill { get; set; }

        [JsonPropertyName("show_certify")]
        public bool ShowCertify { get; set; }

        [JsonPropertyName("user_type")]
        public int UserType { get; set; }

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
    }
}
