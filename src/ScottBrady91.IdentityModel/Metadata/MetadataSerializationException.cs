using System;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class MetadataSerializationException : Exception
    {
        public MetadataSerializationException(string message) :
            base(message)
        {
        }

        public MetadataSerializationException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}