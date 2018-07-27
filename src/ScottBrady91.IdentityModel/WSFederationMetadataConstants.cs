namespace ScottBrady91.IdentityModel
{
    internal static class WSFederationMetadataConstants
    {
        public const string Namespace = "http://docs.oasis-open.org/wsfed/federation/200706";
        public const string Prefix = "fed";
        public const string WSTransferAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get";
        public const string WSTransferResponseAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse";
        public const string FederationMetadataHandler = "FederationMetadataHandler";

        public static class Attributes
        {
            public const string RealmName = "RealmName";
            public const string FederationId = "FederationID";
            public const string Uri = "Uri";
            public const string Optional = "Optional";
        }

        public static class Elements
        {
            public const string AttributeServiceEndpoint = "AttributeServiceEndpoint";
            public const string AutomaticPseudonyms = "AutomaticPseudonyms";
            public const string ClaimTypesOffered = "ClaimTypesOffered";
            public const string Federation = "Federation";
            public const string FederationMetadata = "FederationMetadata";
            public const string IssuerName = "IssuerName";
            public const string IssuerNamesOffered = "IssuerNamesOffered";
            public const string MetadataReference = "MetadataReference";
            public const string PassiveRequestorEndpoints = "PassiveRequestorEndpoints";
            public const string PseudonymServiceEndpoint = "PseudonymServiceEndpoint";
            public const string SingleSignoutNotificationEndpoint = "SingleSignoutNotificationEndpoint";
            public const string SingleSignOutSubscriptionEndpoint = "SingleSignOutSubscriptionEndpoint";
            public const string TokenIssuerEndpoints = "TokenIssuerEndpoints";
            public const string TokenIssuerName = "TokenIssuerName";
            public const string TokenKeyTransferKeyInfo = "TokenKeyTransferKeyInfo";
            public const string TokenSigningKeyInfo = "TokenSigningKeyInfo";
            public const string TokenType = "TokenType";
            public const string TokenTypesOffered = "TokenTypesOffered";
        }
    }
}