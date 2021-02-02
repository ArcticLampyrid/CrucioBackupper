using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.Crucio.Model
{
    public class ApiResult<T>
    {
        public ApiResult()
        {
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        public ApiResult(T data) : this()
        {
            Data = data;
        }

        public int Code { get; set; }
        public T Data { get; set; }
        public string Msg { get; set; }
        public long Timestamp { get; set; }

        public override string ToString()
        {
            return CrucioApi.SerializeObject(this);
        }

        public bool HasError => Code != 0;
        public void MakeSureNoError()
        {
            if (HasError)
            {
                throw new Exception($"(Code: {Code}) {Msg}");
            }
        }
    }
}
