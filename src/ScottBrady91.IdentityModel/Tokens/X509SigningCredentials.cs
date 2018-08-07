using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace ScottBrady91.IdentityModel.Tokens
{
    public class X509SigningCredentials : SigningCredentials
    {
        public X509Certificate2 Certificate { get; }

        public X509SigningCredentials(X509Certificate2 certificate)
            : base(new X509SecurityKey(certificate), SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest)
        {
            Certificate = certificate;
        }

        public X509SigningCredentials(X509Certificate2 certificate, string algorithm, string digest) 
            : base(new X509SecurityKey(certificate), algorithm, digest)
        {
            Certificate = certificate;
        }
    }
}