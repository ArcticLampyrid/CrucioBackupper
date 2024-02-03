using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.Model
{
    public class CollectionModel
    {
        public string Name { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public string CoverUuid { get; set; } = string.Empty;
        public int StoryCount { get; set; }
        public required List<BasicStoryModel> Stories { get; set; }
    }
}
