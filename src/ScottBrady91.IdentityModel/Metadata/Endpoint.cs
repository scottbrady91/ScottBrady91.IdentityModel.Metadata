using System;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class Endpoint
	{
		public Uri Binding { get; set; }
		public Uri Location { get; set; }
		public Uri ResponseLocation { get; set; }

		public Endpoint()
		{
		}

		public Endpoint(Uri binding, Uri location)
		{
			Binding = binding;
			Location = location;
		}
	}
}
