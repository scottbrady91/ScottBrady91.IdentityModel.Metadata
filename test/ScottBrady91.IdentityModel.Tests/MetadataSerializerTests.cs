using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using FluentAssertions;
using ScottBrady91.IdentityModel.Metadata;
using ScottBrady91.IdentityModel.Tokens;
using Xunit;

namespace ScottBrady91.IdentityModel.Tests
{
    public class MetadataSerializerTests
    {
        private const string Xmlns = "urn:oasis:names:tc:SAML:2.0:metadata";

        private static readonly IdentityProviderSingleSignOnDescriptor Idp = new IdentityProviderSingleSignOnDescriptor
        {
            ErrorUrl = new Uri("http://localhost/uh-oh"),
            WantAuthenticationRequestsSigned = true,
            ProtocolsSupported = {new Uri("urn:oasis:names:tc:SAML:2.0:protocol")},
            SingleSignOnServices = {new ProtocolEndpoint(new Uri("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"), new Uri("http://localhost:5000/saml/sso"))},
            SingleLogoutServices = {new ProtocolEndpoint(new Uri("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"), new Uri("http://localhost:5000/saml/slo"))},
            NameIdentifierFormats = {new Uri("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified")}
        };

        private static readonly EntityDescriptor Entity = new EntityDescriptor
        {
            EntityId = new EntityId("internal"),
            RoleDescriptors = {Idp},
            Organization = new Organization
            {
                Names = {new LocalizedName("scott", new CultureInfo("en-GB"))},
                DisplayNames = {new LocalizedName("Scott", new CultureInfo("en-GB"))},
                Urls = {new LocalizedUri(new Uri("https://www.scottbrady91.com"), new CultureInfo("en-GB"))}
            },
            Contacts =
            {
                new ContactPerson
                {
                    GivenName = "Scott",
                    Surname = "Brady",
                    Company = "Rock Solid Knowledge",
                    Type = ContactType.Technical,
                    EmailAddresses = {"scott.brady@rocksolidknowledge.com"},
                    TelephoneNumbers = {"441174220515"}
                }
            }
        };

        // TODO: What about ID?
        [Fact]
        public void WithEntityId_ExpectCorrectNamespaceAndEntityId()
        {
            var xml = SerializeMetadata(Entity);

            xml.Name.Should().Be("EntityDescriptor");
            xml.Should().HaveAttribute("entityID", Entity.EntityId.Id);
            xml.NamespaceURI.Should().Be(Xmlns);
        }

        [Fact]
        public void WithOrganization_ExpectOrganizationInMetadata()
        {
            var xml = SerializeMetadata(Entity);

            xml.Should().HaveElementWithNamespace("Organization", Xmlns);
            var organization = xml["Organization"];

            organization.Should().HaveElementWithNamespace("OrganizationName", Xmlns)
                .Which.Should().HaveAttributeWithNamespace("lang", "http://www.w3.org/XML/1998/namespace", Entity.Organization.Names.Single().Language.Name)
                .And.HaveInnerText(Entity.Organization.Names.Single().Name);

            organization.Should().HaveElementWithNamespace("OrganizationDisplayName", Xmlns)
                .Which.Should().HaveAttributeWithNamespace("lang", "http://www.w3.org/XML/1998/namespace", Entity.Organization.DisplayNames.Single().Language.Name)
                .And.HaveInnerText(Entity.Organization.DisplayNames.Single().Name);

            organization.Should().HaveElementWithNamespace("OrganizationURL", Xmlns)
                .Which.Should().HaveAttributeWithNamespace("lang", "http://www.w3.org/XML/1998/namespace", Entity.Organization.Urls.Single().Language.Name)
                .And.HaveInnerText(Entity.Organization.Urls.Single().Uri.ToString());
        }

