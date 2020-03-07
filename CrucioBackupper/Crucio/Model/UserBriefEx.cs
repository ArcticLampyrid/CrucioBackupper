namespace CrucioBackupper.Crucio.Model
{
    public class UserBriefEx
    {
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public string IsFollower { get; set; }
        public string IsFollowing { get; set; }
        public string IsInBlacklist { get; set; }
        public string Uuid { get; set; }
    }
}