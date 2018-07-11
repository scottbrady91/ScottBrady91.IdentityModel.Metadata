using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace ScottBrady91.IdentityModel.Tokens
{
    public abstract class SecurityToken
    {
        public abstract string Id { get; }
        public abstract ReadOnlyCollection<SecurityKey> SecurityKeys { get; }
        public abstract DateTime ValidFrom { get; }
        public abstract DateTime ValidTo { get; }

        public virtual bool CanCreateKeyIdentifierClause<T>() where T : SecurityKeyIdentifierClause
        {
            return ((typeof(T) == typeof(LocalIdKeyIdentifierClause)) && CanCreateLocalKeyIdentifierClause());
        }

        public virtual T CreateKeyIdentifierClause<T>() where T : SecurityKeyIdentifierClause
        {
            if ((typeof(T) == typeof(LocalIdKeyIdentifierClause)) && CanCreateLocalKeyIdentifierClause())
                return new LocalIdKeyIdentifierClause(Id, GetType()) as T;

            throw new NotSupportedException();
        }

        public virtual bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (keyIdentifierClause is LocalIdKeyIdentifierClause localKeyIdentifierClause)
                return localKeyIdentifierClause.Matches(Id, GetType());

            return false;
        }

        public virtual SecurityKey ResolveKeyIdentifierClause(
            SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (SecurityKeys.Count != 0 && MatchesKeyIdentifierClause(keyIdentifierClause))
                return SecurityKeys[0];

            return null;
        }

        private bool CanCreateLocalKeyIdentifierClause()
        {
            return (Id != null);
        }
    }

    public class LocalIdKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        private readonly Type[] ownerTypes;

        public string LocalId { get; }
        public Type OwnerType => ownerTypes == null || ownerTypes.Length == 0 ? null : ownerTypes[0];

        public LocalIdKeyIdentifierClause(string localId)
            : this(localId, (Type[])null)
        {
        }

        public LocalIdKeyIdentifierClause(string localId, Type ownerType)
            : this(localId, ownerType == null ? null : new[] { ownerType })
        {
        }

        public LocalIdKeyIdentifierClause(string localId, byte[] derivationNonce, int derivationLength, Type ownerType)
            : this(null, derivationNonce, derivationLength, ownerType == null ? null : new[] { ownerType })
        {
        }

        internal LocalIdKeyIdentifierClause(string localId, Type[] ownerTypes)
            : this(localId, null, 0, ownerTypes)
        {
        }

        internal LocalIdKeyIdentifierClause(string localId, byte[] derivationNonce, int derivationLength, Type[] ownerTypes)
            : base(null, derivationNonce, derivationLength)
        {
            if (string.IsNullOrEmpty(localId)) throw new ArgumentException("Value cannot be null or empty.", nameof(localId));

            LocalId = localId;
            this.ownerTypes = ownerTypes;
        }
        

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            var that = keyIdentifierClause as LocalIdKeyIdentifierClause;

            return ReferenceEquals(this, that) || (that != null && that.Matches(LocalId, OwnerType));
        }

        public bool Matches(string localId, Type ownerType)
        {
            if (string.IsNullOrEmpty(localId)) return false;
            if (LocalId != localId) return false;
            if (ownerTypes == null || ownerType == null) return true;

            return ownerTypes.Any(t => t == null || t == ownerType);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "LocalIdKeyIdentifierClause(LocalId = '{0}', Owner = '{1}')", LocalId, OwnerType);
        }
    }
}