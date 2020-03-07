using System.Collections.Generic;

namespace CrucioBackupper.Crucio.Model
{
    public class StoryBriefEx
    {
        public string Uuid { get; set; }
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }
        public int LikeStatus { get; set; }
        public List<CharacterBrief> Characters { get; set; }
    }
}