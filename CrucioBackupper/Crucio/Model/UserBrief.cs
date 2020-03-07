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
        public string IsEditor { get; set; }
        public string IsOfficial { get; set; }
        public string IsVip { get; set; }
        public string Name { get; set; }
        public string ProfileImageUuid { get; set; }
        public string Signature { get; set; }
        public string Uuid { get; set; }
    }
}