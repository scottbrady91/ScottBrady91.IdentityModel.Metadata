
namespace ScottBrady91.IdentityModel.Metadata
{
    public class ServiceProviderSingleSignOnDescriptor : SingleSignOnDescriptor
	{
		public IndexedCollectionWithDefault<IndexedEndpoint> AssertionConsumerServices { get; private set; } =
			new IndexedCollectionWithDefault<IndexedEndpoint>();
		public bool AuthenticationRequestsSigned { get; set; }
		public bool WantAssertionsSigned { get; set; }
	}
}
