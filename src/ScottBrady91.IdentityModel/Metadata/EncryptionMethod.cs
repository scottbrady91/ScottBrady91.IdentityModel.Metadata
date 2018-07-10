using System;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class EncryptionMethod
	{
	    public Uri Algorithm { get; set; }

        // TODO: EncryptionMethod extensions
        public int KeySize { get; set; }
		public byte[] OAEPparams { get; set; }
	}
}
