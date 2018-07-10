using Microsoft.IdentityModel.Tokens.Saml2;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class IdentityProviderSingleSignOnDescriptor : SingleSignOnDescriptor
    {
        public bool? WantAuthnRequestsSigned { get; set; }
        public ICollection<SingleSignOnService> SingleSignOnServices { get; } = new Collection<SingleSignOnService>();
        public ICollection<Saml2Attribute> SupportedAttributes { get; } = new Collection<Saml2Attribute>();

        // TODO: IdentityProviderSingleSignOnDescriptor extensions
        public ICollection<NameIDMappingService> NameIDMappingServices { get; } = new Collection<NameIDMappingService>();
		public ICollection<AssertionIdRequestService> AssertionIDRequestServices { get; private set; } = new Collection<AssertionIdRequestService>();
		public ICollection<AttributeProfile> AttributeProfiles { get; } = new Collection<AttributeProfile>();
		
	}
}
