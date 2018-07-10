using System;

namespace ScottBrady91.IdentityModel.Metadata
{
    // TODO: EncryptedValue
    public class EncryptedValue
	{
		public Uri DecryptionCondition { get; set; }
		public EncryptedData EncryptedData { get; set; }
	}
}
