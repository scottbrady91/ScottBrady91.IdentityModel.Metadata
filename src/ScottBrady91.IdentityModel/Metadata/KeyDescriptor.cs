using ScottBrady91.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.Xml;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class KeyDescriptor
    {
        public KeyDescriptor() { }
        public KeyDescriptor(SecurityKeyIdentifier keyInfo) { KeyInfo = keyInfo; }

        public SecurityKeyIdentifier KeyInfo { get; set; }
        public KeyType Use { get; set; } = KeyType.Unspecified;
        public ICollection<EncryptionMethod> EncryptionMethods { get; } = new Collection<EncryptionMethod>();
    }
}