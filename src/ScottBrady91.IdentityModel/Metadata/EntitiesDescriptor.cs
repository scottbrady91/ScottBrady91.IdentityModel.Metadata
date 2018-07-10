using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class EntitiesDescriptor : MetadataBase
    {
        public string Name { get; set; }
        public ICollection<EntityDescriptor> ChildEntities { get; } = new Collection<EntityDescriptor>();
		public ICollection<EntitiesDescriptor> ChildEntityGroups { get; } = new Collection<EntitiesDescriptor>();

        // TODO EntitiesDescriptor extensions
        public ICollection<XmlElement> Extensions { get; } = new Collection<XmlElement>();
        public string Id { get; set; }
		public DateTime? ValidUntil { get; set; }

        public EntitiesDescriptor() { }

        public EntitiesDescriptor(ICollection<EntityDescriptor> entityList)
        {
            ChildEntities = entityList;
        }

        public EntitiesDescriptor(ICollection<EntitiesDescriptor> entityGroupList)
        {
            ChildEntityGroups = entityGroupList;
        }
    }
}
