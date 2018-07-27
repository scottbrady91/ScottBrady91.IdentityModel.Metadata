using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class EndpointReference
	{
	    public Uri Uri { get; set; }
        public ICollection<XmlElement> Details { get; } = new Collection<XmlElement>();
        
		public EndpointReference(string uri)
		{
		    if (uri == null) throw new ArgumentNullException(nameof(uri));
            Uri = new Uri(uri);
            if (!Uri.IsAbsoluteUri) throw new ArgumentException("Must be an absolute URI", nameof(uri));
		}
    }
}
