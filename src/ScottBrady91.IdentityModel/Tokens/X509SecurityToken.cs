using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;

namespace ScottBrady91.IdentityModel.Tokens
{
    // TODO: X509SecurityToken
    public class X509SecurityToken : SecurityToken, IDisposable
    {
        private readonly X509Certificate2 certificate;
        private bool disposed = false;
        private bool disposable;


        public override string Id { get; }
        public override DateTime ValidFrom { get; }
        public override DateTime ValidTo { get; }
        public override ReadOnlyCollection<SecurityKey> SecurityKeys { get; }

        // TODO: Constructors using IdentityModel Crypto random
        internal X509SecurityToken(X509Certificate2 certificate, string id, bool clone)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            this.certificate = clone ? new X509Certificate2(certificate) : certificate;
            disposable = clone || disposable;
        }

        public void Dispose()
        {
            if (disposable && !disposed)
            {
                disposed = true;
                certificate.Reset();
            }
        }
    }
}