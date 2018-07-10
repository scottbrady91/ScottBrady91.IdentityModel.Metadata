﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class PDPDescriptor : RoleDescriptor
    {
		public ICollection<AuthzService> AuthzServices { get; private set; } =
			new Collection<AuthzService>();
		public ICollection<AssertionIdRequestService> AssertionIdRequestServices { get; private set; } =
			new Collection<AssertionIdRequestService>();
		public ICollection<NameIDFormat> NameIDFormats { get; private set; } =
			new Collection<NameIDFormat>();
	}
}
