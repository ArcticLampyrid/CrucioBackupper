using System.Collections.Generic;

namespace CrucioBackupper.Crucio.Model
{
    public class TagBrief
    {
        public string Name { get; set; }
        public List<string> SubTagNames { get; set; }
        public string Uuid { get; set; }
    }
}