using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class EndpointReference
	{
	    public ICollection<XmlElement> Details { get; } = new Collection<XmlElement>();
	    public Uri Uri { get; set; }

        // TODO: EndpointReference extensions
        public ICollection<XmlElement> Metadata { get; } = new Collection<XmlElement>();
		public ICollection<XmlElement> ReferenceProperties { get; } = new Collection<XmlElement>();
		public ICollection<XmlElement> ReferenceParameters { get;  } = new Collection<XmlElement>();
		public ICollection<XmlElement> Policies { get; } = new Collection<XmlElement>();
		public string PortType { get; set; }
		public ServiceName ServiceName { get; set; }

		internal EndpointReference() { }
		public EndpointReference(string uri)
		{
		    if (uri == null) throw new ArgumentNullException(nameof(uri));
            Uri = new Uri(uri);

            if (!Uri.IsAbsoluteUri) throw new ArgumentException("Must be an absolute URI", nameof(uri));
		}

	    public void WriteTo(XmlWriter writer)
	    {
	        if (writer == null) throw new ArgumentNullException(nameof(writer));

	        writer.WriteStartElement(WSAddressing10Constants.Prefix, WSAddressing10Constants.Elements.EndpointReference, WSAddressing10Constants.NamespaceUri);
            writer.WriteStartElement(WSAddressing10Constants.Prefix, WSAddressing10Constants.Elements.Address, WSAddressing10Constants.NamespaceUri);
	        writer.WriteString(Uri.AbsoluteUri);
	        writer.WriteEndElement();
	        foreach (var element in Details) element.WriteTo(writer);

	        writer.WriteEndElement();
	    }

	    public static EndpointReference ReadFrom(XmlReader reader) => ReadFrom(XmlDictionaryReader.CreateDictionaryReader(reader));

        public static EndpointReference ReadFrom(XmlDictionaryReader reader)
	    {
	        if (reader == null) throw new ArgumentNullException(nameof(reader));

	        reader.ReadFullStartElement();
	        reader.MoveToContent();

	        if (reader.IsNamespaceUri(WSAddressing10Constants.NamespaceUri) || reader.IsNamespaceUri(WSAddressing200408Constants.NamespaceUri))
	        {
	            if (reader.IsStartElement(WSAddressing10Constants.Elements.Address, WSAddressing10Constants.NamespaceUri)
	                || reader.IsStartElement(WSAddressing10Constants.Elements.Address, WSAddressing200408Constants.NamespaceUri))
	            {
	                var er = new EndpointReference(reader.ReadElementContentAsString());
	                while (reader.IsStartElement())
	                {
	                    var emptyElement = reader.IsEmptyElement;

	                    var subtreeReader = reader.ReadSubtree();
	                    var doc = new XmlDocument {PreserveWhitespace = true};
	                    doc.Load(subtreeReader);

	                    er.Details.Add(doc.DocumentElement);

	                    if (!emptyElement)
	                    {
	                        reader.ReadEndElement();
	                    }
	                }

	                reader.ReadEndElement();
	                return er;
	            }
	        }

	        return null;
	    }
    }
}
