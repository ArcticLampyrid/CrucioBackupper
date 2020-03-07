namespace CrucioBackupper.Model
{
    public class VideoModel
    {
        public string Uuid { get; set; }
        public string CoverImageUuid { get; set; }
        public long Duration { get; set; }
        public int OriginalHeight { get; set; }
        public int OriginalWidth { get; set; }
    }
}