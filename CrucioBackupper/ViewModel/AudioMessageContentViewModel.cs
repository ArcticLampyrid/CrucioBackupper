using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.ViewModel
{
    public class AudioMessageContentViewModel
    {
        public long Duration { get; set; }
        public string DurationDesc => Duration >= 60000 ? string.Format("{0}′{1}″", Duration / 60000, (Duration % 60000) / 1000) : string.Format("{0}″", Duration / 1000);
        public required string Uuid { get; set; }
    }
}
