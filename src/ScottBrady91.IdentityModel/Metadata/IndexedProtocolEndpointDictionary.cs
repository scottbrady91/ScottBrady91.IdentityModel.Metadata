using System.Collections.Generic;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class IndexedProtocolEndpointDictionary : SortedList<int, IndexedProtocolEndpoint>
    {
        public IndexedProtocolEndpoint Default
        {
            get
            {
                IndexedProtocolEndpoint impliedDefault = null;
                foreach (var endpoint in Values)
                {
                    if (endpoint.IsDefault == true)
                    {
                        return endpoint;
                    }
                    if (endpoint.IsDefault.HasValue && impliedDefault == null)
                    {
                        impliedDefault = endpoint;
                    }
                }

                return impliedDefault ?? (0 < Count ? this[0] : null);
            }
        }
    }
}