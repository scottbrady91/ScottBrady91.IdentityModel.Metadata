using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Tokens
{
	public class AudienceRestriction
    {
		public AudienceUriMode AudienceMode { get; set; } = AudienceUriMode.Always;
		public ICollection<Uri> AllowedAudienceUris { get; } = new Collection<Uri>();

		public AudienceRestriction() { }
		public AudienceRestriction(AudienceUriMode audienceMode)
		{
			AudienceMode = audienceMode;
		}
	}
}
