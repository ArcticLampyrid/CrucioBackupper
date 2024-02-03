using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using CrucioNetwork.Model;
using System.Security.Cryptography;
using System.Web;
using System.Text.Json;
using CrucioNetwork.Utils;

namespace CrucioNetwork
{
    public class CrucioApi
    {
        public static CrucioApi Default { get; } = new CrucioApi();

        private const string ApiDomain = "api.crucio.hecdn.com";
        private const string ImageDomain = "ve.i.hecdn.com";
        private const string EmptyContentSHA256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
        private static readonly byte[] NormalSignKey = Encoding.UTF8.GetBytes("0e666341456764fcb3184767adcf2884");
        private static readonly byte[] ApiSignKey = Encoding.UTF8.GetBytes("ae2ce93f486b638849f6f438c3b38d3a");

        private readonly CookieContainer cookieContainer = new CookieContainer();
        private readonly HttpClient client;

        public CrucioApi(string uid = null)
        {
            if (string.IsNullOrEmpty(uid))
            {
                uid = GenerateUid(new Random());
            }
            client = new HttpClient(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = cookieContainer
            });
            client.DefaultRequestHeaders.UserAgent.TryParseAdd($"Crucio/4.01.03.1 (Android/28;Build/HUAWEI GLK-AL00;Screen/480dpi-1080x2310;Uid/{uid}) Hybrid/-1");
        }

        public void SetToken(string token)
        {
            var cookie = new Cookie("token", token ?? "", "/", ApiDomain);
            if (string.IsNullOrEmpty(token))
            {
                cookie.Expired = true;
            }
            cookieContainer.Add(cookie);
        }

        public string GetToken()
        {
            return cookieContainer.GetCookies(new Uri($"https://{ApiDomain}"))["token"]?.Value;
        }

        private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
        {
            IgnoreNullValues = true
        };

