
namespace ScottBrady91.IdentityModel.Metadata
{
    // TODO: DiscoveryResponse
    public class DiscoveryResponse : IndexedEndpoint
	{
	}

    // TODO: SingleSignOnDescriptor2
    public class SingleSignOnDescriptor2 : SingleSignOnDescriptor
	{
		public IndexedCollectionWithDefault<AssertionConsumerService> AssertionConsumerServices { get; private set; } =
			new IndexedCollectionWithDefault<AssertionConsumerService>();
		public IndexedCollectionWithDefault<AttributeConsumingService> AttributeConsumingServices { get; private set; } =
			new IndexedCollectionWithDefault<AttributeConsumingService>();
		public bool? AuthnRequestsSigned { get; set; }
		public bool? WantAssertionsSigned { get; set; }
		public IndexedCollectionWithDefault<DiscoveryResponse> DiscoveryResponses { get; private set; } =
			new IndexedCollectionWithDefault<DiscoveryResponse>();
	}
}
