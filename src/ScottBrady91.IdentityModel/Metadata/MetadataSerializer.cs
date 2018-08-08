using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.IdentityModel.Xml;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

// ReSharper disable VirtualMemberNeverOverridden
namespace ScottBrady91.IdentityModel.Metadata
{
    public class MetadataSerializer
    {
        private const string LanguageNamespaceUri = "http://www.w3.org/XML/1998/namespace";

        public MetadataSerializer() { }

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

            if (string.IsNullOrEmpty(claim.ClaimType)) throw new MetadataSerializationException("Missing ClaimType");
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
                if (!string.IsNullOrEmpty(entity.FederationId))
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

            writer.WriteAttributeIfPresent(WSFederationMetadataConstants.Attributes.FederationId, WSFederationMetadataConstants.Namespace,
                entityDescriptor.FederationId);
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

            var dSigSerializer = DSigSerializer.Default;
            dSigSerializer.WriteKeyInfo(writer, keyDescriptor.KeyInfo);

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
    }
}