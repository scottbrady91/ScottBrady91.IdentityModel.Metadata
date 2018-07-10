using System.Xml;

namespace ScottBrady91.IdentityModel
{
    public class XmlHelpers
    {
        public static XmlDocument CreateSafeXmlDocument()
        {
            return new XmlDocument()
            {
                // Null is the default on 4.6 and later, but not on 4.5.
                XmlResolver = null,
                PreserveWhitespace = true
            };
        }
    }
}