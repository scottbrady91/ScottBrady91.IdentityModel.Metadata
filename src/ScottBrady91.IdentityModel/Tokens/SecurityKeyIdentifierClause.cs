using Microsoft.IdentityModel.Tokens;
using System;

namespace ScottBrady91.IdentityModel.Tokens
{
    public abstract class SecurityKeyIdentifierClause
    {
        private readonly byte[] derivationNonce;
        
        public string Id { get; set; }
        public string ClauseType { get; }
		public int DerivationLength { get; }

        protected SecurityKeyIdentifierClause(string clauseType, byte[] nonce = null, int length = 0)
        {
            ClauseType = clauseType;
            DerivationLength = length;
            derivationNonce = nonce;
        }

        public byte[] GetDerivationNonce() => derivationNonce?.CloneByteArray();
        public virtual bool CanCreateKey => false;
        public virtual SecurityKey CreateKey() => throw new NotSupportedException("SecurityKeyIdentifierClause does not support key creation");
        public virtual bool Matches(SecurityKeyIdentifierClause keyIdentifierClause) => ReferenceEquals(this, keyIdentifierClause);
	}
}
