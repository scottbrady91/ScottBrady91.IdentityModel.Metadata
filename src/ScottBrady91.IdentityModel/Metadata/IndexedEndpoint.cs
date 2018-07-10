using System;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class IndexedEndpoint : ProtocolEndpoint, IIndexedEntryWithDefault
    {
        public int Index { get; set; }
		public bool? IsDefault { get; set; }

        public IndexedEndpoint() { }
        public IndexedEndpoint(int index, Uri binding, Uri location) : base(binding, location)
        {
            Index = index;
        }
    }
}
