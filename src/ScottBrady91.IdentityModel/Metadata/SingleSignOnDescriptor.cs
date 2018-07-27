using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class SingleSignOnDescriptor : RoleDescriptor
    {
		public IndexedCollectionWithDefault<ArtifactResolutionService> ArtifactResolutionServices { get; } = new IndexedCollectionWithDefault<ArtifactResolutionService>();
		public ICollection<SingleLogoutService> SingleLogoutServices { get; } = new Collection<SingleLogoutService>();
		public ICollection<NameIDFormat> NameIdentifierFormats { get; } = new Collection<NameIDFormat>();
    }
}
