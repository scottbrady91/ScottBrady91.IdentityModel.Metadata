using System;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class EncryptedValue
	{
		public Uri DecryptionCondition { get; set; }
		public EncryptedData EncryptedData { get; set; }
	}
}
