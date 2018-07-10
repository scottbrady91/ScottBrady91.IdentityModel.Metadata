using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class ApplicationServiceDescriptor : WebServiceDescriptor
	{
		public ICollection<EndpointReference> Endpoints { get; } = new Collection<EndpointReference>();
		public ICollection<EndpointReference> PassiveRequestorEndpoints { get; } = new Collection<EndpointReference>();

        // TODO: ApplicationServiceDescriptor SingleSignOutEndpoints
        public ICollection<EndpointReference> SingleSignOutEndpoints { get; } = new Collection<EndpointReference>();

		public ApplicationServiceDescriptor() { }
	}
}