        [Fact]
        public void WithContact_ExceptContactInMetadata()
        {
            var xml = SerializeMetadata(Entity);

            xml.Should().HaveElementWithNamespace("ContactPerson", Xmlns)
                .Which.Should().HaveAttribute("contactType", "technical");
            var contact = xml["ContactPerson"];

            contact.Should().HaveElementWithNamespace("GivenName", Xmlns)
                .Which.Should().HaveInnerText(Entity.Contacts.Single().GivenName);
            contact.Should().HaveElementWithNamespace("SurName", Xmlns)
                .Which.Should().HaveInnerText(Entity.Contacts.Single().Surname);
            contact.Should().HaveElementWithNamespace("Company", Xmlns)
                .Which.Should().HaveInnerText(Entity.Contacts.Single().Company);
            contact.Should().HaveElementWithNamespace("EmailAddress", Xmlns)
                .Which.Should().HaveInnerText(Entity.Contacts.Single().EmailAddresses.Single());
            contact.Should().HaveElementWithNamespace("TelephoneNumber", Xmlns)
                .Which.Should().HaveInnerText(Entity.Contacts.Single().TelephoneNumbers.Single());
        }

        [Fact]
        public void WhenIdp_ExpectIdpAttributesInMetadata()
        {
            var xml = SerializeMetadata(Entity);

            xml.Should().HaveElementWithNamespace("IDPSSODescriptor", Xmlns)
                .Which.Should().HaveAttribute("protocolSupportEnumeration", Idp.ProtocolsSupported.Single().ToString())
                .And.HaveAttribute("WantAuthnRequestsSigned", Idp.WantAuthenticationRequestsSigned.ToString().ToLower())
                .And.HaveAttribute("errorURL", Idp.ErrorUrl.ToString());
        }

        [Fact]
        public void WhenIdpWithSsoService_ExpectSsoServiceInMetadata()
        {
            var xml = SerializeMetadata(Entity);

            xml.Should().HaveElementWithNamespace("IDPSSODescriptor", Xmlns);
            var idpElement = xml["IDPSSODescriptor"];

            idpElement.Should().HaveElementWithNamespace("SingleSignOnService", Xmlns)
                .Which.Should().HaveAttribute("Binding", Idp.SingleSignOnServices.Single().Binding.ToString())
                .And.HaveAttribute("Location", Idp.SingleSignOnServices.Single().Location.ToString());
        }

        [Fact]
        public void WhenIdpWithSloService_ExpectSloServiceInMetadata()
        {
            var xml = SerializeMetadata(Entity);

            xml.Should().HaveElementWithNamespace("IDPSSODescriptor", Xmlns);
            var idpElement = xml["IDPSSODescriptor"];

            idpElement.Should().HaveElementWithNamespace("SingleLogoutService", Xmlns)
                .Which.Should().HaveAttribute("Binding", Idp.SingleLogoutServices.Single().Binding.ToString())
                .And.HaveAttribute("Location", Idp.SingleLogoutServices.Single().Location.ToString());
        }

        [Fact]
        public void WhenIdpWithNameIdentifierFormats_ExpectNameIdentifierFormatsInMetadata()
        {
            var xml = SerializeMetadata(Entity);

            xml.Should().HaveElementWithNamespace("IDPSSODescriptor", Xmlns);
            var idpElement = xml["IDPSSODescriptor"];

            idpElement.Should().HaveElementWithNamespace("NameIDFormat", Xmlns)
                .Which.Should().HaveInnerText("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified");
        }

        [Fact]
        public void WhenIdpHasSigningKey_ExpectPublicKeyInMetadata()
        {
            var clause = new X509SecurityToken(new X509Certificate2("idsrv3test.pfx", "idsrv3test"))
                .CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause>();
            var key = new KeyDescriptor(new SecurityKeyIdentifier(clause)) { Use = KeyType.Signing };
            Idp.Keys.Add(key);

            var xml = SerializeMetadata(Entity);

            xml.Should().HaveElementWithNamespace("IDPSSODescriptor", Xmlns);
            var idpElement = xml["IDPSSODescriptor"];

            idpElement.Should().HaveElementWithNamespace("NameIDFormat", Xmlns)
                .Which.Should().HaveInnerText("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified");
        }

        private XmlElement SerializeMetadata(EntityDescriptor entityDescriptor)
        {
            var ser = new MetadataSerializer();
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { OmitXmlDeclaration = true }))
                {
                    ser.WriteMetadata(xmlWriter, entityDescriptor);
                }
            }

            var xml = sb.ToString();
            xml.Should().NotBeNullOrEmpty();

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement;
        }
    }
}