using System;
using System.Globalization;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class LocalizedUri : LocalizedEntry
	{
		public Uri Uri { get; set; }

	    public LocalizedUri() { }
        public LocalizedUri(Uri uri, CultureInfo language) : base(language)
		{
			Uri = uri;
		}
	}
}
