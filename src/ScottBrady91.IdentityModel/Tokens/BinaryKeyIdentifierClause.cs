using Microsoft.IdentityModel.Tokens;
using System;

namespace ScottBrady91.IdentityModel.Tokens
{
	public abstract class BinaryKeyIdentifierClause : SecurityKeyIdentifierClause
	{
		private readonly byte[] identificationData;
		private readonly bool cloneBuffer;

		protected BinaryKeyIdentifierClause(string clauseType, byte[] identificationData, bool cloneBuffer, byte[] derivationNonce = null, int derivationLength = 0) 
		    : base(clauseType, derivationNonce, derivationLength)
		{
		    if (identificationData == null) throw new ArgumentNullException(nameof(identificationData));
		    if (identificationData.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(identificationData));
            
		    this.cloneBuffer = cloneBuffer;
			this.identificationData = cloneBuffer ? identificationData.CloneByteArray() : identificationData;
		}

	    public byte[] GetBuffer()
		{
			return identificationData.CloneByteArray(); // TODO
		}

		public byte[] GetRawBuffer()
		{
			return cloneBuffer ? identificationData.CloneByteArray() : identificationData;
		}

		public bool Matches(byte[] data, int offset)
		{
			if (data.Length - offset != identificationData.Length)
			{
				return false;
			}
			for (int i = 0; i < identificationData.Length; ++i)
			{
				if (data[i + offset] != identificationData[i])
				{
					return false;
				}
			}
			return true;
		}

		public bool Matches(byte[] data)
		{
			return Matches(data, 0);
		}

		public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
		{
			return keyIdentifierClause is BinaryKeyIdentifierClause otherClause &&
				Matches(otherClause.identificationData);
		}
	}
}
