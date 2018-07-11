using System;
using System.Security.Cryptography.X509Certificates;

namespace ScottBrady91.IdentityModel.Tokens
{
	public class X509RawDataKeyIdentifierClause : BinaryKeyIdentifierClause
    {
		private X509Certificate2 certificate;

        public X509RawDataKeyIdentifierClause(X509Certificate2 certificate)
            : this(GetRawData(certificate), false)
        {
            this.certificate = certificate;
        }

        public X509RawDataKeyIdentifierClause(byte[] certificateRawData)
            : this(certificateRawData, true)
        {
        }

        internal X509RawDataKeyIdentifierClause(byte[] certificateRawData, bool cloneBuffer)
            : base(null, certificateRawData, cloneBuffer)
        {
        }

        public override bool CanCreateKey => true;

        public override SecurityKey CreateKey()
        {
            if (certificate == null) certificate = new X509Certificate2(GetX509RawData());
            return new X509AsymmetricSecurityKey(certificate);
        }

        private static byte[] GetRawData(X509Certificate certificate)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));
            return certificate.GetRawCertData();
        }

        public byte[] GetX509RawData()
		{
			return GetRawBuffer();
		}

		public bool Matches(X509Certificate2 otherCert)
		{
			return Matches(otherCert.RawData);
		}
    }
}
