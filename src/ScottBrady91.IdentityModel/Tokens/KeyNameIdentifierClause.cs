using System;

namespace ScottBrady91.IdentityModel.Tokens
{
    public class KeyNameIdentifierClause : SecurityKeyIdentifierClause
    {
        public string KeyName { get; }

        public KeyNameIdentifierClause(string keyName) : base(null)
		{
			KeyName = keyName;
		}

		public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
		{
			if (keyIdentifierClause == null) throw new ArgumentNullException(nameof(keyIdentifierClause));
			return keyIdentifierClause is KeyNameIdentifierClause otherClause && Matches(otherClause.KeyName);
		}

		public bool Matches(string keyName) => KeyName == keyName;
        public override string ToString() => $"KeyNameIdentifierClause(KeyName = '{KeyName}')";
    }
}
