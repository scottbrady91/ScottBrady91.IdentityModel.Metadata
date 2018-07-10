using ScottBrady91.IdentityModel.Tokens;

namespace ScottBrady91.IdentityModel.Selectors
{
    public class NullSecurityTokenResolver : SecurityTokenResolver
    {
		private NullSecurityTokenResolver()
		{
		}

		protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
		{
			token = null;
			return false;
		}

		protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifier, out SecurityToken token)
		{
			token = null;
			return false;
		}

		protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifier, out SecurityKey key)
		{
			key = null;
			return false;
		}

		public static SecurityTokenResolver Instance { get; } = new NullSecurityTokenResolver();
	}
}
