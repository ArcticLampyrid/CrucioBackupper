using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CrucioBackupper.ViewModel
{
    public class AboutViewModel
    {
        public static AboutViewModel Instance { get; } = new AboutViewModel();

        private AboutViewModel()
        {
            var versionInfo = Attribute.GetCustomAttribute(typeof(AboutViewModel).Assembly, typeof(AssemblyInformationalVersionAttribute))
                as AssemblyInformationalVersionAttribute;
            var informationalVersion = versionInfo?.InformationalVersion;
            if (string.IsNullOrEmpty(informationalVersion))
            {
                VersionDesc = "vUnknown";
            }
            else
            {
                VersionDesc = $"v{informationalVersion}";
            }
        }

        public string VersionDesc { get; }
    }
}
