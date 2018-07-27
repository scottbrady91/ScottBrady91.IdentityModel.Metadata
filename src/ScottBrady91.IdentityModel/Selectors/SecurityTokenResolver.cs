using System;
using System.Xml;
using ScottBrady91.IdentityModel.Tokens;

namespace ScottBrady91.IdentityModel.Selectors
{
    public abstract class SecurityTokenResolver
    {
        public SecurityToken ResolveToken(SecurityKeyIdentifier keyIdentifier)
        {
            if (keyIdentifier == null) throw new ArgumentNullException(nameof(keyIdentifier));
            if (!TryResolveTokenCore(keyIdentifier, out var token)) throw new InvalidOperationException($"Unable to resolve token for key identifier {keyIdentifier}");
            
            return token;
        }

        public bool TryResolveToken(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
        {
            if (keyIdentifier == null) throw new ArgumentNullException(nameof(keyIdentifier));
            return TryResolveTokenCore(keyIdentifier, out token);
        }

        public SecurityToken ResolveToken(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (keyIdentifierClause == null) throw new ArgumentNullException(nameof(keyIdentifierClause));
            if (!TryResolveTokenCore(keyIdentifierClause, out var token)) throw new InvalidOperationException($"Unable to resolve token for key identifier clause {keyIdentifierClause}");

            return token;
        }

        public bool TryResolveToken(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            if (keyIdentifierClause == null) throw new ArgumentNullException(nameof(keyIdentifierClause));
            return TryResolveTokenCore(keyIdentifierClause, out token);
        }

        public SecurityKey ResolveSecurityKey(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (keyIdentifierClause == null) throw new ArgumentNullException(nameof(keyIdentifierClause));
            if (!TryResolveSecurityKeyCore(keyIdentifierClause, out var key)) throw new InvalidOperationException($"Unable to resolve key for key identifier clause {keyIdentifierClause}");

            return key;
        }

        public bool TryResolveSecurityKey(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
        {
            if (keyIdentifierClause == null) throw new ArgumentNullException(nameof(keyIdentifierClause));

            return TryResolveSecurityKeyCore(keyIdentifierClause, out key);
        }

        public virtual void LoadCustomConfiguration(XmlNodeList nodeList)
        {
            throw new NotImplementedException();
        }

        protected abstract bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token);
        protected abstract bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifier, out SecurityToken token);
        protected abstract bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifier, out SecurityKey key);
    }
}