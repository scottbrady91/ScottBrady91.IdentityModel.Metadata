using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class KeyDescriptor
	{
        // TODO: DSignKeyInfo vs SecurityKeyIdentifier
        public DSigKeyInfo KeyInfo { get; set; }
		public KeyType Use { get; set; } = KeyType.Unspecified;
		public ICollection<EncryptionMethod> EncryptionMethods { get; } = new Collection<EncryptionMethod>();

		public KeyDescriptor() { }
        public KeyDescriptor(DSigKeyInfo keyInfo)
        {
            KeyInfo = keyInfo;
        }

    }
}
