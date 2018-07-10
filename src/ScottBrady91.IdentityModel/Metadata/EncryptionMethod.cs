using System;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class EncryptionMethod
	{
		public int KeySize { get; set; }
		public byte[] OAEPparams { get; set; }
		public Uri Algorithm { get; set; }
	}
}
