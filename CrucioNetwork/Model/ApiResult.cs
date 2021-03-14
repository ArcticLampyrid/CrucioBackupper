using CrucioNetwork.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class ApiResult<T>
    {
        public ApiResult()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }
        public ApiResult(T data) : this()
        {
            Data = data;
        }

        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("data")]
        public T Data { get; set; }
        [JsonPropertyName("msg")]
        public string Msg { get; set; }
        [JsonPropertyName("timestamp")]
        [JsonConverter(typeof(JsonUnixTimeMillisecondsConverter))]
        public DateTimeOffset Timestamp { get; set; }

        public override string ToString()
        {
            return CrucioApi.SerializeObject(this);
        }

        [JsonIgnore]
        public bool HasError => Code != 0;
        public void MakeSureNoError()
        {
            if (HasError)
            {
                throw new Exception($"(Code: {Code}) {Msg}");
            }
        }

        public T GetDataOrFail()
        {
            MakeSureNoError();
            return Data;
        }
    }
}
