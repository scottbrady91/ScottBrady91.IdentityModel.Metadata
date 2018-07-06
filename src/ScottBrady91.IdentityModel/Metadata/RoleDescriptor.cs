using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
    public abstract class RoleDescriptor
    {
        protected RoleDescriptor() : this(new Collection<Uri>()) { }
        protected RoleDescriptor(ICollection<Uri> protocolsSupported)
        {
            ProtocolsSupported = protocolsSupported;
        }

        public string Id { get; set; }
        public ICollection<KeyDescriptor> Keys { get; } = new Collection<KeyDescriptor>();
        public ICollection<Uri> ProtocolsSupported { get; }
        public Uri ErrorUrl { get; set; }
        public Organization Organization { get; set; }
        public ICollection<ContactPerson> Contacts { get; } = new Collection<ContactPerson>();

        public DateTime? ValidUntil { get; set; }

        //public XsdDuration? CacheDuration { get; set; }
        //public ICollection<XmlElement> Extensions { get; private set; } = new Collection<XmlElement>();
    }
}