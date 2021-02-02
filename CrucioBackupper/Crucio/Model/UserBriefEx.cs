namespace CrucioBackupper.Crucio.Model
{
    public class UserBriefEx
    {
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsFollower { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsInBlacklist { get; set; }
        public string Uuid { get; set; }
    }
}