        internal static async Task<T> DeserializeObject<T>(Stream stream)
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, serializerOptions);
        }

        internal static string SerializeObject(object data)
        {
            return JsonSerializer.Serialize(data, serializerOptions);
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
            builder.Append(reportingBodyIds[random.Next(reportingBodyIds.Length)]);
            builder.AppendFormat("{0:000000}", random.Next(0, 1000000));
            builder.AppendFormat("{0:000000}", random.Next(0, 1000000));
            int checksum = 0;
            for (int i = 0; i < builder.Length; i += 2)
            {
                checksum += builder[i] - '0';
                var doubleValue = (builder[i + 1] - '0') * 2;
                checksum += doubleValue % 10;
                checksum += doubleValue / 10;
            }
            int finalDigit = (10 - (checksum % 10)) % 10;
            builder.Append((char)(finalDigit + '0'));
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

        public async Task<T> ApiGet<T>(string path)
        {
            using (var stream = await ApiGetStream(path))
            {
                return await JsonSerializer.DeserializeAsync<T>(stream, serializerOptions);
            }
        }

        public async Task<Stream> ApiGetStream(string path)
        {
            var url = $"https://{ApiDomain}{path}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            return await HttpRequest(request);
        }

        public async Task<Stream> HttpRequest(HttpRequestMessage request)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var queryList = HttpUtility.ParseQueryString(request.RequestUri.Query);
            var queryInfo = string.Join("&", queryList.AllKeys.OrderBy(x => x).Select(x => $"{Uri.EscapeDataString(x)}={Uri.EscapeDataString(queryList[x]).Replace("%3D", "%253D")}"));
            var requestBodySHA256 = request.Content != null
                ? HexConvert.ToStringLC(SHA256.Create().ComputeHash(await request.Content.ReadAsStreamAsync()))
                : EmptyContentSHA256;
            var requestInfo = $"{request.Method.Method}\n{request.RequestUri.AbsolutePath}\n{queryInfo}\nx-crucio-timestamp:{timestamp}\n{requestBodySHA256}";
            var requestInfoSHA256 = HexConvert.ToStringLC(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(requestInfo)));
            var signInfo = $"KD1\n{timestamp}\nb975487c3cc1867a9a0bd87a63aae258ac776a06\n{requestInfoSHA256}";
            var hmacKey = request.RequestUri.Host == ApiDomain ? ApiSignKey : NormalSignKey;
            var sign = HexConvert.ToStringLC(new HMACSHA256(hmacKey).ComputeHash(Encoding.UTF8.GetBytes(signInfo)));
            request.Headers.Add("X-Crucio-Timestamp", timestamp.ToString());
            request.Headers.Add("Authorization", $"KD1 Credential=10001, Signature={sign}");

            var responseMessage = await client.SendAsync(request);
            var result = await responseMessage.Content.ReadAsStreamAsync();
            if (responseMessage.Headers.Contains("X-Crucio-Codec") && responseMessage.Headers.GetValues("X-Crucio-Codec").First() == "1")
            {
                result = CrucioCodec.Decode(result);
            }
            return result;
        }

        public async Task<ApiResult<SearchResult>> Search(string target)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "q", target }
            });
            return await ApiGet<ApiResult<SearchResult>>("/v7/search?" + await content.ReadAsStringAsync());
        }

        public async Task<ApiResult<CollectionDetail>> GetCollectionDetail(string uuid)
        {
            return await ApiGet<ApiResult<CollectionDetail>>($"/v6/collection/{uuid}");
        }

        public async Task<ApiResult<StoryDetail>> GetStoryDetail(string uuid)
        {
            return await ApiGet<ApiResult<StoryDetail>>($"/v10/story/{uuid}/basis");
        }

        public async Task<ApiResult<DialogInfo>> GetDialogFragment(string uuid, int start, int end)
        {
            return await ApiGet<ApiResult<DialogInfo>>($"/v10/story/{uuid}/dialogs?start={start}&end={end}");
        }

        public async Task<ApiResult<DialogInfo>> GetAllDialog(StoryBrief storyBrief)
        {
            var result = new DialogInfo()
            {
                CurrentStoryUuid = storyBrief.Uuid,
                Dialogs = new List<DialogBrief>()
            };
            var dialogInfo = await GetDialogFragment(storyBrief.Uuid, 0, 20);
            if (dialogInfo.HasError)
            {
                return dialogInfo;
            }
            result.Dialogs.AddRange(dialogInfo.Data.Dialogs);
            while (result.Dialogs.Count < storyBrief.DialogCount)
            {
                dialogInfo = await GetDialogFragment(storyBrief.Uuid, result.Dialogs.Count, result.Dialogs.Count + 20);
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

        public async Task<ApiResult<UserMomentDetail>> GetUserMomentFragment(string uuid, string cursor = null)
        {
            return await ApiGet<ApiResult<UserMomentDetail>>($"/v6/profile/{uuid}/moments" + (string.IsNullOrEmpty(cursor) ? "" : $"?cursor={cursor}"));
        }

        public async Task<ApiResult<UserStoryDetail>> GetUserStoryFragment(string uuid, string cursor = null)
        {
            return await ApiGet<ApiResult<UserStoryDetail>>($"/v6/profile/{uuid}/stories" + (string.IsNullOrEmpty(cursor) ? "" : $"?cursor={cursor}"));
        }

        public async Task<ApiResult<SsoQrInfo>> RequireSsoQrInfo()
        {
            var body = await client.GetStringAsync("https://editor.kuaidianyuedu.com/api/v1/sso/qr_info");
            return JsonSerializer.Deserialize<ApiResult<SsoQrInfo>>(body, serializerOptions);
        }

        public async Task<ApiResult<CurrentUserInfo>> GetCurrentUserInfo()
        {
            return await ApiGet<ApiResult<CurrentUserInfo>>("/v12/user");
        }

        public async Task<(ApiResult<ValidSsoInfo>, string)> ValidSsoQrInfo(SsoQrInfo qrInfo)
        {
            var param = $"{{\"token\":\"{qrInfo.Token}\"}}";
            var content = new StringContent(param, Encoding.UTF8, "application/json");
            using (var response = await client.PostAsync("https://editor.kuaidianyuedu.com/api/v1/sso/loop_valid", content))
            {
                var result = JsonSerializer.Deserialize<ApiResult<ValidSsoInfo>>(await response.Content.ReadAsStringAsync(), serializerOptions);
                if (!result.HasError)
                {
                    return (result, cookieContainer.GetCookies(new Uri("https://editor.kuaidianyuedu.com/"))["p"].Value);
                }
                return (result, null);
            }
        }

        public static string GetImageUrl(string uuid)
        {
            return $"https://{ImageDomain}/{uuid}?x-oss-process=image/format,webp";
        }
    }
}
