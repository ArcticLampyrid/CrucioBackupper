using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.Crucio.Model
{
    public class DialogBrief
    {
        public int AudioCommentCount { get; set; }
        public string CharacterUuid { get; set; }
        public int CommentCount { get; set; }
        public int Index { get; set; }
        public int LikeCount { get; set; }
        public bool Liked { get; set; }
        public bool ShowCommentIcon { get; set; }
        public string StoryUuid { get; set; }
        public string Uuid { get; set; }
        public int VideoCommentCount { get; set; }

        public string Type { get; set; }
        public string Text { get; set; }
        public ImageInfo Image { get; set; }
        public AudioInfo Audio { get; set; }
        public VideoInfo Video { get; set; }
    }
}
