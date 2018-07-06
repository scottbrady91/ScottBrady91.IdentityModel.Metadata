namespace ScottBrady91.IdentityModel.Metadata
{
    public class ServiceProviderSingleSignOnDescriptor : SingleSignOnDescriptor
    {
        public bool WantAssertionsSigned { get; set; }
        public bool AuthenticationRequestsSigned { get; set; }
        public IndexedProtocolEndpointDictionary AssertionConsumerServices { get; }

        public ServiceProviderSingleSignOnDescriptor() : this(new IndexedProtocolEndpointDictionary()) { }
        public ServiceProviderSingleSignOnDescriptor(IndexedProtocolEndpointDictionary endpoints)
        {
            AssertionConsumerServices = endpoints;
        }
    }
}