using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace Sustainsys.Saml2.Metadata
{
    public class EntityDescriptor : MetadataBase
    {
		public string Id { get; set; }
		public ICollection<ContactPerson> Contacts { get; private set; } =
			new Collection<ContactPerson>();
		public EntityId EntityId { get; set; }
		public string FederationId { get; set; }
		public Organization Organization { get; set; }
		public ICollection<RoleDescriptor> RoleDescriptors { get; private set; } =
			new Collection<RoleDescriptor>();
		public DateTime? ValidUntil { get; set; }
		public ICollection<AdditionalMetadataLocation> AdditionalMetadataLocations { get; private set; } =
			new Collection<AdditionalMetadataLocation>();
		public Collection<XmlElement> Extensions { get; private set; } =
			new Collection<XmlElement>();

		public EntityDescriptor(EntityId entityId)
		{
			EntityId = entityId;
		}

		public EntityDescriptor() :
			this(null)
		{
		}
	}
}
