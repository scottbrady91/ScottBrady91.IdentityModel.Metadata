using System;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class ProtocolEndpoint
    {
		public Uri Binding { get; set; }
		public Uri Location { get; set; }
		public Uri ResponseLocation { get; set; }

		public ProtocolEndpoint() { }
		public ProtocolEndpoint(Uri binding, Uri location)
		{
			Binding = binding;
			Location = location;
		}
	}
}
