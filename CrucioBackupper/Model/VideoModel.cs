namespace CrucioBackupper.Model
{
    public class VideoModel
    {
        public required string Uuid { get; set; }
        public string CoverImageUuid { get; set; } = string.Empty;
        public long Duration { get; set; }
        public int OriginalHeight { get; set; }
        public int OriginalWidth { get; set; }
    }
}