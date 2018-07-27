using System;
using System.Xml;

namespace ScottBrady91.IdentityModel
{
    internal static class XmlHelpers
    {
        // Element helpers
        public static void WriteElementIfPresent(this XmlWriter writer, string elementName, string elementNamespace, string value)
        {
            if (!string.IsNullOrEmpty(value))
                writer.WriteElementString(elementName, elementNamespace, value);
        }

        public static void WriteElementIfPresent(this XmlWriter writer, string prefix, string elementName, string elementNamespace, string value)
        {
            if (!string.IsNullOrEmpty(value))
                writer.WriteElementString(prefix, elementName, elementNamespace, value);
        }

        public static void WriteElementIfPresent(this XmlWriter writer, string elementName, string elementNamespace, byte[] value)
        {
            if (value != null)
                writer.WriteElementString(elementName, elementNamespace, Convert.ToBase64String(value));
        }

        // Attribute helpers
        public static void WriteAttributeIfPresent(this XmlWriter writer, string attributeName, string attributeNamespace, bool? value)
        {
            if (value.HasValue)
                writer.WriteAttributeString(attributeName, attributeNamespace, XmlConvert.ToString(value.Value));
            
        }

        public static void WriteAttributeIfPresent(this XmlWriter writer, string attributeName, string attributeNamespace, string value)
        {
            if (!string.IsNullOrEmpty(value))
                writer.WriteAttributeString(attributeName, attributeNamespace, value);
        }
        
        public static void WriteAttributeIfPresent(this XmlWriter writer, string attributeName, string attributeNamespace, Uri value)
        {
            if (value != null)
                writer.WriteAttributeString(attributeName, attributeNamespace, value.IsAbsoluteUri ? value.AbsoluteUri : value.ToString());
        }
    }
}