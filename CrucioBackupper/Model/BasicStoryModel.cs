using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.Model
{
    public class BasicStoryModel
    {
        public int Seq { get; set; }
        public string Title { get; set; }
        [JsonIgnore]
        public string DisplayName => string.IsNullOrWhiteSpace(Title) ? $"第{Seq}话" : $"第{Seq}话 {Title}";
    }
}
