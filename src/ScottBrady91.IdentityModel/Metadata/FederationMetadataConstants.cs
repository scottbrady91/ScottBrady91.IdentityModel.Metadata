namespace ScottBrady91.IdentityModel.Metadata
{
    internal static class FederationMetadataConstants
    {
        public const string Namespace = "http://docs.oasis-open.org/wsfed/federation/200706";
        public const string Prefix = "fed";

        public static class Elements
        {
            public const string ClaimTypesOffered = "ClaimTypesOffered";
            public const string ClaimTypesRequested = "ClaimTypesRequested";
            public const string TargetScopes = "TargetScopes";
            public const string TokenTypesOffered = "TokenTypesOffered";

            public const string ApplicationServiceType = "ApplicationServiceType";
            public const string SecurityTokenServiceType = "SecurityTokenServiceType";

            public const string ApplicationServiceEndpoint = "ApplicationServiceEndpoint";
            public const string PassiveRequestorEndpoint = "PassiveRequestorEndpoint";
            public const string SecurityTokenServiceEndpoint = "SecurityTokenServiceEndpoint";
        }

    }
}