using System;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class SingleSignOnService : ProtocolEndpoint
    {

        public SingleSignOnService()
        {
        }

        public SingleSignOnService(Uri binding, Uri location) :
            base(binding, location)
        {
        }
    }
}
