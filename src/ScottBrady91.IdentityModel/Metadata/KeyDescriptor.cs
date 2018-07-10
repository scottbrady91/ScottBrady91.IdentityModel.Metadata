using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ScottBrady91.IdentityModel.Tokens;
using Microsoft.IdentityModel.Xml;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class KeyDescriptor
	{
		public DSigKeyInfo KeyInfo { get; set; }
		public KeyType Use { get; set; } = KeyType.Unspecified;
		public ICollection<EncryptionMethod> EncryptionMethods { get; private set; } =
			new Collection<EncryptionMethod>();

		public KeyDescriptor()
		{
		}
	}
}
