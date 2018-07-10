using System.Xml;

namespace ScottBrady91.IdentityModel.Metadata
{
    // TODO: ExtendedMetadataSerializer
    internal class ExtendedMetadataSerializer : MetadataSerializer
    {
        private ExtendedMetadataSerializer() { }
        private ExtendedMetadataSerializer(SecurityTokenSerializer serializer) : base(serializer) { }
        
        public static ExtendedMetadataSerializer ReaderInstance { get; } = new ExtendedMetadataSerializer();

        public static ExtendedMetadataSerializer WriterInstance { get; } = new ExtendedMetadataSerializer();

        protected override void WriteCustomAttributes<T>(XmlWriter writer, T source)
        {
            if(typeof(T) == typeof(EntityDescriptor))
            {
                writer.WriteAttributeString("xmlns", "saml2", null, "urn:oasis:names:tc:SAML:2.0:assertion"); // TODO: const
            }
        }
	}
}
