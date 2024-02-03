using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.Model
{
    public class DialogModel
    {
        public required CharacterModel Character { get; set; }
        public required string Type { get; set; }
        public string? Text { get; set; }
        public ImageModel? Image { get; set; }
        public AudioModel? Audio { get; set; }
        public VideoModel? Video { get; set; }
    }
}
