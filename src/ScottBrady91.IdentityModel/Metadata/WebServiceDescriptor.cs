using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
	public abstract class WebServiceDescriptor : RoleDescriptor
	{
	    public ICollection<DisplayClaim> ClaimTypesOffered { get; } = new Collection<DisplayClaim>();
	    public ICollection<DisplayClaim> ClaimTypesRequested { get; } = new Collection<DisplayClaim>();
	    public string ServiceDescription { get; set; }
	    public string ServiceDisplayName { get; set; }
        public ICollection<EndpointReference> TargetScopes { get; } = new Collection<EndpointReference>();
	    public ICollection<Uri> TokenTypesOffered { get; } = new Collection<Uri>();

        // TODO: WebServiceDescriptor extensions??? (localization)
        public bool? AutomaticPseudonyms { get; set; }
		public ICollection<Uri> ClaimDialectsOffered { get; } = new Collection<Uri>();
		public ICollection<Uri> LogicalServiceNamesOffered { get; } = new Collection<Uri>();
	}
}