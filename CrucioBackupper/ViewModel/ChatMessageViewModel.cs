using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace CrucioBackupper.ViewModel
{
    public class ChatMessageViewModel
    {
        public string AvatarPath { get; set; }
        public string CharacterName { get; set; }
        public FrameworkElement Content { get; set; }
    }
}
