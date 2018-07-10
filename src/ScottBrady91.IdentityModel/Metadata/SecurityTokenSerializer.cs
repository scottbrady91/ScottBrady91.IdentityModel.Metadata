using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ScottBrady91.IdentityModel.Selectors;
using ScottBrady91.IdentityModel.Tokens;

using SecurityToken = Microsoft.IdentityModel.Tokens.SecurityToken;

namespace ScottBrady91.IdentityModel.Metadata
{
	public abstract class SecurityTokenSerializer
	{
		protected abstract bool CanReadTokenCore(XmlReader reader);
		protected abstract bool CanWriteTokenCore(SecurityToken token);
		protected abstract bool CanReadKeyIdentifierCore(XmlReader reader);
		protected abstract bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier);
		protected abstract bool CanReadKeyIdentifierClauseCore(XmlReader reader);
		protected abstract bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause);

		protected abstract SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver);
		protected abstract void WriteTokenCore(XmlWriter writer, SecurityToken token);
		protected abstract SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader);
		protected abstract void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier);
		protected abstract SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader);
		protected abstract void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause);
        
	    public bool CanReadToken(XmlReader reader)
	    {
	        if (reader == null) throw new ArgumentNullException(nameof(reader));
            return CanReadTokenCore(reader);
	    }

	    public bool CanWriteToken(SecurityToken token)
	    {
	        if (token == null) throw new ArgumentNullException(nameof(token));
	        return CanWriteTokenCore(token);
	    }

        public bool CanReadKeyIdentifier(XmlReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return CanReadKeyIdentifierCore(reader);
        }

	    public bool CanWriteKeyIdentifier(SecurityKeyIdentifier keyIdentifier)
	    {
	        if (keyIdentifier == null) throw new ArgumentNullException(nameof(keyIdentifier));
	        return CanWriteKeyIdentifierCore(keyIdentifier);
	    }

        public bool CanReadKeyIdentifierClause(XmlReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return CanReadKeyIdentifierClauseCore(reader);
        }

		public bool CanWriteKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
		{
		    if (keyIdentifierClause == null) throw new ArgumentNullException(nameof(keyIdentifierClause));
		    return CanWriteKeyIdentifierClauseCore(keyIdentifierClause);
		}

	    public SecurityToken ReadToken(XmlReader reader, SecurityTokenResolver resolver)
	    {
	        if (resolver == null) throw new ArgumentNullException(nameof(resolver));
	        return ReadTokenCore(reader, resolver);
	    }

	    public void WriteToken(XmlWriter writer, SecurityToken token)
	    {
	        if (writer == null) throw new ArgumentNullException(nameof(writer));
	        if (token == null) throw new ArgumentNullException(nameof(token));
	        WriteTokenCore(writer, token);
	    }

        public SecurityKeyIdentifier ReadKeyIdentifier(XmlReader reader)
		{
		    if (reader == null) throw new ArgumentNullException(nameof(reader));
			return ReadKeyIdentifierCore(reader);
		}

	    public void WriteKeyIdentifier(XmlWriter writer, SecurityKeyIdentifier keyIdentifier)
	    {
	        if (writer == null) throw new ArgumentNullException(nameof(writer));
	        if (keyIdentifier == null) throw new ArgumentNullException(nameof(keyIdentifier));
	        WriteKeyIdentifierCore(writer, keyIdentifier);
	    }

        public SecurityKeyIdentifierClause ReadKeyIdentifierClause(XmlReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return ReadKeyIdentifierClauseCore(reader);
        }

		public void WriteKeyIdentifierClause(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
		{
		    if (writer == null) throw new ArgumentNullException(nameof(writer));
		    if (keyIdentifierClause == null) throw new ArgumentNullException(nameof(keyIdentifierClause));
		    WriteKeyIdentifierClauseCore(writer, keyIdentifierClause);
		}

	    internal abstract class KeyIdentifierClauseEntry
	    {
            protected abstract XmlDictionaryString LocalName { get; }
	        protected abstract XmlDictionaryString NamespaceUri { get; }

	        public virtual bool CanReadKeyIdentifierClauseCore(XmlDictionaryReader reader) => reader.IsStartElement(LocalName, NamespaceUri);
            public abstract SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader);
            public abstract bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause);
            public abstract void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause);
	    }

        internal abstract class StrEntry
        {
            public abstract string GetTokenTypeUri();
            public abstract Type GetTokenType(SecurityKeyIdentifierClause clause);
            public abstract bool CanReadClause(XmlDictionaryReader reader, string tokenType);
            public abstract SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNonce, int derivationLength, string tokenType);
            public abstract bool SupportsCore(SecurityKeyIdentifierClause clause);
            public abstract void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause);
        }

        internal abstract class SerializerEntries
        {
            public virtual void PopulateTokenEntries(IList<TokenEntry> tokenEntries) { }
            public virtual void PopulateKeyIdentifierEntries(IList<KeyIdentifierEntry> keyIdentifierEntries) { }
            public virtual void PopulateKeyIdentifierClauseEntries(IList<KeyIdentifierClauseEntry> keyIdentifierClauseEntries) { }
            public virtual void PopulateStrEntries(IList<StrEntry> strEntries) { }
        }

        internal abstract class KeyIdentifierEntry
        {
            protected abstract XmlDictionaryString LocalName { get; }
            protected abstract XmlDictionaryString NamespaceUri { get; }

            public virtual bool CanReadKeyIdentifierCore(XmlDictionaryReader reader) => reader.IsStartElement(LocalName, NamespaceUri);
            public abstract SecurityKeyIdentifier ReadKeyIdentifierCore(XmlDictionaryReader reader);
            public abstract bool SupportsCore(SecurityKeyIdentifier keyIdentifier);
            public abstract void WriteKeyIdentifierCore(XmlDictionaryWriter writer, SecurityKeyIdentifier keyIdentifier);
        }

        internal abstract class TokenEntry
        {
            private Type[] tokenTypes;

            protected abstract XmlDictionaryString LocalName { get; }
            protected abstract XmlDictionaryString NamespaceUri { get; }
            public Type TokenType => GetTokenTypes()[0];
            public abstract string TokenTypeUri { get; }
            protected abstract string ValueTypeUri { get; }

            public bool SupportsCore(Type tokenType) => GetTokenTypes().Any(t => t.IsAssignableFrom(tokenType));
            protected abstract Type[] GetTokenTypesCore();
            public Type[] GetTokenTypes() => tokenTypes ?? (tokenTypes = GetTokenTypesCore());
            public virtual bool SupportsTokenTypeUri(string tokenTypeUri) => (TokenTypeUri == tokenTypeUri);

        }
    }
}
