using System;
using System.Linq;

namespace ScottBrady91.IdentityModel.Tokens
{
    public class LocalIdKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        private readonly Type[] ownerTypes;

        public string LocalId { get; }
        public Type OwnerType => (ownerTypes == null || ownerTypes.Length == 0) ? null : ownerTypes[0];

        public LocalIdKeyIdentifierClause(string localId, Type ownerType) : this(localId, (Type[])null) { }
        public LocalIdKeyIdentifierClause(string localId, byte[] derivationNonce, int derivationLength, Type ownerType) : this(localId, ownerType == null ? (Type[])null : new Type[] { ownerType }) { }
        internal LocalIdKeyIdentifierClause(string localId, Type[] ownerTypes) : this(localId, null, 0, ownerTypes) { }
        internal LocalIdKeyIdentifierClause(string localId, byte[] derivationNonce, int derivationLength, Type[] ownerTypes)
            : base(null, derivationNonce, derivationLength)
        {
            LocalId = localId ?? throw new ArgumentNullException(nameof(localId));
            this.ownerTypes = ownerTypes ?? throw new ArgumentNullException(nameof(ownerTypes));
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            var that = keyIdentifierClause as LocalIdKeyIdentifierClause;
            return ReferenceEquals(this, that) || that != null && that.Matches(LocalId, OwnerType);
        }

        public bool Matches(string localId, Type ownerType)
        {
            if (string.IsNullOrEmpty(localId)) return false;
            if (LocalId != localId) return false;
            if (ownerTypes == null || ownerType == null) return true;

            return ownerTypes.Any(t => t == null || t == ownerType);
        }
    }
}