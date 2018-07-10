using Microsoft.IdentityModel.Tokens.Saml2;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
    // TODO: AttributeAuthorityDescriptor???
    public class AttributeAuthorityDescriptor : RoleDescriptor
	{
		public ICollection<AttributeService> AttributeServices { get; } = new Collection<AttributeService>();
		public ICollection<AssertionIdRequestService> AssertionIdRequestServices { get; } = new Collection<AssertionIdRequestService>();
		public ICollection<NameIDFormat> NameIdFormats { get; } = new Collection<NameIDFormat>();
		public ICollection<AttributeProfile> AttributeProfiles { get; } = new Collection<AttributeProfile>();
		public ICollection<Saml2Attribute> Attributes { get; } = new Collection<Saml2Attribute>();
	}
}
