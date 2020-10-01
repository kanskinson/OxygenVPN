using System.Collections.Generic;

namespace OxygenVPN.Models.GitHubRelease
{
    public class VersionComparer : IComparer<object>
    {
        public int Compare(object x, object y)
        {
            return VersionUtil.CompareVersion(x?.ToString(), y?.ToString());
        }
    }
}
