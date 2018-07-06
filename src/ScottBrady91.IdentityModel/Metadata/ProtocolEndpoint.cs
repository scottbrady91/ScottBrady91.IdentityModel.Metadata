using System;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class ProtocolEndpoint
    {
        public Uri Binding { get; set; }
        public Uri Location { get; set; }
        public Uri ResponseLocation { get; set; }

        public ProtocolEndpoint(Uri binding = null, Uri location = null)
        {
            Binding = binding;
            Location = location;
        }
    }
}