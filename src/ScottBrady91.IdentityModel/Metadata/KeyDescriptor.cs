using System.Collections.Generic;
using System.Collections.ObjectModel;
using ScottBrady91.IdentityModel.Tokens;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class KeyDescriptor
	{
        public SecurityKeyIdentifier KeyInfo { get; set; }
		public KeyType Use { get; set; } = KeyType.Unspecified;
		public ICollection<EncryptionMethod> EncryptionMethods { get; } = new Collection<EncryptionMethod>();

		public KeyDescriptor() { }
        public KeyDescriptor(SecurityKeyIdentifier keyInfo)
        {
            KeyInfo = keyInfo;
        }

    }
}
