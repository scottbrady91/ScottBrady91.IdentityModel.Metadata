using System;
using System.Security.Cryptography.X509Certificates;

namespace ScottBrady91.IdentityModel.Tokens
{
    public class X509IssuerSerialKeyIdentifierClause : SecurityKeyIdentifierClause
    {
		public string IssuerName { get; }
		public string IssuerSerialNumber { get; }

        public X509IssuerSerialKeyIdentifierClause(string issuerName, string issuerSerialNumber) : base(null)
        {
            if (string.IsNullOrEmpty(issuerName)) throw new ArgumentException("Value cannot be null or empty.", nameof(issuerName));
            if (string.IsNullOrEmpty(issuerSerialNumber)) throw new ArgumentException("Value cannot be null or empty.", nameof(issuerSerialNumber));

            IssuerName = issuerName;
            IssuerSerialNumber = issuerSerialNumber;
        }

        public X509IssuerSerialKeyIdentifierClause(X509Certificate2 certificate) : base(null)
		{
			if (certificate == null) throw new ArgumentNullException(nameof(certificate));
			
			IssuerName = certificate.IssuerName.Name;
            // TODO: Serial Number - hexadecimal vs decimal
			IssuerSerialNumber = Asn1IntegerConverter.Asn1IntegerToDecimalString(certificate.GetSerialNumber());
		}
        
		public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
		{
			if (keyIdentifierClause == null) throw new ArgumentNullException(nameof(keyIdentifierClause));
			return keyIdentifierClause is X509IssuerSerialKeyIdentifierClause otherClause 
			       && Matches(otherClause.IssuerName, otherClause.IssuerSerialNumber);
		}

		public bool Matches(string issuerName, string issuerSerialNumber)
		{
            // TODO: X500DistinguishedName match?
            return IssuerName == issuerName && IssuerSerialNumber == issuerSerialNumber;
		}

		public bool Matches(X509Certificate2 certificate)
		{
		    if (certificate == null) return false;
		    return Matches(certificate.IssuerName.Name, Asn1IntegerConverter.Asn1IntegerToDecimalString(certificate.GetSerialNumber()));
		}
	}
}
