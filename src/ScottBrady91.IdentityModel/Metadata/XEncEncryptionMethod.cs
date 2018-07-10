using System;

namespace ScottBrady91.IdentityModel.Metadata
{
    // TODO: XEncEncryptionMethod
    public class XEncEncryptionMethod
	{
		public int KeySize { get; set; }
		public byte[] OAEPparams { get; set; }
		public Uri Algorithm { get; set; }
	}
}
