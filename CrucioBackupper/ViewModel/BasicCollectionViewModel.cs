﻿using CrucioNetwork;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.ViewModel
{
    public class BasicCollectionViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int StoryCount { get; set; }
        public string Desc { get; set; } = string.Empty;
        public string CoverUuid { get; set; } = string.Empty;
        private string? _coverUrl;
        public string CoverUrl => _coverUrl ?? CrucioApi.GetImageUrl(CoverUuid);
        public void UseCustomCoverUrl(string coverUrl)
        {
            _coverUrl = coverUrl;
        }
    }
}
