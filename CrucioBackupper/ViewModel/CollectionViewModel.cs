using CrucioNetwork;
using CrucioNetwork.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CrucioBackupper.ViewModel
{
    public class CollectionViewModel : BasicCollectionViewModel
    {
        public required string Uuid { get; set; }
        public long ClickCount { get; set; }
        public int LikeCount { get; set; }
        public string ClickCountDesc => GetNumberDesc(ClickCount);
        public string LikeCountDesc => GetNumberDesc(LikeCount);
        public bool IsVideo { get; set; } = false;
        public string ShareUuid { get; set; } = string.Empty;
        public string ShareUuidDisplay => IsVideo ? $"KV号：{ShareUuid}" : $"KN号：{ShareUuid}";
        public string Author { get; set; } = string.Empty;

        private static string GetNumberDesc(long x) => x switch
        {
            _ when x >= 1e8 => (x / 1e8).ToString("############.00亿", CultureInfo.InvariantCulture),
            _ when x >= 1e4 => (x / 1e4).ToString("####.00万", CultureInfo.InvariantCulture),
            _ => x.ToString(),
        };
    }
}
