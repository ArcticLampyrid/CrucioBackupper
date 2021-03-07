using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class DialogInfo
    {
        [JsonPropertyName("current_story_uuid")]
        public string CurrentStoryUuid { get; set; }
        [JsonPropertyName("dialogs")]
        public List<DialogBrief> Dialogs { get; set; }
    }
}
