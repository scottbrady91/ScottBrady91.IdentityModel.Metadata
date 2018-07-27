using System;
using System.IO;

namespace ScottBrady91.IdentityModel
{
    public class PathHelper
    {
        public static string BasePath => AppDomain.CurrentDomain.BaseDirectory;

        public static string MapPath(string virtualPath)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException(nameof(virtualPath));
            }

            if (!IsWebRootRelative(virtualPath))
            {
                return Path.GetFullPath(virtualPath);
            }
            
            // Strip until and including the initial /
            virtualPath = virtualPath.Substring(virtualPath.IndexOfAny(new[] {'/', '\\'}) + 1);

            // Normalize the slashes.
            virtualPath = virtualPath.Replace('/', '\\');
            return Path.Combine(BasePath, virtualPath);
        }

        public static bool IsWebRootRelative(string virtualPath)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException(nameof(virtualPath));
            }
            if (virtualPath.Length == 0)
            {
                return false;
            }

            if (virtualPath.StartsWith(@"~/", StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }
    }
}