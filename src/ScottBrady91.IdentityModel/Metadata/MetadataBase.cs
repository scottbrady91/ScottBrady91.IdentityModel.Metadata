using Microsoft.IdentityModel.Tokens;

namespace ScottBrady91.IdentityModel.Metadata
{
    public abstract class MetadataBase
    {
		public SigningCredentials SigningCredentials { get; set; }
	}
}
