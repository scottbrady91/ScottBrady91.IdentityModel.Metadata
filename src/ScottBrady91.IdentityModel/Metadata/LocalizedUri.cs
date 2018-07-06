using System;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class LocalizedUri : LocalizedEntry
    {
        public LocalizedUri() { }
        public LocalizedUri(Uri uri, string language) : base(language)
        {
            Uri = uri;
        }
        
        public Uri Uri { get; set; }
    }
}