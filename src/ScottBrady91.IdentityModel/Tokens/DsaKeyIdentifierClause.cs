﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace ScottBrady91.IdentityModel.Tokens
{
	public class DsaSecurityKey : AsymmetricSecurityKey
	{
		DSA dsa;

		public DsaSecurityKey(DSA dsa)
		{
			this.dsa = dsa;
		}

		public override AsymmetricAlgorithm GetAsymmetricAlgorithm(string algorithm, bool privateKey)
		{
			if (algorithm == null)
			{
				throw new ArgumentNullException(nameof(algorithm));
			}

			switch (algorithm)
			{
				case SignedXml.XmlDsigDSAUrl:
					return dsa;
			}

			throw new NotSupportedException($"DsaSecurityKey does not support the algorithm '{algorithm}'");
		}

		public override HashAlgorithm GetHashAlgorithmForSignature(string algorithm)
		{
			if (algorithm == null)
			{
				throw new ArgumentNullException(nameof(algorithm));
			}
			switch (algorithm)
			{
				case SignedXml.XmlDsigDSAUrl:
					return new SHA1Managed();
			}
			throw new NotSupportedException($"The hash algorithm '{algorithm}' is not supported");
		}

		public override AsymmetricSignatureDeformatter GetSignatureDeformatter(string algorithm)
		{
			if (algorithm == null)
			{
				throw new ArgumentNullException(nameof(algorithm));
			}
			switch (algorithm)
			{
				case SignedXml.XmlDsigDSAUrl:
					return new DSASignatureDeformatter(dsa);
			}
			throw new NotSupportedException($"DsaSecurityKey does not support the signature algorithm '{algorithm}'");
		}

		public override AsymmetricSignatureFormatter GetSignatureFormatter(string algorithm)
		{
			if (algorithm == null)
			{
				throw new ArgumentNullException(nameof(algorithm));
			}
			switch (algorithm)
			{
				case SignedXml.XmlDsigDSAUrl:
					return new DSASignatureFormatter(dsa);
			}
			throw new NotSupportedException($"DsaSecurityKey does not support the signature algorithm '{algorithm}'");
		}

		public override int KeySize => dsa.KeySize;

		public override byte[] DecryptKey(string algorithm, byte[] keyData)
		{
			if (algorithm == null)
			{
				throw new ArgumentNullException(nameof(algorithm));
			}
			if (keyData == null)
			{
				throw new ArgumentNullException(nameof(keyData));
			}

			throw new NotSupportedException("DSA keys do not support encryption");
		}

		public override byte[] EncryptKey(string algorithm, byte[] keyData)
		{
			if (algorithm == null)
			{
				throw new ArgumentNullException(nameof(algorithm));
			}
			if (keyData == null)
			{
				throw new ArgumentNullException(nameof(keyData));
			}

			throw new NotSupportedException("DSA keys do not support encryption");
		}

		public override bool IsAsymmetricAlgorithm(string algorithm)
		{
			if (algorithm == null)
			{
				throw new ArgumentNullException(nameof(algorithm));
			}
			switch (algorithm)
			{
				case SecurityAlgorithms.DsaSha1Signature:
					return true;
			}
			return false;
		}

		public override bool IsSupportedAlgorithm(string algorithm)
		{
			if (algorithm == null)
			{
				throw new ArgumentNullException(nameof(algorithm));
			}
			switch (algorithm)
			{
				case SecurityAlgorithms.DsaSha1Signature:
					return true;
			}
			return false;
		}

		public override bool IsSymmetricAlgorithm(string algorithm)
		{
			if (algorithm == null)
			{
				throw new ArgumentNullException(nameof(algorithm));
			}
			switch (algorithm)
			{
				case SecurityAlgorithms.HmacSha1Signature:
				case SecurityAlgorithms.Psha1KeyDerivation:
				case SecurityAlgorithms.Aes128Encryption:
				case SecurityAlgorithms.Aes128KeyWrap:
				case SecurityAlgorithms.Aes192Encryption:
				case SecurityAlgorithms.Aes192KeyWrap:
				case SecurityAlgorithms.Aes256Encryption:
				case SecurityAlgorithms.Aes256KeyWrap:
				case SecurityAlgorithms.TripleDesEncryption:
				case SecurityAlgorithms.TripleDesKeyWrap:
				case SecurityAlgorithms.DesEncryption:
					return true;
			}
			return false;
		}

		public override bool HasPrivateKey()
		{
			return true;
		}
	}

	public class DsaKeyIdentifierClause : SecurityKeyIdentifierClause
    {
		readonly DSAParameters parameters;
		DsaSecurityKey key;
		DSA dsa;

		public DSA Dsa
		{
			get
			{
				if (dsa == null)
				{
					dsa = DSA.Create();
					dsa.ImportParameters(parameters);
				}
				return dsa;
			}
		}


		public DsaKeyIdentifierClause(DSAParameters parameters) :
			base(null)
		{
			this.parameters = parameters;
		}

		public DsaKeyIdentifierClause(DSA dsa) :
			base(null)
		{
			this.dsa = dsa;
			this.parameters = dsa.ExportParameters(false);
		}

		public override bool CanCreateKey => true;

		public override SecurityKey CreateKey()
		{
			if (key == null)
			{
				key = new DsaSecurityKey(Dsa);
			}
			return key;
		}

		public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
		{
			if (keyIdentifierClause == null)
			{
				throw new ArgumentNullException(nameof(keyIdentifierClause));
			}
			return keyIdentifierClause is DsaKeyIdentifierClause otherClause &&
				Matches(otherClause.parameters);
		}

		public bool Matches(DSAParameters p2)
		{
			var p1 = parameters;
			return p1.Counter == p2.Counter &&
				ByteArraysEqual(p1.G, p2.G) &&
				ByteArraysEqual(p1.J, p2.J) &&
				ByteArraysEqual(p1.P, p2.P) &&
				ByteArraysEqual(p1.Q, p2.Q) &&
				ByteArraysEqual(p1.X, p2.X) &&
				ByteArraysEqual(p1.Y, p2.Y) &&
				ByteArraysEqual(p1.Seed, p2.Seed);
		}

        public static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null)
            {
                return b == null;
            }
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
