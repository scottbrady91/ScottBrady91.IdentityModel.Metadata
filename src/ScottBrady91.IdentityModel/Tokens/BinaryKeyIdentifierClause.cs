using Microsoft.IdentityModel.Tokens;
using System;

namespace ScottBrady91.IdentityModel.Tokens
{
	public abstract class BinaryKeyIdentifierClause : SecurityKeyIdentifierClause
	{
		private readonly byte[] identificationData;

		protected BinaryKeyIdentifierClause(string clauseType, byte[] identificationData, bool cloneBuffer, byte[] derivationNonce = null, int derivationLength = 0) 
		    : base(clauseType, derivationNonce, derivationLength)
		{
		    if (identificationData == null) throw new ArgumentNullException(nameof(identificationData));
		    if (identificationData.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(identificationData));
            
		    this.identificationData = cloneBuffer ? identificationData.CloneByteArray() : identificationData;
		}

	    public byte[] GetBuffer()
		{
			return SecurityUtils.CloneBuffer(identificationData);
		}

	    public byte[] GetRawBuffer() => identificationData;

		public bool Matches(byte[] data, int offset = 0)
		{
		    if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
		    return SecurityUtils.MatchesBuffer(identificationData, 0, data, offset);
        }

	    public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
		{
			return keyIdentifierClause is BinaryKeyIdentifierClause otherClause &&
				Matches(otherClause.identificationData);
		}
	}
}
