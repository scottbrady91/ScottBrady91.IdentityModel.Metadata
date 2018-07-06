using System;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class IndexedProtocolEndpoint : ProtocolEndpoint
    {
        public int Index { get; set; }
        public bool? IsDefault { get; set; } = null;

        public IndexedProtocolEndpoint(int index, Uri binding, Uri location) : base(binding, location)
        {
            Index = index;
        }
    }
}