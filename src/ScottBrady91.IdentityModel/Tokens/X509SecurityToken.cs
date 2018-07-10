using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;

namespace ScottBrady91.IdentityModel.Tokens
{
    public class X509SecurityToken : SecurityToken, IDisposable
    {
        private readonly X509Certificate2 certificate;
        private readonly bool disposable;
        private bool disposed;
        
        public override string Id { get; }
        public override DateTime ValidFrom { get; }
        public override DateTime ValidTo { get; }

        private ReadOnlyCollection<SecurityKey> securityKeys;
        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                ThrowIfDisposed();
                if (securityKeys == null)
                {
                    var temp = new List<SecurityKey>(1);
                    temp.Add(new X509AsymmetricSecurityKey(this.certificate)); // TODO
                    securityKeys = temp.AsReadOnly();
                }
                return this.securityKeys;

            }
        }

        public X509SecurityToken(X509Certificate2 certificate) : this(certificate, SecurityUniqueId.Create().Value) { }
        public X509SecurityToken(X509Certificate2 certificate, string id) : this(certificate, id, true) { }
        internal X509SecurityToken(X509Certificate2 certificate, bool clone) : this(certificate, SecurityUniqueId.Create().Value, clone) { }
        internal X509SecurityToken(X509Certificate2 certificate, bool clone, bool disposable) : this(certificate, SecurityUniqueId.Create().Value, clone, disposable) { }
        internal X509SecurityToken(X509Certificate2 certificate, string id, bool clone) : this(certificate, id, clone, true) { }

        internal X509SecurityToken(X509Certificate2 certificate, string id, bool clone, bool disposable)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            this.certificate = clone ? new X509Certificate2(certificate) : certificate;
            this.disposable = clone || disposable;
        }

        public void Dispose()
        {
            if (disposable && !disposed)
            {
                disposed = true;
                certificate.Reset();
            }
        }

        protected void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException("X509SecurityToken");
        }
    }
}