using System;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class EncryptionMethod
	{
	    public EncryptionMethod(Uri algorithm)
	    {
	        Algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
	    }

	    public Uri Algorithm { get; set; }
	}
}
