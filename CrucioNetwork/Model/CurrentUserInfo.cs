using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class CurrentUserInfo
    {
        [JsonPropertyName("users")]
        public List<UserBrief> Users { get; set; }
    }
}
