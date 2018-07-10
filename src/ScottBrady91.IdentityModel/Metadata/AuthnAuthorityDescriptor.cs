﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class AuthnAuthorityDescriptor : RoleDescriptor
	{
		public ICollection<AuthnQueryService> AuthnQueryServices { get; private set; } =
			new Collection<AuthnQueryService>();
		public ICollection<AssertionIdRequestService> AssertionIdRequestServices { get; private set; } =
			new Collection<AssertionIdRequestService>();
		public ICollection<NameIDFormat> NameIDFormats { get; private set; } =
			new Collection<NameIDFormat>();
	}
}
