using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.Model
{
    public class CollectionModel
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public string CoverUuid { get; set; }
        public int StoryCount { get; set; }
        public List<BasicStoryModel> Stories { get; set; }
    }
}
