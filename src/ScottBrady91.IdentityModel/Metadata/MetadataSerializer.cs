using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.IdentityModel.Xml;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.IdentityModel.Tokens;
using static System.String;
using static Microsoft.IdentityModel.Logging.LogHelper;

// ReSharper disable VirtualMemberNeverOverridden
namespace ScottBrady91.IdentityModel.Metadata
{
    public class MetadataSerializer
    {
        private const string LanguageNamespaceUri = "http://www.w3.org/XML/1998/namespace";

        public SecurityTokenSerializer SecurityTokenSerializer { get; }

        public MetadataSerializer()
        {
        } // TODO: Default Key Serializer

        public MetadataSerializer(SecurityTokenSerializer serializer)
        {
            SecurityTokenSerializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        protected virtual void WriteCustomAttributes<T>(XmlWriter writer, T source)
        {
        }

        protected virtual void WriteCustomElements<T>(XmlWriter writer, T source)
        {
        }

        protected virtual void WriteApplicationServiceDescriptor(XmlWriter writer, ApplicationServiceDescriptor appService)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (appService == null) throw new ArgumentNullException(nameof(appService));

            writer.WriteStartElement(Saml2MetadataConstants.Elements.RoleDescriptor, Saml2MetadataConstants.Namespace);
            writer.WriteAttributeString("xsi", "type", XmlSchema.InstanceNamespace,
                FederationMetadataConstants.Prefix + ":" + FederationMetadataConstants.Elements.ApplicationServiceType);
            writer.WriteAttributeString("xmlns", FederationMetadataConstants.Prefix, null, FederationMetadataConstants.Namespace);

            WriteWebServiceDescriptorAttributes(writer, appService);
            WriteCustomAttributes(writer, appService);

            WriteWebServiceDescriptorElements(writer, appService);

            foreach (var endpoint in appService.Endpoints)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.ApplicationServiceEndpoint, FederationMetadataConstants.Namespace);
                WriteEndpointReference(writer, endpoint);
                writer.WriteEndElement();
            }

