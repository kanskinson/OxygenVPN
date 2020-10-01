using System;
using System.Collections.Generic;
using System.Net;
using OxygenVPN.Models.GitHubRelease;
using OxygenVPN.Utils;
using Newtonsoft.Json;

namespace OxygenVPN.Controllers
{
    public class UpdateChecker
    {
        public const string Owner = @"kanskinson";
        public const string Repo = @"OxygenVPN";

        public const string Name = @"OxygenVPN";
        public const string Copyright = @"Copyright © 2020";

        public const string AssemblyVersion = @"1.0.1";
        private const string Suffix = @"Release";

        public static readonly string Version = $"{AssemblyVersion}{(string.IsNullOrEmpty(Suffix) ? "" : $"-{Suffix}")}";

        public string LatestVersionNumber;
        public string LatestVersionUrl;
        public string LatestVersionDownloadUrl;

        public event EventHandler NewVersionFound;
        public event EventHandler NewVersionFoundFailed;
        public event EventHandler NewVersionNotFound;

        public async void Check(bool isPreRelease)
        {
            try
            {
                var updater = new GitHubRelease(Owner, Repo);
                var url = updater.AllReleaseUrl;

                var json = await WebUtil.DownloadStringAsync(WebUtil.CreateRequest(url));

                var releases = JsonConvert.DeserializeObject<List<Release>>(json);
                var latestRelease = VersionUtil.GetLatestRelease(releases, isPreRelease);
                LatestVersionNumber = latestRelease.tag_name;
                LatestVersionUrl = latestRelease.html_url;
                LatestVersionDownloadUrl = latestRelease.assets[0].browser_download_url;
                Logging.Info($"Github Lastest Release: {latestRelease.tag_name}");
                int compareVer = VersionUtil.CompareVersion(latestRelease.tag_name, Version);
                if (compareVer > 0)
                {
                    Logging.Info("New Version Found");
                    Logging.Info("Ver Compare:"+compareVer);
                    NewVersionFound?.Invoke(this, new EventArgs());
                }
                else
                {
                    Logging.Info("This is the latest version");
                    NewVersionNotFound?.Invoke(this, new EventArgs());
                }
            }
            catch (Exception e)
            {
                if (e is WebException)
                    Logging.Warning($"Update Failed: {e.Message}");
                else
                {
                    Logging.Warning(e.ToString());
                }

                NewVersionFoundFailed?.Invoke(this, new EventArgs());
            }
        }
    }
}
