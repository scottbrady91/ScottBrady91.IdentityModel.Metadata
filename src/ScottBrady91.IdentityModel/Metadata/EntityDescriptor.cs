using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class EntityDescriptor : MetadataBase
    {
        public EntityId EntityId { get; set; }
        public ICollection<RoleDescriptor> RoleDescriptors { get; } = new Collection<RoleDescriptor>();
        public ICollection<ContactPerson> Contacts { get; } = new Collection<ContactPerson>();
        public Organization Organization { get; set; }
        public string FederationId { get; set; }

        public EntityDescriptor() { }
        public EntityDescriptor(EntityId entityId)
		{
			EntityId = entityId;
		}
	}
}
