using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScottBrady91.IdentityModel.Metadata
{
    public abstract class MetadataBase
    {
		public SigningCredentials SigningCredentials { get; set; }
	}
}
