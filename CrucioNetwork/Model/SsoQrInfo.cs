using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class SsoQrInfo
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonIgnore]
        public string ImageUrl => $"https://api.crucio.hecdn.com/v1/sso/qr_img?token={Token}";
    }
}
