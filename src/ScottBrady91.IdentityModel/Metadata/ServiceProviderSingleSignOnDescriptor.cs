
namespace ScottBrady91.IdentityModel.Metadata
{
    public class ServiceProviderSingleSignOnDescriptor : SingleSignOnDescriptor
	{
		public IndexedCollectionWithDefault<AssertionConsumerService> AssertionConsumerServices { get; private set; } =
			new IndexedCollectionWithDefault<AssertionConsumerService>();
		public IndexedCollectionWithDefault<AttributeConsumingService> AttributeConsumingServices { get; private set; } =
			new IndexedCollectionWithDefault<AttributeConsumingService>();
		public bool AuthenticationRequestsSigned { get; set; }
		public bool WantAssertionsSigned { get; set; }
	}
}
