using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace ScottBrady91.IdentityModel.Metadata
{
    public abstract class RoleDescriptor
    {
        public string Id { get; set; }
        public DateTime? ValidUntil { get; set; }
        
        public ICollection<KeyDescriptor> Keys { get; } = new Collection<KeyDescriptor>();
        public ICollection<Uri> ProtocolsSupported { get; } = new Collection<Uri>();

        public ICollection<ContactPerson> Contacts { get; } = new Collection<ContactPerson>();
        public Organization Organization { get; set; }
        public Uri ErrorUrl { get; set; }

        public ICollection<XmlElement> Extensions { get; private set; } = new Collection<XmlElement>(); // TODO: Metadata Extensions???

		protected RoleDescriptor() : this(new Collection<Uri>()) { }
        protected RoleDescriptor(ICollection<Uri> protocolsSupported)
		{
			ProtocolsSupported = protocolsSupported;
		}
    }
}
