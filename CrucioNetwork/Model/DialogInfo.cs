using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioNetwork.Model
{
    public class DialogInfo
    {
        public string CurrentStoryUuid { get; set; }
        public List<DialogBrief> Dialogs { get; set; }
    }
}
