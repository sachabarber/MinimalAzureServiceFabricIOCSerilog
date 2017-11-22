using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Stateless1.Extensions
{
    public static class AssemblyExtensions
    {
        public static string GetInformationalVersion(this Assembly assembly, bool fallbackToAssemblyVersion = true)
        {
            var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (infoVersion != null)
            {
                return infoVersion.InformationalVersion;
            }

            if (fallbackToAssemblyVersion)
            {
                return assembly.GetName().Version.ToString();
            }

            return null;
        }
    }
}
