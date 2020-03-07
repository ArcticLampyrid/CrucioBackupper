using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.Crucio.Model
{
    public class DialogInfo
    {
        public string CurrentStoryUuid { get; set; }
        public List<DialogBrief> Dialogs { get; set; }
    }
}
