using System.Collections.Generic;

namespace CrucioBackupper.Crucio.Model
{
    public class StoryBrief
    {
        public string AuthorUuid { get; set; }
        public string BlurCoverUuid { get; set; }
        public string CollectionUuid { get; set; }
        public string ContentType { get; set; }
        public string CoverDominantColor { get; set; }
        public string CoverUuid { get; set; }
        public string Desc { get; set; }
        public int DialogCount { get; set; }
        public int Index { get; set; }
        public string IsLocked { get; set; }
        public string IsVideoType { get; set; }
        public string Name { get; set; }
        public int Style { get; set; }
        public string Title { get; set; }
        public string Uuid { get; set; }
        public List<string> WriterUuids { get; set; }
    }
}