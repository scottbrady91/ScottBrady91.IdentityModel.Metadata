using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class EncryptionProperties
	{
		// EncryptionProperty
		public string Id { get; set; }
		public ICollection<EncryptionProperty> Properties { get; private set; } =
			new Collection<EncryptionProperty>();
	}
}
