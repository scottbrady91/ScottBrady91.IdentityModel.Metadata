using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class EntityDescriptor : MetadataBase
    {
        public EntityId EntityId { get; set; }
        public ICollection<RoleDescriptor> RoleDescriptors { get; } = new Collection<RoleDescriptor>();
        public ICollection<ContactPerson> Contacts { get; } = new Collection<ContactPerson>();
        public Organization Organization { get; set; }
        public string FederationId { get; set; }

        // TODO: EntityDescriptor extension
        public string Id { get; set; }
		public DateTime? ValidUntil { get; set; }
		public ICollection<AdditionalMetadataLocation> AdditionalMetadataLocations { get; } = new Collection<AdditionalMetadataLocation>();
		public ICollection<XmlElement> Extensions { get; } = new Collection<XmlElement>();

        public EntityDescriptor() { }
        public EntityDescriptor(EntityId entityId)
		{
			EntityId = entityId;
		}
	}
}
