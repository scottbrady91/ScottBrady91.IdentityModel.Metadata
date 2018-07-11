using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;

namespace ScottBrady91.IdentityModel.Tokens
{
	public class X509SecurityToken : SecurityToken, IDisposable
	{
	    public override string Id { get; }

        private ReadOnlyCollection<SecurityKey> keys;
        private X509Certificate2 certificate;
	    private bool disposed = false;
	    private bool disposable;

        public X509SecurityToken(X509Certificate2 certificate)
	        : this(certificate, SecurityUniqueId.Create().Value)
	    {
	    }

	    public X509SecurityToken(X509Certificate2 certificate, string id)
	        : this(certificate, id, true)
	    {
	    }

	    internal X509SecurityToken(X509Certificate2 certificate, bool clone)
	        : this(certificate, SecurityUniqueId.Create().Value, clone)
	    {
	    }

	    internal X509SecurityToken(X509Certificate2 certificate, bool clone, bool disposable)
	        : this(certificate, SecurityUniqueId.Create().Value, clone, disposable)
	    {
	    }

	    internal X509SecurityToken(X509Certificate2 certificate, string id, bool clone)
	        : this(certificate, id, clone, true)
	    {
	    }

	    internal X509SecurityToken(X509Certificate2 certificate, string id, bool clone, bool disposable)
	    {
	        if (certificate == null) throw new ArgumentNullException(nameof(certificate));

	        Id = id ?? throw new ArgumentNullException(nameof(id));
	        this.certificate = clone ? new X509Certificate2(certificate) : certificate;
	        this.disposable = clone || disposable;
	    }

	    public override ReadOnlyCollection<SecurityKey> SecurityKeys
	    {
	        get
	        {
	            CheckDisposed();

	            if (keys == null) keys = new ReadOnlyCollection<SecurityKey>(new List<SecurityKey> {new X509AsymmetricSecurityKey(Certificate)}.AsReadOnly());
	            return keys;
	        }
	    }

	    public override DateTime ValidFrom
	    {
	        get
	        {
	            CheckDisposed();
	            /*if (this.effectiveTime == SecurityUtils.MaxUtcDateTime)
	                this.effectiveTime = this.certificate.NotBefore.ToUniversalTime();
	            return this.effectiveTime;*/

                return Certificate.NotBefore.ToUniversalTime();
	        }
	    }

	    public override DateTime ValidTo
	    {
	        get
	        {
	            CheckDisposed();
	            /*if (this.expirationTime == SecurityUtils.MinUtcDateTime)
	                this.expirationTime = this.certificate.NotAfter.ToUniversalTime();*/

                return Certificate.NotAfter.ToUniversalTime();
	        }
	    }

	    public override bool CanCreateKeyIdentifierClause<T>()
	    {
	        CheckDisposed();

	        var t = typeof(T);
	        return t == typeof(X509RawDataKeyIdentifierClause)
	               || t == typeof(X509IssuerSerialKeyIdentifierClause)
	               // || t == typeof(X509ThumbprintKeyIdentifierClause);
	               || base.CanCreateKeyIdentifierClause<T>();
	    }

	    public override T CreateKeyIdentifierClause<T>()
	    {
	        CheckDisposed();

	        Type t = typeof(T);
	        if (t == typeof(X509RawDataKeyIdentifierClause))
	        {
	            return (T)(object)new X509RawDataKeyIdentifierClause(certificate);
	        }
	        if (t == typeof(X509IssuerSerialKeyIdentifierClause))
	        {
	            return (T)(object)new X509IssuerSerialKeyIdentifierClause(certificate);
	        }
	        throw new NotSupportedException($"A key identifier of type {t} could not be created");
	    }


        public X509Certificate2 Certificate
		{
			get
			{
				CheckDisposed();
				return certificate;
			}
		}

        
		void CheckDisposed()
		{
			if (certificate == null)
			{
				throw new ObjectDisposedException("X509SecurityToken");
			}
		}



		public virtual void Dispose()
		{
			CheckDisposed();

			certificate.Dispose();
			certificate = null;
		}

		



		public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
		{
			CheckDisposed();

			// TODO:
			// LocalIdKeyIdentifierClause , X509ThumbprintKeyIdentifierClause , 
			// X509SubjectKeyIdentifierClause 
			if (keyIdentifierClause is X509IssuerSerialKeyIdentifierClause isk)
			{
				return isk.Matches(certificate);
			}
			if (keyIdentifierClause is X509RawDataKeyIdentifierClause rdk)
			{
				return rdk.Matches(certificate);
			}
			return false;
		}
	}
}

