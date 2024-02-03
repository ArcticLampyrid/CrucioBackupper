using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace CrucioBackupper.ViewModel
{
    public class ChatMessageViewModel
    {
        public required string AvatarPath { get; set; }
        public required string CharacterName { get; set; }
        public required FrameworkElement Content { get; set; }
    }
}
