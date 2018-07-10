using System;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class SingleLogoutService : Endpoint
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
