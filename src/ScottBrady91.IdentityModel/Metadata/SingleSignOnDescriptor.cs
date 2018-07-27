using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class SingleSignOnDescriptor : RoleDescriptor
    {
        public IndexedCollectionWithDefault<IndexedEndpoint> ArtifactResolutionServices { get; } = new IndexedCollectionWithDefault<IndexedEndpoint>();
		public ICollection<ProtocolEndpoint> SingleLogoutServices { get; } = new Collection<ProtocolEndpoint>();
		public ICollection<Uri> NameIdentifierFormats { get; } = new Collection<Uri>();
    }
}
