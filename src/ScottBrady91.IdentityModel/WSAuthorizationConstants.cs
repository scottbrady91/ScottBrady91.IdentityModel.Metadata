namespace ScottBrady91.IdentityModel
{
    internal static class WSAuthorizationConstants
    {
        public const string Prefix = "auth";
        public const string Namespace = "http://docs.oasis-open.org/wsfed/authorization/200706";
        public const string Dialect = Namespace + "/authclaims";
        public const string Action = Namespace + "/claims/action";

        public static class Attributes
        {
            public const string Name = "Name";
            public const string Scope = "Scope";
        }

        public static class Elements
        {
            public const string AdditionalContext = "AdditionalContext";
            public const string ClaimType = "ClaimType";
            public const string ContextItem = "ContextItem";
            public const string Description = "Description";
            public const string DisplayName = "DisplayName";
            public const string Value = "Value";
        }
    }
}