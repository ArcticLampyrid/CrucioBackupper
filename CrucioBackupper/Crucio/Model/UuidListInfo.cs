using System.Collections.Generic;

namespace CrucioBackupper.Crucio.Model
{
    public class UuidListInfo
    {
        public string Cursor { get; set; }
        public bool Hasmore { get; set; }
        public List<string> List { get; set; }
    }
}