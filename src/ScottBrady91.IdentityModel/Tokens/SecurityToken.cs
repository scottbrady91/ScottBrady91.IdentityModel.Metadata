using System;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Tokens
{
    public abstract class SecurityToken
    {
        public abstract string Id { get; }
        public abstract DateTime ValidFrom { get; }
        public abstract DateTime ValidTo { get; }
        public abstract ReadOnlyCollection<SecurityKey> SecurityKeys { get; }

        public bool CanCreateLocalKeyIdentifierClause() => Id != null;

        public virtual bool CanCreateKeyIdentifierClause<T>() where T : SecurityKeyIdentifierClause
        {
            return typeof(T) == typeof(LocalIdKeyIdentifierClause) && CanCreateLocalKeyIdentifierClause();
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

        public virtual SecurityKey ResolveKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (SecurityKeys.Count != 0 && MatchesKeyIdentifierClause(keyIdentifierClause))
                return SecurityKeys[0];

            return null;

        }
    }
}