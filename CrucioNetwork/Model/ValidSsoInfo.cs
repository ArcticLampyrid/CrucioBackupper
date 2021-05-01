using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class ValidSsoInfo
    {
        [JsonPropertyName("next_token")]
        public string NextToken { get; set; }

        [JsonPropertyName("user")]
        public EditorUserInfo User { get; set; }
    }
}
