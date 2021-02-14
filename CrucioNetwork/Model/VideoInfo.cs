using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioNetwork.Model
{
    public class VideoInfo
    {
        public bool Available { get; set; }
        public string CoverImageUuid { get; set; }
        public long Duration { get; set; }
        public string FallbackPlayUrl { get; set; }
        public bool InProcess { get; set; }
        public int OriginalHeight { get; set; }
        public int OriginalWidth { get; set; }
        public string PlayUrl { get; set; }
        public string ShareUrl { get; set; }
        public string Uuid { get; set; }

        public string GetVideoPlayUrl()
        {
            return string.IsNullOrEmpty(PlayUrl) ? FallbackPlayUrl : PlayUrl;
        }
    }
}
