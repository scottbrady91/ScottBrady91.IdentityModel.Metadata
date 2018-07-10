using System;
using System.Collections.Generic;
using System.Xml;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class DisplayClaim
	{
		public string ClaimType { get; private set; }
		public string DisplayName { get; set; }
	    public string DisplayValue { get; set; }
        public string Description { get; set; }
		public bool? Optional { get; set; }
        public bool WriteOptionalAttribute { get; set; }

        // TODO DisplayClaim extensions
        public string Value { get; set; }
		public ICollection<XmlElement> StructuredValue { get; set; }
		public EncryptedValue EncryptedValue { get; set; }
		public ConstrainedValue ConstrainedValue { get; set; }

		public DisplayClaim(string claimType)
		{
		    ClaimType = claimType ?? throw new ArgumentNullException(nameof(claimType));
		}
	}
}
