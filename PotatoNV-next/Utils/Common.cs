using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PotatoNV_next.Utils
{
    class Common
    {
        public static string GetAssemblyVersion(Assembly assembly)
        {
            return assembly.GetName().Version.ToString(3);
        }

        public static string GetAssemblyVersion(string name)
        {
            try
            {
                return FileVersionInfo.GetVersionInfo(name).FileVersion;
            }
            catch
            {
                return null;
            }
        }
    }
}
