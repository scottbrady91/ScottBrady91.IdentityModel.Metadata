using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.IdentityModel.Xml;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class KeyDescriptor
	{
        public KeyInfo KeyInfo { get; set; }
		public KeyType Use { get; set; } = KeyType.Unspecified;
		public ICollection<EncryptionMethod> EncryptionMethods { get; } = new Collection<EncryptionMethod>();

		public KeyDescriptor() { }
        public KeyDescriptor(KeyInfo keyInfo)
        {
            KeyInfo = keyInfo;
        }

    }
}
