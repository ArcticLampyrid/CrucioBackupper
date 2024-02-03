using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.Model
{
    public class StoryModel : BasicStoryModel
    {
        public required List<DialogModel> Dialogs { get; set; }
    }
}
