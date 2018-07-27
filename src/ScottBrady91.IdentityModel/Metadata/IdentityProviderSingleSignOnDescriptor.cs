using Microsoft.IdentityModel.Tokens.Saml2;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class IdentityProviderSingleSignOnDescriptor : SingleSignOnDescriptor
    {
        public bool WantAuthenticationRequestsSigned { get; set; }
        public ICollection<SingleSignOnService> SingleSignOnServices { get; } = new Collection<SingleSignOnService>();
        public ICollection<Saml2Attribute> SupportedAttributes { get; } = new Collection<Saml2Attribute>();
	}
}
