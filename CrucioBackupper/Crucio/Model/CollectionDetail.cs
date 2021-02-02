using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.Crucio.Model
{
    public class CollectionDetail
    {
        public List<CollectionBrief> Collections { get; set; }
        public List<StoryBrief> Stories { get; set; }
        public UuidListInfo StoryUuids { get; set; }
        public List<UserBrief> Users { get; set; }
        public int DiscussionAuthorCount { get; set; }
        public int DiscussionCount { get; set; }
        public UuidListInfo DiscussionUuids { get; set; }
        public List<DiscussionBrief> Discussions { get; set; }
    }
}
