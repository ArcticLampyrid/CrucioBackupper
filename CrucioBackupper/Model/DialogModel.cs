using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.Model
{
    public class DialogModel
    {
        public CharacterModel Character { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public ImageModel Image { get; set; }
        public AudioModel Audio { get; set; }
        public VideoModel Video { get; set; }
    }
}
