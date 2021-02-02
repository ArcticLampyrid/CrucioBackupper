using System.Collections.Generic;

namespace CrucioBackupper.Crucio.Model
{
    public class UserBrief
    {
        public string AvatarUuid { get; set; }
        public string AvatarWidgetImageUuid { get; set; }
        public string AvatarWidgetSquareImageUuid { get; set; }
        public List<int> Badges { get; set; }
        public string Gender { get; set; }
        public bool IsEditor { get; set; }
        public bool IsOfficial { get; set; }
        public bool IsVip { get; set; }
        public string Name { get; set; }
        public string ProfileImageUuid { get; set; }
        public string Signature { get; set; }
        public string Uuid { get; set; }
    }
}