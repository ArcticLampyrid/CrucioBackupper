using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.Model
{
    public class DialogModel
    {
        public CharacterModel Character { get; set; }
        public string Type { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Text { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ImageModel Image { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AudioModel Audio { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public VideoModel Video { get; set; }
    }
}
