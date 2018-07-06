using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScottBrady91.IdentityModel.Tokens
{
    public class SecurityKeyIdentifier : IEnumerable<SecurityKeyIdentifierClause>
    {
        private readonly List<SecurityKeyIdentifierClause> clauses;

        public bool IsReadOnly { get; private set; }

        public int Count => clauses.Count;
        public void MakeReadOnly() => IsReadOnly = true;
        public bool CanCreateKey => clauses.Exists(clause => clause.CanCreateKey);

        public SecurityKeyIdentifierClause this[int index] => this.clauses[index];
        public void Add(SecurityKeyIdentifierClause clause)
        {
            if (IsReadOnly) throw new InvalidOperationException("SecurityKeyIdentifier is read only");
            if (clause == null) throw new ArgumentNullException(nameof(clause));
            clauses.Add(clause);
        }

        public SecurityKey CreateKey()
        {
            var clause = clauses.FirstOrDefault(x => x.CanCreateKey);
            if (clause == null) throw new NotSupportedException("SecurityKeyIdentifier does not support key creation");
            return clause.CreateKey();
        }

        public bool TryFind<TClause>(out TClause clause) where TClause : SecurityKeyIdentifierClause
        {
            clause = (TClause)clauses.FirstOrDefault(x => x is TClause);
            return clause != null;
        }

        public TClause Find<TClause>() where TClause : SecurityKeyIdentifierClause
        {
            if (!TryFind(out TClause clause)) throw new InvalidOperationException($"A clause of type ${typeof(TClause).Name} could not be found");
            return clause;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("SecurityKeyIdentifier(IsReadOnly = ");
            sb.Append(IsReadOnly);
            sb.Append(", Count = ");
            sb.Append(Count);

            sb.Append(", Clauses = [");
            for (int i = 0; i < clauses.Count; ++i)
            {
                if (i != 0)
                {
                    sb.Append(", ");
                }
                sb.Append(clauses[i]);
            }
            sb.Append("])");

            return sb.ToString();
        }

        public IEnumerator<SecurityKeyIdentifierClause> GetEnumerator() => clauses.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => clauses.GetEnumerator();
    }
}
