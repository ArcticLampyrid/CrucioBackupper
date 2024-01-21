using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CrucioBackupper
{
    public class GitHubUpdateChecker(string repositoryOwner, string repositoryName, Version currentVersion)
    {
        private readonly string _repositoryOwner = repositoryOwner;
        private readonly string _repositoryName = repositoryName;
        private readonly Version _currentVersion = currentVersion;

        public GitHubUpdateChecker(string repositoryOwner, string repositoryName)
            : this(repositoryOwner, repositoryName, GetCurrentVersion())
        {
            // Do nothing here
        }

        private static Version GetCurrentVersion() => typeof(GitHubUpdateChecker).Assembly.GetName().Version;

        public async Task<string> CheckForUpdatesAsync()
        {
            using var client = new HttpClient();
            try
            {
                string url = $"https://api.github.com/repos/{_repositoryOwner}/{_repositoryName}/releases/latest";
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue(_repositoryName, _currentVersion.ToString()));
                var response = await client.GetStringAsync(url);

                using var jsonDoc = JsonDocument.Parse(response);
                var latestVersion = jsonDoc.RootElement.GetProperty("tag_name").GetString();
                if (latestVersion.StartsWith('v'))
                {
                    latestVersion = latestVersion[1..];
                }
                if (Version.Parse(latestVersion) > _currentVersion)
                {
                    return jsonDoc.RootElement.GetProperty("html_url").GetString();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "检查版本更新失败");
                return null;
            }
        }
    }
}
