using CrucioNetwork;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.ViewModel
{
    public class BasicCollectionViewModel
    {
        public string Name { get; set; }
        public int StoryCount { get; set; }
        public string Desc { get; set; }
        public string CoverUuid { get; set; }
        public string CoverUrl => CrucioApi.GetImageUrl(CoverUuid);
    }
}