            foreach (var endpoint in appService.PassiveRequestorEndpoints)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.PassiveRequestorEndpoint, FederationMetadataConstants.Namespace);
                WriteEndpointReference(writer, endpoint);
                writer.WriteEndElement();
            }

            WriteCustomElements(writer, appService);

            writer.WriteEndElement();
        }

        protected virtual void WriteContactPerson(XmlWriter writer, ContactPerson contactPerson)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (contactPerson == null) throw new ArgumentNullException(nameof(contactPerson));

            writer.WriteStartElement(Saml2MetadataConstants.Elements.ContactPerson, Saml2MetadataConstants.Namespace);
            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.ContactType, null, ContactTypeHelpers.ToString(contactPerson.Type));
            WriteCustomAttributes(writer, contactPerson);

            writer.WriteElementIfPresent(Saml2MetadataConstants.Elements.Company, Saml2MetadataConstants.Namespace, contactPerson.Company);
            writer.WriteElementIfPresent(Saml2MetadataConstants.Elements.GivenName, Saml2MetadataConstants.Namespace, contactPerson.GivenName);
            writer.WriteElementIfPresent(Saml2MetadataConstants.Elements.Surname, Saml2MetadataConstants.Namespace, contactPerson.Surname);

            foreach (var email in contactPerson.EmailAddresses)
                writer.WriteElementString(Saml2MetadataConstants.Elements.EmailAddress, Saml2MetadataConstants.Namespace, email);
            foreach (var phone in contactPerson.TelephoneNumbers)
                writer.WriteElementString(Saml2MetadataConstants.Elements.TelephoneNumber, Saml2MetadataConstants.Namespace, phone);

            WriteCustomElements(writer, contactPerson);

            writer.WriteEndElement();
        }

        protected virtual void WriteProtocolEndpoint(XmlWriter writer, ProtocolEndpoint endpoint, string name, string ns)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (ns == null) throw new ArgumentNullException(nameof(ns));

            writer.WriteStartElement(name, ns);

            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Binding,
                endpoint.Binding.IsAbsoluteUri ? endpoint.Binding.AbsoluteUri : endpoint.Binding.ToString());
            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Location,
                endpoint.Location.IsAbsoluteUri ? endpoint.Location.AbsoluteUri : endpoint.Location.ToString());

            writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.ResponseLocation, null, endpoint.ResponseLocation);

            WriteCustomAttributes(writer, endpoint);

            WriteCustomElements(writer, endpoint);
            writer.WriteEndElement();
        }

        protected virtual void WriteDisplayClaim(XmlWriter writer, DisplayClaim claim)
        {
            writer.WriteStartElement(WSAuthorizationConstants.Prefix, WSAuthorizationConstants.Elements.ClaimType,
                WSAuthorizationConstants.Namespace);

            if (IsNullOrEmpty(claim.ClaimType)) throw new MetadataSerializationException("Missing ClaimType");
            if (!Uri.TryCreate(claim.ClaimType, UriKind.Absolute, out _))
                throw new MetadataSerializationException("Invlaud ClaimtType - must be valid URI");

            writer.WriteAttributeString(WSFederationMetadataConstants.Attributes.Uri, claim.ClaimType);

            if (claim.WriteOptionalAttribute) writer.WriteAttributeIfPresent(WSFederationMetadataConstants.Attributes.Optional, null, claim.Optional);

            writer.WriteElementIfPresent(WSAuthorizationConstants.Prefix, WSAuthorizationConstants.Elements.DisplayName,
                WSAuthorizationConstants.Namespace, claim.DisplayName);
            writer.WriteElementIfPresent(WSAuthorizationConstants.Prefix, WSAuthorizationConstants.Elements.Description,
                WSAuthorizationConstants.Namespace, claim.Description);

            writer.WriteEndElement();
        }

        protected virtual void WriteEntitiesDescriptor(XmlWriter writer, EntitiesDescriptor entitiesDescriptor)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (entitiesDescriptor == null) throw new ArgumentNullException(nameof(entitiesDescriptor));
            if (!entitiesDescriptor.ChildEntities.Any() && !entitiesDescriptor.ChildEntityGroups.Any())
                throw new ArgumentNullException(nameof(entitiesDescriptor));

            var entityReference = "_" + Guid.NewGuid();
            if (entitiesDescriptor.SigningCredentials != null)
                writer = new EnvelopedSignatureWriter(writer, entitiesDescriptor.SigningCredentials, entityReference);
            
            writer.WriteStartElement(Saml2MetadataConstants.Elements.EntitiesDescriptor, Saml2MetadataConstants.Namespace);
            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Id, null, entityReference);

            foreach (var entity in entitiesDescriptor.ChildEntities)
            {
                if (!IsNullOrEmpty(entity.FederationId))
                {
                    if (!StringComparer.Ordinal.Equals(entity.FederationId, entitiesDescriptor.Name))
                    {
                        throw new MetadataSerializationException($"Invalid federation ID of {entity.FederationId}");
                    }
                }
            }

            writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.EntityGroupName, null, entitiesDescriptor.Name);

            WriteCustomAttributes(writer, entitiesDescriptor);

            foreach (var childEntity in entitiesDescriptor.ChildEntities)
            {
                WriteEntityDescriptor(writer, childEntity);
            }

            foreach (var childEntityDescriptor in entitiesDescriptor.ChildEntityGroups)
            {
                WriteEntitiesDescriptor(writer, childEntityDescriptor);
            }

            WriteCustomElements(writer, entitiesDescriptor);

            writer.WriteEndElement();
        }

        protected virtual void WriteEntityDescriptor(XmlWriter writer, EntityDescriptor entityDescriptor)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (entityDescriptor == null) throw new ArgumentNullException(nameof(entityDescriptor));
            if (!entityDescriptor.RoleDescriptors.Any()) throw new MetadataSerializationException("Missing RoleDescriptors");

            var entityReference = "_" + Guid.NewGuid();

            if (entityDescriptor.SigningCredentials != null)
                writer = new EnvelopedSignatureWriter(writer, entityDescriptor.SigningCredentials, entityReference);

            writer.WriteStartElement(Saml2MetadataConstants.Elements.EntityDescriptor, Saml2MetadataConstants.Namespace);
            writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.Id, null, entityReference);

            if (entityDescriptor.EntityId?.Id == null) throw new MetadataSerializationException("Missing entity id");
            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.EntityId, null, entityDescriptor.EntityId.Id);

            writer.WriteAttributeIfPresent(WSFederationMetadataConstants.Attributes.FederationId, WSFederationMetadataConstants.Namespace, entityDescriptor.FederationId);
            WriteCustomAttributes(writer, entityDescriptor);
            
            foreach (var roleDescriptor in entityDescriptor.RoleDescriptors)
            {
                if (roleDescriptor is ServiceProviderSingleSignOnDescriptor spSsoDescriptor)
                {
                    WriteServiceProviderSingleSignOnDescriptor(writer, spSsoDescriptor);
                }

                if (roleDescriptor is IdentityProviderSingleSignOnDescriptor idpSsoDescriptor)
                {
                    WriteIdentityProviderSingleSignOnDescriptor(writer, idpSsoDescriptor);
                }

                if (roleDescriptor is ApplicationServiceDescriptor serviceDescriptor)
                {
                    WriteApplicationServiceDescriptor(writer, serviceDescriptor);
                }

                if (roleDescriptor is SecurityTokenServiceDescriptor secDescriptor)
                {
                    WriteSecurityTokenServiceDescriptor(writer, secDescriptor);
                }
            }

            if (entityDescriptor.Organization != null)
            {
                WriteOrganization(writer, entityDescriptor.Organization);
            }

            foreach (var person in entityDescriptor.Contacts)
            {
                WriteContactPerson(writer, person);
            }

            WriteCustomElements(writer, entityDescriptor);

            writer.WriteEndElement();
        }

        protected virtual void WriteIdentityProviderSingleSignOnDescriptor(XmlWriter writer, IdentityProviderSingleSignOnDescriptor descriptor)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

            writer.WriteStartElement(Saml2MetadataConstants.Elements.IdpssoDescriptor, Saml2MetadataConstants.Namespace);

            if (descriptor.WantAuthenticationRequestsSigned)
            {
                writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.WantAuthenticationRequestsSigned, null,
                    descriptor.WantAuthenticationRequestsSigned);
            }

            WriteSingleSignOnDescriptorAttributes(writer, descriptor);
            WriteCustomAttributes(writer, descriptor);

            WriteSingleSignOnDescriptorElements(writer, descriptor);

            if (!descriptor.SingleSignOnServices.Any()) throw new MetadataSerializationException("Missing single sign on services");

            foreach (var endpoint in descriptor.SingleSignOnServices)
            {
                if (endpoint.ResponseLocation != null) throw new MetadataSerializationException("ResponseLocation present on SingleSignOnService");
                WriteProtocolEndpoint(writer, endpoint, Saml2MetadataConstants.Elements.SingleSignOnService, Saml2MetadataConstants.Namespace);
            }

            foreach (var attribute in descriptor.SupportedAttributes)
            {
                WriteAttribute(writer, attribute);
            }

            WriteCustomElements(writer, descriptor);

            writer.WriteEndElement();
        }

        protected virtual void WriteIndexedProtocolEndpoint(XmlWriter writer, IndexedEndpoint endpoint, string name, string ns)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (ns == null) throw new ArgumentNullException(nameof(ns));

            writer.WriteStartElement(name, ns);

            if (endpoint.Binding == null) throw new MetadataSerializationException($"Endpoint {name} missing binding");
            writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.Binding, null, endpoint.Binding);

            if (endpoint.Location == null) throw new MetadataSerializationException($"Endpoint {name} missing location");
            writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.Location, null, endpoint.Location);

            if (endpoint.Index < 0) throw new MetadataSerializationException($"Endpoint {name} index is less than zero");
            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.EndpointIndex, null, endpoint.Index.ToString(CultureInfo.InvariantCulture));

            writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.ResponseLocation, null, endpoint.ResponseLocation);

            writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.EndpointIsDefault, null, endpoint.IsDefault);

            WriteCustomAttributes(writer, endpoint);
            WriteCustomElements(writer, endpoint);

            writer.WriteEndElement();
        }

        protected virtual void WriteKeyDescriptor(XmlWriter writer, KeyDescriptor keyDescriptor)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (keyDescriptor == null) throw new ArgumentNullException(nameof(keyDescriptor));

            writer.WriteStartElement(Saml2MetadataConstants.Elements.KeyDescriptor, Saml2MetadataConstants.Namespace);

            if (keyDescriptor.Use == KeyType.Encryption || keyDescriptor.Use == KeyType.Signing)
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Use, null, keyDescriptor.Use.ToString().ToLowerInvariant());
            }

            WriteCustomAttributes(writer, keyDescriptor);

            if (keyDescriptor.KeyInfo == null) throw new MetadataSerializationException("Null key info");

            // SecurityTokenSerializer.WriteKeyIdentifier(writer, keyDescriptor.KeyInfo);
            // WriteDSigKeyInfo(writer, keyDescriptor.KeyInfo);

            if (keyDescriptor.EncryptionMethods?.Any() == true)
            {
                foreach (var encryptionMethod in keyDescriptor.EncryptionMethods)
                {
                    if (encryptionMethod.Algorithm == null) throw new MetadataSerializationException("Encryption algorithm missing algorithm");
                    if (!encryptionMethod.Algorithm.IsAbsoluteUri)
                        throw new MetadataSerializationException("Encryption algorithm not using absolute ");

                    writer.WriteStartElement(Saml2MetadataConstants.Elements.EncryptionMethod, Saml2MetadataConstants.Namespace);
                    writer.WriteAttributeString(Saml2MetadataConstants.Attributes.Algorithm, null, encryptionMethod.Algorithm.AbsoluteUri);
                    writer.WriteEndElement();
                }
            }

            WriteCustomElements(writer, keyDescriptor);

            writer.WriteEndElement();
        }

        protected virtual void WriteLocalizedName(XmlWriter writer, LocalizedName name, string elementName, string elementNamespace)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (elementName == null) throw new ArgumentNullException(nameof(elementName));
            if (elementNamespace == null) throw new ArgumentNullException(nameof(elementNamespace));

            writer.WriteStartElement(elementName, elementNamespace);
            writer.WriteAttributeString("xml", "lang", LanguageNamespaceUri, name.Language.Name);
            WriteCustomAttributes(writer, name);
            writer.WriteString(name.Name);
            WriteCustomElements(writer, name);
            writer.WriteEndElement();
        }

        protected virtual void WriteLocalizedUri(XmlWriter writer, LocalizedUri uri, string elementName, string elementNamespace)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (elementName == null) throw new ArgumentNullException(nameof(elementName));
            if (elementNamespace == null) throw new ArgumentNullException(nameof(elementNamespace));

            writer.WriteStartElement(elementName, elementNamespace);
            writer.WriteAttributeString("xml", "lang", LanguageNamespaceUri, uri.Language.Name);
            WriteCustomAttributes(writer, uri);
            writer.WriteString(uri.Uri.IsAbsoluteUri ? uri.Uri.AbsoluteUri : uri.Uri.ToString());
            WriteCustomElements(writer, uri);
            writer.WriteEndElement();
        }

        public void WriteMetadata(Stream stream, MetadataBase metadata)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8, false))
            {
                WriteMetadata(writer, metadata);
            }
        }

        public void WriteMetadata(XmlWriter writer, MetadataBase metadata)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            WriteMetadataCore(writer, metadata);
        }

        protected virtual void WriteMetadataCore(XmlWriter writer, MetadataBase metadata)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            if (metadata is EntitiesDescriptor entities)
            {
                WriteEntitiesDescriptor(writer, entities);
            }
            else if (metadata is EntityDescriptor entity)
            {
                WriteEntityDescriptor(writer, entity);
            }
            else
            {
                throw new MetadataSerializationException("Unsupported metadata entity");
            }
        }

        protected virtual void WriteOrganization(XmlWriter writer, Organization organization)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (organization == null) throw new ArgumentNullException(nameof(organization));
            if (organization.Names.Count == 0) throw new MetadataSerializationException("An organisation must have at least one Name property");
            if (organization.DisplayNames.Count == 0)
                throw new MetadataSerializationException("An organisation must have at least one DisplayName property");
            if (organization.Urls.Count == 0) throw new MetadataSerializationException("An organisation must have at least one Url property");

            writer.WriteStartElement(Saml2MetadataConstants.Elements.Organization, Saml2MetadataConstants.Namespace);

            foreach (var name in organization.Names)
                WriteLocalizedName(writer, name, Saml2MetadataConstants.Elements.OrganizationName, Saml2MetadataConstants.Namespace);

            foreach (var displayName in organization.DisplayNames)
                WriteLocalizedName(writer, displayName, Saml2MetadataConstants.Elements.OrganizationDisplayName, Saml2MetadataConstants.Namespace);

            foreach (var uri in organization.Urls)
                WriteLocalizedUri(writer, uri, Saml2MetadataConstants.Elements.OrganizationUrl, Saml2MetadataConstants.Namespace);

            WriteCustomAttributes(writer, organization);
            WriteCustomElements(writer, organization);
            writer.WriteEndElement();
        }

        protected virtual void WriteRoleDescriptorAttributes(XmlWriter writer, RoleDescriptor descriptor)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (descriptor.ProtocolsSupported?.Any() != true) throw new ArgumentNullException(nameof(descriptor.ProtocolsSupported));

            if (descriptor.ValidUntil.HasValue && descriptor.ValidUntil != DateTime.MaxValue)
            {
                writer.WriteAttributeString(Saml2MetadataConstants.Attributes.ValidUntil, null,
                    descriptor.ValidUntil.Value.ToString("s", CultureInfo.InvariantCulture));
            }

            writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.ErrorUrl, null, descriptor.ErrorUrl);

            var sb = new StringBuilder();
            foreach (var protocol in descriptor.ProtocolsSupported)
            {
                sb.AppendFormat("{0} ", protocol.IsAbsoluteUri ? protocol.AbsoluteUri : protocol.ToString());
            }

            writer.WriteAttributeString(Saml2MetadataConstants.Attributes.ProtocolsSupported, null, sb.ToString().Trim());

            WriteCustomAttributes(writer, descriptor);
        }

        protected virtual void WriteRoleDescriptorElements(XmlWriter writer, RoleDescriptor descriptor)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Organization != null)
            {
                WriteOrganization(writer, descriptor.Organization);
            }

            foreach (var key in descriptor.Keys)
            {
                WriteKeyDescriptor(writer, key);
            }

            foreach (var contact in descriptor.Contacts)
            {
                WriteContactPerson(writer, contact);
            }

            WriteCustomElements(writer, descriptor);
        }

        protected virtual void WriteSecurityTokenServiceDescriptor(XmlWriter writer, SecurityTokenServiceDescriptor descriptor)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (descriptor.SecurityTokenServiceEndpoints.Count == 0)
                throw new MetadataSerializationException("Missing SecurityTokenServiceEndpoints");

            writer.WriteStartElement(Saml2MetadataConstants.Elements.RoleDescriptor, Saml2MetadataConstants.Namespace);
            writer.WriteAttributeString("xsi", "type", XmlSchema.InstanceNamespace,
                FederationMetadataConstants.Prefix + ":" + FederationMetadataConstants.Elements.SecurityTokenServiceType);
            writer.WriteAttributeString("xmlns", FederationMetadataConstants.Prefix, null, FederationMetadataConstants.Namespace);
            WriteWebServiceDescriptorAttributes(writer, descriptor);
            WriteCustomAttributes(writer, descriptor);

            WriteWebServiceDescriptorElements(writer, descriptor);

            foreach (var endpoint in descriptor.SecurityTokenServiceEndpoints)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.SecurityTokenServiceEndpoint, FederationMetadataConstants.Namespace);
                WriteEndpointReference(writer, endpoint);
                writer.WriteEndElement();
            }

            foreach (var endpoint in descriptor.PassiveRequestorEndpoints)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.PassiveRequestorEndpoint, FederationMetadataConstants.Namespace);
                WriteEndpointReference(writer, endpoint);
                writer.WriteEndElement();
            }

            WriteCustomElements(writer, descriptor);

            writer.WriteEndElement();
        }

        protected virtual void WriteServiceProviderSingleSignOnDescriptor(XmlWriter writer, ServiceProviderSingleSignOnDescriptor descriptor)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

            writer.WriteStartElement(Saml2MetadataConstants.Elements.SpssoDescriptor, Saml2MetadataConstants.Namespace);

            if (descriptor.AuthenticationRequestsSigned)
                writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.AuthenticationRequestsSigned, null,
                    descriptor.AuthenticationRequestsSigned);
            if (descriptor.WantAssertionsSigned)
                writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.WantAssertionsSigned, null, descriptor.WantAssertionsSigned);

            WriteSingleSignOnDescriptorAttributes(writer, descriptor);
            WriteCustomAttributes(writer, descriptor);

            WriteSingleSignOnDescriptorElements(writer, descriptor);
            if (descriptor.AssertionConsumerServices.Count == 0) throw new MetadataSerializationException("Missing AssertionConsumerServices");

            foreach (var ep in descriptor.AssertionConsumerServices.Values)
            {
                WriteIndexedProtocolEndpoint(writer, ep, Saml2MetadataConstants.Elements.AssertionConsumerService, Saml2MetadataConstants.Namespace);
            }

            WriteCustomElements(writer, descriptor);
            writer.WriteEndElement();
        }

        protected virtual void WriteSingleSignOnDescriptorAttributes(XmlWriter writer, SingleSignOnDescriptor singleSignOnDescriptor)
        {
            WriteRoleDescriptorAttributes(writer, singleSignOnDescriptor);
            WriteCustomAttributes(writer, singleSignOnDescriptor);
        }

        protected virtual void WriteSingleSignOnDescriptorElements(XmlWriter writer, SingleSignOnDescriptor singleSignOnDescriptor)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (singleSignOnDescriptor == null) throw new ArgumentNullException(nameof(singleSignOnDescriptor));

            WriteRoleDescriptorElements(writer, singleSignOnDescriptor);

            if (singleSignOnDescriptor.ArtifactResolutionServices != null && singleSignOnDescriptor.ArtifactResolutionServices.Count > 0)
            {
                foreach (var ep in singleSignOnDescriptor.ArtifactResolutionServices.Values)
                {
                    if (ep.ResponseLocation != null)
                        throw new MetadataSerializationException("An artifact resoluce service has a null ResponseLocation");
                    WriteIndexedProtocolEndpoint(writer, ep, Saml2MetadataConstants.Elements.ArtifactResolutionService,
                        Saml2MetadataConstants.Namespace);
                }
            }

            if (singleSignOnDescriptor.SingleLogoutServices != null && singleSignOnDescriptor.SingleLogoutServices.Count > 0)
            {
                foreach (var endpoint in singleSignOnDescriptor.SingleLogoutServices)
                {
                    WriteProtocolEndpoint(writer, endpoint, Saml2MetadataConstants.Elements.SingleLogoutService, Saml2MetadataConstants.Namespace);
                }
            }

            if (singleSignOnDescriptor.NameIdentifierFormats != null && singleSignOnDescriptor.NameIdentifierFormats.Count > 0)
            {
                foreach (var nameId in singleSignOnDescriptor.NameIdentifierFormats)
                {
                    if (!nameId.IsAbsoluteUri) throw new MetadataSerializationException("NameIdentifierFormat is not absolute URI");

                    writer.WriteStartElement(Saml2MetadataConstants.Elements.NameIdFormat, Saml2MetadataConstants.Namespace);
                    writer.WriteString(nameId.AbsoluteUri);
                    writer.WriteEndElement();
                }
            }

            WriteCustomElements(writer, singleSignOnDescriptor);
        }

        protected virtual void WriteWebServiceDescriptorAttributes(XmlWriter writer, WebServiceDescriptor descriptor)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

            WriteRoleDescriptorAttributes(writer, descriptor);
            writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.ServiceDisplayName, null, descriptor.ServiceDisplayName);
            writer.WriteAttributeIfPresent(Saml2MetadataConstants.Attributes.ServiceDescription, null, descriptor.ServiceDescription);

            WriteCustomAttributes(writer, descriptor);
        }

        protected virtual void WriteWebServiceDescriptorElements(XmlWriter writer, WebServiceDescriptor descriptor)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

            WriteRoleDescriptorElements(writer, descriptor);

            if (descriptor.TokenTypesOffered.Count > 0)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.TokenTypesOffered, FederationMetadataConstants.Namespace);
                foreach (var tokenType in descriptor.TokenTypesOffered)
                {
                    writer.WriteStartElement(WSFederationMetadataConstants.Elements.TokenType, WSFederationMetadataConstants.Namespace);
                    if (!tokenType.IsAbsoluteUri) throw new MetadataSerializationException("Token type is not absolute URI");

                    writer.WriteAttributeString(WSFederationMetadataConstants.Attributes.Uri, tokenType.AbsoluteUri);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            if (descriptor.ClaimTypesOffered.Count > 0)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.ClaimTypesOffered, FederationMetadataConstants.Namespace);
                foreach (var claim in descriptor.ClaimTypesOffered)
                {
                    WriteDisplayClaim(writer, claim);
                }

                writer.WriteEndElement();
            }

            if (descriptor.ClaimTypesRequested.Count > 0)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.ClaimTypesRequested, FederationMetadataConstants.Namespace);
                foreach (var claim in descriptor.ClaimTypesRequested)
                {
                    WriteDisplayClaim(writer, claim);
                }

                writer.WriteEndElement();
            }

            if (descriptor.TargetScopes.Count > 0)
            {
                writer.WriteStartElement(FederationMetadataConstants.Elements.TargetScopes, FederationMetadataConstants.Namespace);
                foreach (var address in descriptor.TargetScopes)
                {
                    WriteEndpointReference(writer, address);
                }

                writer.WriteEndElement();
            }

            WriteCustomElements(writer, descriptor);
        }

        protected virtual void WriteAttribute(XmlWriter writer, Saml2Attribute attribute)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (attribute == null) throw new ArgumentNullException(nameof(attribute));

            writer.WriteStartElement(Saml2Constants.Elements.Attribute, Saml2Constants.Namespace);

            writer.WriteAttributeString(Saml2Constants.Attributes.Name, attribute.Name);

            writer.WriteAttributeIfPresent(Saml2Constants.Attributes.NameFormat, null, attribute.NameFormat.AbsoluteUri);
            writer.WriteAttributeIfPresent(Saml2Constants.Attributes.FriendlyName, null, attribute.FriendlyName);

            foreach (var value in attribute.Values)
            {
                writer.WriteStartElement(Saml2Constants.Elements.AttributeValue, Saml2Constants.Namespace);

                if (null == value)
                {
                    writer.WriteAttributeString("nil", XmlSchema.InstanceNamespace, XmlConvert.ToString(true));
                }
                else if (value.Length > 0)
                {
                    writer.WriteString(value);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        // Replaces EndpointReference.WriteTo(XmlWriter writer) { ... }
        protected virtual void WriteEndpointReference(XmlWriter writer, EndpointReference endpointReference)
        {
            writer.WriteStartElement(WSAddressing10Constants.Prefix, WSAddressing10Constants.Elements.EndpointReference,
                WSAddressing10Constants.NamespaceUri);
            WriteCustomAttributes(writer, endpointReference);

            writer.WriteStartElement(WSAddressing10Constants.Prefix, WSAddressing10Constants.Elements.Address, WSAddressing10Constants.NamespaceUri);
            writer.WriteString(endpointReference.Uri.AbsoluteUri);
            writer.WriteEndElement();

            foreach (var element in endpointReference.Details)
            {
                element.WriteTo(writer);
            }

            WriteCustomElements(writer, endpointReference);

            writer.WriteEndElement();
        }


        private const string XEncNamespace = "http://www.w3.org/2001/04/xmlenc#";
        private const string DSigNamespace = "http://www.w3.org/2000/09/xmldsig#";
        private const string DSig11Namespace = "http://www.w3.org/2009/xmldsig11#";

        // TODO: Custom approach vs SecurityTokenSerializer + KeyIdentifierClause
        protected virtual void WriteDSigKeyInfo(XmlWriter writer, DSigKeyInfo keyInfo)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (keyInfo == null) throw new ArgumentNullException(nameof(keyInfo));

            writer.WriteStartElement("KeyInfo", DSigNamespace);
            writer.WriteAttributeIfPresent("Id", null, keyInfo.Id);
            WriteCustomAttributes(writer, keyInfo);

            foreach (var keyName in keyInfo.KeyNames)
            {
                writer.WriteElementString("KeyName", DSigNamespace, keyName);
            }

            foreach (var keyValue in keyInfo.KeyValues) WriteKeyValue(writer, keyValue);
            foreach (var keyRetrievalMethods in keyInfo.RetrievalMethods) WriteRetrievalMethod(writer, keyRetrievalMethods);
            foreach (var keyData in keyInfo.Data) WriteKeyData(writer, keyData);

            WriteCustomElements(writer, keyInfo);
            writer.WriteEndElement();
        }

        protected virtual void WriteKeyValue(XmlWriter writer, KeyValue keyValue)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (keyValue == null) throw new ArgumentNullException(nameof(keyValue));

            writer.WriteStartElement("KeyValue", DSigNamespace);
            WriteCustomAttributes(writer, keyValue);

            if (keyValue is RsaKeyValue rsa)
            {
                WriteRsaKeyValue(writer, rsa);
            }
            else
            {
                throw new MetadataSerializationException("Unsupported Key Type");
            }

            WriteCustomElements(writer, keyValue);
            writer.WriteEndElement();
        }

        protected virtual void WriteRsaKeyValue(XmlWriter writer, RsaKeyValue rsa)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));

            writer.WriteStartElement("RSAKeyValue", DSigNamespace);
            WriteCustomAttributes(writer, rsa);

            writer.WriteElementIfPresent("Modulus", DSigNamespace, rsa.Parameters.Modulus);
            writer.WriteElementIfPresent("Exponent", DSigNamespace, rsa.Parameters.Exponent);

            WriteCustomElements(writer, rsa);
            writer.WriteEndElement();
        }

        //TODO: wat dis
        protected virtual void WriteRetrievalMethod(XmlWriter writer, RetrievalMethod method)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (method == null) throw new ArgumentNullException(nameof(method));

            writer.WriteStartElement("RetrievalMethod", DSigNamespace);
            writer.WriteAttributeIfPresent("URI", null, method.Uri);
            writer.WriteAttributeIfPresent("Type", null, method.Type);
            WriteCustomAttributes(writer, method);

            // TODO: Wrapped Elements?
            //WriteWrappedElements(writer, "ds", "Transforms", DSigNamespace, method.Transforms);

            WriteCustomElements(writer, method);
            writer.WriteEndElement();
        }

        protected virtual void WriteKeyData(XmlWriter writer, KeyData keyData)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (keyData == null) throw new ArgumentNullException(nameof(keyData));

            if (keyData is X509Data x509Data) WriteX509Data(writer, x509Data);
            else throw new MetadataSerializationException("Unsupported KeyData type");
        }

        protected virtual void WriteX509Data(XmlWriter writer, X509Data data)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (data == null) throw new ArgumentNullException(nameof(data));

            writer.WriteStartElement("X509Data", DSigNamespace);
            WriteCustomAttributes(writer, data);

            if (data.IssuerSerial != null)
            {
                WriteX509IssuerSerial(writer, data.IssuerSerial);
            }

            if (data.SKI != null)
            {
                writer.WriteElementIfPresent("X509SKI", DSigNamespace, data.SKI);
            }

            writer.WriteElementIfPresent("X509SubjectName", DSigNamespace, data.SubjectName);
            foreach (var cert in data.Certificates)
            {
                writer.WriteElementIfPresent("X509Certificate", DSigNamespace, cert.GetRawCertData());
            }

            if (data.CRL != null)
            {
                writer.WriteElementIfPresent("X509CRL", DSigNamespace, data.CRL);
            }

            if (data.Digest != null)
            {
                WriteX509Digest(writer, data.Digest);
            }

            WriteCustomElements(writer, data);
            writer.WriteEndElement();
        }

        protected virtual void WriteX509IssuerSerial(XmlWriter writer, X509IssuerSerial issuerSerial)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (issuerSerial == null) throw new ArgumentNullException(nameof(issuerSerial));

            writer.WriteStartElement("X509IssuerSerial", DSigNamespace);
            WriteCustomAttributes(writer, issuerSerial);
            writer.WriteElementIfPresent("X509IssuerName", DSigNamespace, issuerSerial.Name);
            writer.WriteElementIfPresent("X509SerialNumber", DSigNamespace, issuerSerial.Serial);
            WriteCustomElements(writer, issuerSerial);
            writer.WriteEndElement();
        }

        protected virtual void WriteX509Digest(XmlWriter writer, X509Digest digest)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (digest == null) throw new ArgumentNullException(nameof(digest));

            writer.WriteStartElement("X509Digest", DSigNamespace);
            writer.WriteAttributeString("Algorithm", digest.Algorithm.ToString());
            WriteCustomAttributes(writer, digest);
            writer.WriteBase64(digest.Value, 0, digest.Value.Length);
            WriteCustomElements(writer, digest);
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Wraps a <see cref="XmlWriter"/> and generates a signature automatically when the envelope
    /// is written completely. By default the generated signature is inserted as
    /// the last element in the envelope. This can be modified by explicitly
    /// calling WriteSignature to indicate the location inside the envelope where
    /// the signature should be inserted.
    /// </summary>
    public class EnvelopedSignatureWriter : DelegatingXmlDictionaryWriter
    {
        private MemoryStream _canonicalStream;
        private bool _disposed;
        private DSigSerializer _dsigSerializer = DSigSerializer.Default;
        private int _elementCount;
        private string _inclusiveNamespacesPrefixList;
        private XmlWriter _originalWriter;
        private string _referenceUri;
        private long _signaturePosition;
        private SigningCredentials _signingCredentials;
        private MemoryStream _writerStream;

        /// <summary>
        /// Initializes an instance of <see cref="Microsoft.IdentityModel.Xml.EnvelopedSignatureWriter"/>. The returned writer can be directly used
        /// to write the envelope. The signature will be automatically generated when
        /// the envelope is completed.
        /// </summary>
        /// <param name="writer">Writer to wrap/</param>
        /// <param name="signingCredentials">SigningCredentials to be used to generate the signature.</param>
        /// <param name="referenceId">The reference Id of the envelope.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="writer"/> is null.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="signingCredentials"/> is null.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="referenceId"/> is null or Empty.</exception>
        public EnvelopedSignatureWriter(XmlWriter writer, SigningCredentials signingCredentials, string referenceId)
            : this(writer, signingCredentials, referenceId, null)
        {
        }

        internal static string[] TokenizeInclusiveNamespacesPrefixList(string inclusiveNamespacesPrefixList)
        {
            if (inclusiveNamespacesPrefixList == null)
                return null;

            string[] prefixes = inclusiveNamespacesPrefixList.Split(null);
            int count = 0;
            for (int i = 0; i < prefixes.Length; i++)
            {
                string prefix = prefixes[i];
                if (prefix == "#default")
                {
                    prefixes[count++] = string.Empty;
                }
                else if (prefix.Length > 0)
                {
                    prefixes[count++] = prefix;
                }
            }

            if (count == 0)
            {
                return null;
            }
            else if (count == prefixes.Length)
            {
                return prefixes;
            }
            else
            {
                string[] result = new string[count];
                Array.Copy(prefixes, result, count);
                return result;
            }
        }

        /// <summary>
        /// Initializes an instance of <see cref="Microsoft.IdentityModel.Xml.EnvelopedSignatureWriter"/>. The returned writer can be directly used
        /// to write the envelope. The signature will be automatically generated when
        /// the envelope is completed.
        /// </summary>
        /// <param name="writer">Writer to wrap/</param>
        /// <param name="signingCredentials">SigningCredentials to be used to generate the signature.</param>
        /// <param name="referenceId">The reference Id of the envelope.</param>
        /// <param name="inclusivePrefixList">inclusive prefix list to use for exclusive canonicalization.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="writer"/> is null.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="signingCredentials"/> is null.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="referenceId"/> is null or Empty.</exception>
        public EnvelopedSignatureWriter(XmlWriter writer, SigningCredentials signingCredentials, string referenceId, string inclusivePrefixList)
        {
            _originalWriter = writer ?? throw LogArgumentNullException(nameof(writer));
            _signingCredentials = signingCredentials ?? throw LogArgumentNullException(nameof(signingCredentials));
            if (string.IsNullOrEmpty(referenceId))
                throw LogArgumentNullException(nameof(referenceId));

            _inclusiveNamespacesPrefixList = inclusivePrefixList;
            _referenceUri = referenceId;
            _writerStream = new MemoryStream();
            _canonicalStream = new MemoryStream();
            InnerWriter = CreateTextWriter(_writerStream, Encoding.UTF8, false);
            InnerWriter.StartCanonicalization(_canonicalStream, false, TokenizeInclusiveNamespacesPrefixList(_inclusiveNamespacesPrefixList));
            _signaturePosition = -1;
        }


        /// <summary>
        /// Gets or sets the <see cref="DSigSerializer"/> to use.
        /// </summary>
        /// <exception cref="ArgumentNullException">if value is null.</exception>
        public DSigSerializer DSigSerializer
        {
            get => _dsigSerializer;
            set => _dsigSerializer = value ?? throw LogArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Calculates and inserts the Signature.
        /// </summary>
        private void OnEndRootElement()
        {
            if (_signaturePosition == -1)
                WriteSignature();

            InnerWriter.WriteEndElement();
            InnerWriter.Flush();
            InnerWriter.EndCanonicalization();

            var signature = CreateSignature();
            var signatureStream = new MemoryStream();
            var signatureWriter = CreateTextWriter(signatureStream);
            DSigSerializer.WriteSignature(signatureWriter, signature);
            signatureWriter.Flush();
            var signatureBytes = signatureStream.ToArray();
            var writerBytes = _writerStream.ToArray();
            byte[] effectiveBytes = new byte[signatureBytes.Length + writerBytes.Length];
            Array.Copy(writerBytes, effectiveBytes, (int) _signaturePosition);
            Array.Copy(signatureBytes, 0, effectiveBytes, (int) _signaturePosition, signatureBytes.Length);
            Array.Copy(writerBytes, (int) _signaturePosition, effectiveBytes, (int) _signaturePosition + signatureBytes.Length,
                writerBytes.Length - (int) _signaturePosition);

            XmlDocument doc = new XmlDocument();
            string xml = Encoding.UTF8.GetString(effectiveBytes);


            XmlReader reader = XmlDictionaryReader.CreateTextReader(effectiveBytes, XmlDictionaryReaderQuotas.Max);

            var readOuterXml = reader.ReadOuterXml();
            var readContentAsString = reader.ReadContentAsString();
            var readInnerXml = reader.ReadInnerXml();

            var readerCanResolveEntity = reader.CanResolveEntity;
            var moveToFirstAttribute = reader.MoveToFirstAttribute();

            reader.MoveToContent();
            _originalWriter.WriteNode(reader, false);
            _originalWriter.Flush();
        }

        private Signature CreateSignature()
        {
            CryptoProviderFactory cryptoProviderFactory = _signingCredentials.CryptoProviderFactory ?? _signingCredentials.Key.CryptoProviderFactory;
            var hashAlgorithm = cryptoProviderFactory.CreateHashAlgorithm(_signingCredentials.Digest);
            if (hashAlgorithm == null)
                throw LogExceptionMessage(new XmlValidationException(FormatInvariant(LogMessages.IDX30213, cryptoProviderFactory.ToString(),
                    _signingCredentials.Digest)));

            Reference reference = null;
            try
            {
                reference = new Reference(new EnvelopedSignatureTransform(),
                    new ExclusiveCanonicalizationTransform {InclusiveNamespacesPrefixList = _inclusiveNamespacesPrefixList})
                {
                    Uri = _referenceUri,
                    DigestValue = Convert.ToBase64String(hashAlgorithm.ComputeHash(_canonicalStream.ToArray())),
                    DigestMethod = _signingCredentials.Digest
                };
            }
            finally
            {
                if (hashAlgorithm != null)
                    cryptoProviderFactory.ReleaseHashAlgorithm(hashAlgorithm);
            }

            var signedInfo = new SignedInfo(reference)
            {
                CanonicalizationMethod = SecurityAlgorithms.ExclusiveC14n,
                SignatureMethod = _signingCredentials.Algorithm
            };

            var canonicalSignedInfoStream = new MemoryStream();
            var signedInfoWriter = CreateTextWriter(Stream.Null);
            signedInfoWriter.StartCanonicalization(canonicalSignedInfoStream, false, null);
            DSigSerializer.WriteSignedInfo(signedInfoWriter, signedInfo);
            signedInfoWriter.EndCanonicalization();
            signedInfoWriter.Flush();

            var provider = cryptoProviderFactory.CreateForSigning(_signingCredentials.Key, _signingCredentials.Algorithm);
            if (provider == null)
                throw LogExceptionMessage(new XmlValidationException(FormatInvariant(LogMessages.IDX30213, cryptoProviderFactory.ToString(),
                    _signingCredentials.Key.ToString(), _signingCredentials.Algorithm)));

            try
            {
                return new Signature
                {
                    KeyInfo = new KeyInfo(_signingCredentials.Key),
                    SignatureValue = Convert.ToBase64String(provider.Sign(canonicalSignedInfoStream.ToArray())),
                    SignedInfo = signedInfo,
                };
            }
            finally
            {
                if (provider != null)
                    cryptoProviderFactory.ReleaseSignatureProvider(provider);
            }
        }

        /// <summary>
        /// Sets the position of the signature within the envelope. Call this
        /// method while writing the envelope to indicate at which point the 
        /// signature should be inserted.
        /// </summary>
        public void WriteSignature()
        {
            InnerWriter.Flush();
            _signaturePosition = _writerStream.Length;
        }

        /// <summary>
        /// Overrides the base class implementation. When the last element of the envelope is written
        /// the signature is automatically computed over the envelope and the signature is inserted at
        /// the appropriate position, if WriteSignature was explicitly called or is inserted at the
        /// end of the envelope.
        /// </summary>
        public override void WriteEndElement()
        {
            _elementCount--;
            if (_elementCount == 0)
            {
                base.Flush();
                OnEndRootElement();
            }
            else
            {
                base.WriteEndElement();
            }
        }

        /// <summary>
        /// Overrides the base class implementation. When the last element of the envelope is written
        /// the signature is automatically computed over the envelope and the signature is inserted at
        /// the appropriate position, if WriteSignature was explicitly called or is inserted at the
        /// end of the envelope.
        /// </summary>
        public override void WriteFullEndElement()
        {
            _elementCount--;
            if (_elementCount == 0)
            {
                base.Flush();
                OnEndRootElement();
            }
            else
            {
                base.WriteFullEndElement();
            }
        }

        /// <summary>
        /// Overrides the base class. Writes the specified start tag and associates
        /// it with the given namespace.
        /// </summary>
        /// <param name="prefix">The namespace prefix of the element.</param>
        /// <param name="localName">The local name of the element.</param>
        /// <param name="namespace">The namespace URI to associate with the element.</param>
        public override void WriteStartElement(string prefix, string localName, string @namespace)
        {
            _elementCount++;
            base.WriteStartElement(prefix, localName, @namespace);
        }

        #region IDisposable Members

        /// <summary>
        /// Releases the unmanaged resources used by the System.IdentityModel.Protocols.XmlSignature.EnvelopedSignatureWriter and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                if (_writerStream != null)
                {
                    _writerStream.Dispose();
                    _writerStream = null;
                }

                if (_canonicalStream != null)
                {
                    _canonicalStream.Dispose();
                    _canonicalStream = null;
                }
            }
        }

        #endregion
    }

    internal static class LogMessages
    {
#pragma warning disable 1591
        // SamlSerializing reading
        internal const string IDX13102 = "IDX13102: Exception thrown while reading '{0}' for Saml2SecurityToken. Inner exception: '{1}'.";
        internal const string IDX13106 = "IDX13106: Unable to read for Saml2SecurityToken. Element: '{0}' as missing Attribute: '{1}'.";

        internal const string IDX13108 =
            "IDX13108: When reading '{0}', Assertion.Subject is null and no Statements were found. [Saml2Core, line 585].";

        internal const string IDX13109 =
            "IDX13109: When reading '{0}', Assertion.Subject is null and an Authentication, Attribute or AuthorizationDecision Statement was found. and no Statements were found. [Saml2Core, lines 1050, 1168, 1280].";

        internal const string IDX13137 = "IDX13137: Unable to read for Saml2SecurityToken. Version must be '2.0' was: '{0}'.";

        internal const string IDX13141 =
            "IDX13141: EncryptedAssertion is not supported. You will need to override ReadAssertion and provide support.";

        internal const string IDX13302 = "IDX13302: An assertion with no statements must contain a 'Subject' element.";
        internal const string IDX13303 = "IDX13303: 'Subject' is required in Saml2Assertion for built-in statement type.";
        internal const string IDX30213 =
            "IDX30213: The CryptoProviderFactory: '{0}', CreateForSigning returned null for key: '{1}', SignatureMethod: '{2}'.";
#pragma warning restore 1591

    }

}