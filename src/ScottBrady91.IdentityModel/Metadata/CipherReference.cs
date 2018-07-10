using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class CipherReference
	{
		public Uri Uri { get; set; }
		public ICollection<XmlElement> Transforms { get; private set; } =
			new Collection<XmlElement>();
	}
}
