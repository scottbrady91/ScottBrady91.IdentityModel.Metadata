using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class SecurityTokenServiceDescriptor : WebServiceDescriptor
    {
		public ICollection<EndpointReference> SecurityTokenServiceEndpoints { get; } = new Collection<EndpointReference>();
        public ICollection<EndpointReference> PassiveRequestorEndpoints { get; } = new Collection<EndpointReference>();

        // TODO: SecurityTokenServiceDescriptor extensions
        public ICollection<EndpointReference> SingleSignOutSubscriptionEndpoints { get; } = new Collection<EndpointReference>();
		public ICollection<EndpointReference> SingleSignOutNotificationEndpoints { get; } = new Collection<EndpointReference>();
	}
}
