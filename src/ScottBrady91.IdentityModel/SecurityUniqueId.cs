using System;
using System.Globalization;
using System.Threading;

namespace ScottBrady91.IdentityModel
{
    public class SecurityUniqueId
    {
        private static readonly string CommonPrefix = "uuid-" + Guid.NewGuid() + "-";
        private static long nextId = 0;
        
        private readonly long id;
        private readonly string prefix;
        private string val;

        public SecurityUniqueId(string prefix, long id)
        {
            this.id = id;
            this.prefix = prefix;
            val = null;
        }

        public static SecurityUniqueId Create()
        {
            return SecurityUniqueId.Create(CommonPrefix);
        }

        public static SecurityUniqueId Create(string prefix)
        {
            return new SecurityUniqueId(prefix, Interlocked.Increment(ref nextId));
        }

        public string Value => val ?? (val = prefix + id.ToString(CultureInfo.InvariantCulture));
    }
}