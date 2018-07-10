using System;

namespace ScottBrady91.IdentityModel.Metadata
{
    // TODO: SingleLogoutService
    public class SingleLogoutService : ProtocolEndpoint
	{
		public SingleLogoutService()
		{
		}

		public SingleLogoutService(Uri binding, Uri location) :
			base(binding, location)
		{
		}
	}
}
