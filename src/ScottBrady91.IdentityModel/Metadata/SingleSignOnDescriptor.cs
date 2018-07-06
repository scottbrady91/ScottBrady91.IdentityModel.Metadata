using System;
using System.Collections.Generic;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class SingleSignOnDescriptor : RoleDescriptor
    {
        public ICollection<Uri> NameIdentifierFormats { get; } = new List<Uri>();
        public IndexedProtocolEndpointDictionary ArtifactResolutionServices { get; } = new IndexedProtocolEndpointDictionary();
        public ICollection<ProtocolEndpoint> SingleLogoutServices { get; } = new List<ProtocolEndpoint>();
    }
}