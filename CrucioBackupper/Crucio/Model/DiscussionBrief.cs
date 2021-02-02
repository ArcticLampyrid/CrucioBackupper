namespace CrucioBackupper.Crucio.Model
{
    public class DiscussionBrief
    {
        public string AuthorUuid { get; set; }
        public bool Available { get; set; }
        public int CommentCount { get; set; }
        public long CreateTime { get; set; }
        public bool Editable { get; set; }
        public int LikeCount { get; set; }
        public bool Liked { get; set; }
        public string StoryUuid { get; set; }
        public string Text { get; set; }
        public string Uuid { get; set; }
    }
}