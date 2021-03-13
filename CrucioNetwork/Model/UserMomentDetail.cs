﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class UserMomentDetail : BaseMomentDetail
    {
        [JsonPropertyName("user_moment_uuids")]
        public UuidListInfo UserMomentUuids { get; set; }
    }
}
