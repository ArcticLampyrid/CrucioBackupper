using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using CrucioBackupper.Crucio.Model;
using System.Security.Cryptography;

namespace CrucioBackupper.Crucio
{
    public static class CrucioApi
    {
        private const string ApiDomain = "api.crucio.hecdn.com";
        private const string ImageDomain = "qc.i.hecdn.com";

        private static readonly HttpClient client = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        private static readonly JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            ContractResolver = new UnderlineSplitContractResolver()
        });

        public static T DeserializeObject<T>(Stream stream)
        {
            using var jsonTextReader = new JsonTextReader(new StreamReader(stream, Encoding.UTF8));
            return serializer.Deserialize<T>(jsonTextReader);
        }

        public static string SerializeObject(object data)
        {
            using var writer = new StringWriter();
            serializer.Serialize(writer, data);
            return writer.ToString();
        }

        #region Uid
        private static string GenerateMac(Random random)
        {
            var buffer = new byte[6];
            random.NextBytes(buffer);
            var result = string.Join(":", buffer.Select(x => string.Format("{0}", x.ToString("X2"))));
            return result;
        }
        private static string GenerateImei(Random random)
        {
            var reportingBodyIds = new string[] { "01", "10", "30", "33", "35", "44", "45", "49", "50", "51", "52", "53", "54", "86", "91", "98", "99" };
            var builder = new StringBuilder();
            builder.Append(random.Next(0, reportingBodyIds.Length));
            builder.AppendFormat("{0:000000}", random.Next(0, 1000000));
            builder.AppendFormat("{0:000000}", random.Next(0, 1000000));
            int checksum = 0;
            for (int i = 0; i < builder.Length; i += 2)
            {
                checksum += Convert.ToInt32(builder[i]);
            }
            for (int i = 1; i < builder.Length; i += 2)
            {
                var doubleValue = Convert.ToInt32(builder[i]) * 2;
                checksum += doubleValue % 10;
                checksum += doubleValue / 10;
            }
            int finalDigit = (10 - (checksum % 10)) % 10;
            builder.Append(finalDigit);
            return builder.ToString();
        }
        private static string GenerateDeviceId(Random random)
        {
            var buffer = new byte[8];
            random.NextBytes(buffer);
            var result = string.Join("", buffer.Select(x => string.Format("{0}", x.ToString("x2"))));
            return result;
        }
        private static string GenerateUid(Random random)
        {
            var nakedUid = GenerateImei(random) + ";" + GenerateMac(random) + ";" + GenerateDeviceId(random);
            var result = new byte[64];
            var paddingStart = Encoding.UTF8.GetBytes(nakedUid, 0, nakedUid.Length, result, 0);
            for (int i = paddingStart; i < result.Length; i++)
            {
                result[i] = (byte)(result.Length - paddingStart);
            }
            var encryptor = new AesCryptoServiceProvider
            {
                BlockSize = 128,
                KeySize = 256,
                Key = CrucioCodec.Key,
                IV = CrucioCodec.IV,
                Padding = PaddingMode.None,
                Mode = CipherMode.CBC
            }.CreateEncryptor();
            result = encryptor.TransformFinalBlock(result, 0, result.Length);
            return Convert.ToBase64String(result);
        }
        #endregion Uid

        static CrucioApi()
        {
            var random = new Random();
            var uid = GenerateUid(random);
            client.DefaultRequestHeaders.UserAgent.TryParseAdd($"Crucio/3.00.11.1 (Android/28;Build/HUAWEI GLK-AL00;Screen/480dpi-1080x2310;Uid/{uid}) Hybrid/-1");
        }

        public static async Task<Stream> GetData(string path)
        {
            var url = $"https://{ApiDomain}{path}";
            var responseMessage = await client.GetAsync(url);
            var result = await responseMessage.Content.ReadAsStreamAsync();
            if (responseMessage.Headers.Contains("X-Crucio-Codec") && responseMessage.Headers.GetValues("X-Crucio-Codec").First() == "1")
            {
                result = CrucioCodec.Decode(result);
            }
            return result;
        }

        public static async Task<ApiResult<SearchResult>> Search(string target)
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "q", target }
            });
            return DeserializeObject<ApiResult<SearchResult>>(await GetData("/v7/search?" + await content.ReadAsStringAsync()));
        }

        public static async Task<ApiResult<CollectionDetail>> GetCollectionDetail(string uuid)
        {
            return DeserializeObject<ApiResult<CollectionDetail>>(await GetData($"/v6/collection/{uuid}"));
        }

        public static async Task<ApiResult<StoryDetail>> GetStoryDetail(string uuid)
        {
            return DeserializeObject<ApiResult<StoryDetail>>(await GetData($"/v9/story/{uuid}/basis"));
        }

        public static async Task<ApiResult<DialogInfo>> GetDialogInfo(string uuid, int start, int end)
        {
            return DeserializeObject<ApiResult<DialogInfo>>(await GetData($"/v9/story/{uuid}/dialogs?start={start}&end={end}"));
        }

        public static async Task<ApiResult<DialogInfo>> GetAllDialogInfo(StoryBrief storyBrief)
        {
            var result = new DialogInfo()
            {
                CurrentStoryUuid = storyBrief.Uuid,
                Dialogs = new List<DialogBrief>()
            };
            var dialogInfo = await GetDialogInfo(storyBrief.Uuid, -1, -1);
            if (dialogInfo.HasError)
            {
                return dialogInfo;
            }
            result.Dialogs.AddRange(dialogInfo.Data.Dialogs);
            while (result.Dialogs.Count < storyBrief.DialogCount)
            {
                dialogInfo = await CrucioApi.GetDialogInfo(storyBrief.Uuid, result.Dialogs.Count, result.Dialogs.Count + 20);
                if (dialogInfo.HasError)
                {
                    return dialogInfo;
                }
                if (dialogInfo.Data == null || dialogInfo.Data.Dialogs == null || dialogInfo.Data.Dialogs.Count == 0)
                {
                    break;
                }
                result.Dialogs.AddRange(dialogInfo.Data.Dialogs);
            }
            return new ApiResult<DialogInfo>(result);
        }

        public static string GetImageUrl(string uuid)
        {
            return $"https://{ImageDomain}/{uuid}?x-oss-process=image/format,webp";
        }
    }
}
