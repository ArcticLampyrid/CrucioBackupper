using System.Collections.Generic;

namespace CrucioNetwork.Model
{
    public class CollectionBrief
    {
        public long ClickCount { get; set; }
        public int CommentCount { get; set; }
        public string Desc { get; set; }
        public bool IsSubscribed { get; set; }
        public int LikeCount { get; set; }
        public string Name { get; set; }
        public string OriginalStatement { get; set; }
        public int RemakeCount { get; set; }
        public string ShareUuid { get; set; }
        public bool ShowMetadata { get; set; }
        public int StoryCount { get; set; }
        public List<string> TagNames { get; set; }
        public bool ToBeContinued { get; set; }
        public string Uuid { get; set; }
    }
}