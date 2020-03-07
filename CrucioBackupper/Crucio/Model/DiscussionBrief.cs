namespace CrucioBackupper.Crucio.Model
{
    public class DiscussionBrief
    {
        public string AuthorUuid { get; set; }
        public string Available { get; set; }
        public int CommentCount { get; set; }
        public long CreateTime { get; set; }
        public string Editable { get; set; }
        public int LikeCount { get; set; }
        public string Liked { get; set; }
        public string StoryUuid { get; set; }
        public string Text { get; set; }
        public string Uuid { get; set; }
    }
}