using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class RedPacketInfo
    {
        [JsonPropertyName("avatar_uuid")]
        public string AvatarUuid { get; set; }
        [JsonPropertyName("from_user_name")]
        public string FromUserName { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("used_money")]
        public int UsedMoney { get; set; }
        [JsonPropertyName("used_quantity")]
        public int UsedQuantity { get; set; }
        [JsonPropertyName("money")]
        public int Money { get; set; }
    }
}
