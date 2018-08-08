using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Xml;
using ScottBrady91.IdentityModel.Metadata;
using Xunit;

namespace ScottBrady91.IdentityModel.Tests
{
    public class MetadataSerializerTests
    {
        private const string Xmlns = "urn:oasis:names:tc:SAML:2.0:metadata";

        private readonly IdentityProviderSingleSignOnDescriptor idp;
        private readonly EntityDescriptor entity;

        public MetadataSerializerTests()
        {
            idp = new IdentityProviderSingleSignOnDescriptor
            {
                ErrorUrl = new Uri("http://localhost/uh-oh"),
                WantAuthenticationRequestsSigned = true,
                ProtocolsSupported = { new Uri("urn:oasis:names:tc:SAML:2.0:protocol") },
                SingleSignOnServices = { new ProtocolEndpoint(new Uri("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"), new Uri("http://localhost:5000/saml/sso")) },
                SingleLogoutServices = { new ProtocolEndpoint(new Uri("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"), new Uri("http://localhost:5000/saml/slo")) },
                NameIdentifierFormats = { new Uri("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified") }
            };

            entity = new EntityDescriptor
            {
                EntityId = new EntityId("internal"),
                RoleDescriptors = { idp },
                Organization = new Organization
                {
                    Names = { new LocalizedName("scott", new CultureInfo("en-GB")) },
                    DisplayNames = { new LocalizedName("Scott", new CultureInfo("en-GB")) },
                    Urls = { new LocalizedUri(new Uri("https://www.scottbrady91.com"), new CultureInfo("en-GB")) }
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
        }

        // TODO: What about ID?
        [Fact]
        public void WithEntityId_ExpectCorrectNamespaceAndEntityId()
        {
            var xml = SerializeMetadata(entity);

            xml.Name.Should().Be("EntityDescriptor");
            xml.Should().HaveAttribute("entityID", entity.EntityId.Id);
            xml.NamespaceURI.Should().Be(Xmlns);
        }

        [Fact]
        public void WithOrganization_ExpectOrganizationInMetadata()
        {
            var xml = SerializeMetadata(entity);

            xml.Should().HaveElementWithNamespace("Organization", Xmlns);
            var organization = xml["Organization"];

            organization.Should().HaveElementWithNamespace("OrganizationName", Xmlns)
                .Which.Should().HaveAttributeWithNamespace("lang", "http://www.w3.org/XML/1998/namespace", entity.Organization.Names.Single().Language.Name)
                .And.HaveInnerText(entity.Organization.Names.Single().Name);

            organization.Should().HaveElementWithNamespace("OrganizationDisplayName", Xmlns)
                .Which.Should().HaveAttributeWithNamespace("lang", "http://www.w3.org/XML/1998/namespace", entity.Organization.DisplayNames.Single().Language.Name)
                .And.HaveInnerText(entity.Organization.DisplayNames.Single().Name);

            organization.Should().HaveElementWithNamespace("OrganizationURL", Xmlns)
                .Which.Should().HaveAttributeWithNamespace("lang", "http://www.w3.org/XML/1998/namespace", entity.Organization.Urls.Single().Language.Name)
                .And.HaveInnerText(entity.Organization.Urls.Single().Uri.ToString());
        }

        [Fact]
        public void WithContact_ExceptContactInMetadata()
        {
            var xml = SerializeMetadata(entity);

            xml.Should().HaveElementWithNamespace("ContactPerson", Xmlns)
                .Which.Should().HaveAttribute("contactType", "technical");
            var contact = xml["ContactPerson"];

            contact.Should().HaveElementWithNamespace("GivenName", Xmlns)
                .Which.Should().HaveInnerText(entity.Contacts.Single().GivenName);
            contact.Should().HaveElementWithNamespace("SurName", Xmlns)
                .Which.Should().HaveInnerText(entity.Contacts.Single().Surname);
            contact.Should().HaveElementWithNamespace("Company", Xmlns)
                .Which.Should().HaveInnerText(entity.Contacts.Single().Company);
            contact.Should().HaveElementWithNamespace("EmailAddress", Xmlns)
                .Which.Should().HaveInnerText(entity.Contacts.Single().EmailAddresses.Single());
            contact.Should().HaveElementWithNamespace("TelephoneNumber", Xmlns)
                .Which.Should().HaveInnerText(entity.Contacts.Single().TelephoneNumbers.Single());
        }

        [Fact]
        public void WhenIdp_ExpectIdpAttributesInMetadata()
        {
            var xml = SerializeMetadata(entity);

            xml.Should().HaveElementWithNamespace("IDPSSODescriptor", Xmlns)
                .Which.Should().HaveAttribute("protocolSupportEnumeration", idp.ProtocolsSupported.Single().ToString())
                .And.HaveAttribute("WantAuthnRequestsSigned", idp.WantAuthenticationRequestsSigned.ToString().ToLower())
                .And.HaveAttribute("errorURL", idp.ErrorUrl.ToString());
        }

        [Fact]
        public void WhenIdpWithSsoService_ExpectSsoServiceInMetadata()
        {
            var xml = SerializeMetadata(entity);

            xml.Should().HaveElementWithNamespace("IDPSSODescriptor", Xmlns);
            var idpElement = xml["IDPSSODescriptor"];

            idpElement.Should().HaveElementWithNamespace("SingleSignOnService", Xmlns)
                .Which.Should().HaveAttribute("Binding", idp.SingleSignOnServices.Single().Binding.ToString())
                .And.HaveAttribute("Location", idp.SingleSignOnServices.Single().Location.ToString());
        }

        [Fact]
        public void WhenIdpWithSloService_ExpectSloServiceInMetadata()
        {
            var xml = SerializeMetadata(entity);

            xml.Should().HaveElementWithNamespace("IDPSSODescriptor", Xmlns);
            var idpElement = xml["IDPSSODescriptor"];

            idpElement.Should().HaveElementWithNamespace("SingleLogoutService", Xmlns)
                .Which.Should().HaveAttribute("Binding", idp.SingleLogoutServices.Single().Binding.ToString())
                .And.HaveAttribute("Location", idp.SingleLogoutServices.Single().Location.ToString());
        }

        [Fact]
        public void WhenIdpWithNameIdentifierFormats_ExpectNameIdentifierFormatsInMetadata()
        {
            var xml = SerializeMetadata(entity);

            xml.Should().HaveElementWithNamespace("IDPSSODescriptor", Xmlns);
            var idpElement = xml["IDPSSODescriptor"];

            idpElement.Should().HaveElementWithNamespace("NameIDFormat", Xmlns)
                .Which.Should().HaveInnerText("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified");
        }

        //<Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
        //   <SignedInfo>
        //      <CanonicalizationMethod Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#" />
        //      <SignatureMethod Algorithm="http://www.w3.org/2001/04/xmldsig-more#rsa-sha256" />
        //      <Reference URI="#_8b7b2a03-1ae6-4235-afab-78dcd4d5ae84">
        //         <Transforms>
        //            <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature" />
        //            <Transform Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#" />
        //         </Transforms>
        //         <DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256" />
        //         <DigestValue>gVxNWJ2dzeOPVzBWiz86Tblpge8MiurCZ1GX2g/vnus=</DigestValue>
        //      </Reference>
        //   </SignedInfo>
        //   <SignatureValue>...</Signature>
        //   <KeyInfo>...</KeyInfo>
        //</Signature>
        [Fact]
        public void WhenIdpHasSigningCredentialAndDefaultSignatureMethods_ExpectMetadataSignedInfoUsingRsaSha256()
        {
            const string expectedNamespace = "http://www.w3.org/2000/09/xmldsig#";

            entity.SigningCredentials = new Tokens.X509SigningCredentials(new X509Certificate2("idsrv3test.pfx", "idsrv3test"));
            var xml = SerializeMetadata(entity);


            xml.Should().HaveElementWithNamespace("Signature", expectedNamespace)
                .Which.Should().HaveElementWithNamespace("SignedInfo", expectedNamespace);

            var signedInfoXml = xml["Signature"]["SignedInfo"];
            signedInfoXml.Should().HaveElementWithNamespace("CanonicalizationMethod", expectedNamespace)
                .Which.Should().HaveAttribute("Algorithm", "http://www.w3.org/2001/10/xml-exc-c14n#");
            signedInfoXml.Should().HaveElementWithNamespace("SignatureMethod", expectedNamespace)
                .Which.Should().HaveAttribute("Algorithm", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");

            signedInfoXml.Should().HaveElementWithNamespace("Reference", expectedNamespace);
            var reference = signedInfoXml["Reference"];
            reference.HasAttribute("URI").Should().BeTrue();
            var uri = reference.Attributes["URI"];
            uri.Value.Should().StartWith("#_");

            reference.Should().HaveElementWithNamespace("Transforms", expectedNamespace)
                .And.HaveElementWithNamespace("DigestValue", expectedNamespace)
                .And.HaveElementWithNamespace("DigestMethod", expectedNamespace)
                .Which.Should().HaveAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");

            var xTransforms = XElement.Parse(reference["Transforms"].OuterXml);
            xTransforms.Descendants()
                .Any(x => (string)x.Attribute("Algorithm") == "http://www.w3.org/2000/09/xmldsig#enveloped-signature")
                .Should().BeTrue();
            xTransforms.Descendants()
                .Any(x => (string)x.Attribute("Algorithm") == "http://www.w3.org/2001/10/xml-exc-c14n#")
                .Should().BeTrue();
        }

        //<Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
        //   <SignedInfo>...</SignedInfo>
        //   <SignatureValue>KHGcYGlFxFGsPRPjJ0MNitY18iPkhAZ4Cp6pLp1BHTYyNwoTnSZuum3Fx+MblwrrxL5bvxREnZtTllaN2xFj2MlZAa2AdLgeFeMfqzeWrbZIUsfrlLHWZ5C5V7/fG/MU5Me5BZDuRHKHtGCosb5U/2rwr+BvsWbeP6Y2EmU5mWcB7iuQvZdBZIFRdCH11b4GUe4wcR/vSyFQqgfNVvJ5v4gTOD3WRvQKJxOm/EQI6x1coN4/neZGt0HR12WT0+cEyOeaGJBfiolj3n2fX1YTZyqQ4lKAxSrvuakMlNYk0IVLIy0q00BUyb1fQo9iSy65wSxXS+Qx3C0YmwTzUti9YQ==</SignatureValue>
        //   <KeyInfo>
        //      <X509Data>
        //         <X509Certificate>MIIDBTCCAfGgAwIBAgIQNQb+T2ncIrNA6cKvUA1GWTAJBgUrDgMCHQUAMBIxEDAOBgNVBAMTB0RldlJvb3QwHhcNMTAwMTIwMjIwMDAwWhcNMjAwMTIwMjIwMDAwWjAVMRMwEQYDVQQDEwppZHNydjN0ZXN0MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAqnTksBdxOiOlsmRNd+mMS2M3o1IDpK4uAr0T4/YqO3zYHAGAWTwsq4ms+NWynqY5HaB4EThNxuq2GWC5JKpO1YirOrwS97B5x9LJyHXPsdJcSikEI9BxOkl6WLQ0UzPxHdYTLpR4/O+0ILAlXw8NU4+jB4AP8Sn9YGYJ5w0fLw5YmWioXeWvocz1wHrZdJPxS8XnqHXwMUozVzQj+x6daOv5FmrHU1r9/bbp0a1GLv4BbTtSh4kMyz1hXylho0EvPg5p9YIKStbNAW9eNWvv5R8HN7PPei21AsUqxekK0oW9jnEdHewckToX7x5zULWKwwZIksll0XnVczVgy7fCFwIDAQABo1wwWjATBgNVHSUEDDAKBggrBgEFBQcDATBDBgNVHQEEPDA6gBDSFgDaV+Q2d2191r6A38tBoRQwEjEQMA4GA1UEAxMHRGV2Um9vdIIQLFk7exPNg41NRNaeNu0I9jAJBgUrDgMCHQUAA4IBAQBUnMSZxY5xosMEW6Mz4WEAjNoNv2QvqNmk23RMZGMgr516ROeWS5D3RlTNyU8FkstNCC4maDM3E0Bi4bbzW3AwrpbluqtcyMN3Pivqdxx+zKWKiORJqqLIvN8CT1fVPxxXb/e9GOdaR8eXSmB0PgNUhM4IjgNkwBbvWC9F/lzvwjlQgciR7d4GfXPYsE1vf8tmdQaY8/PtdAkExmbrb9MihdggSoGXlELrPA91Yce+fiRcKY3rQlNWVd4DOoJ/cPXsXwry8pWjNCo5JD8Q+RQ5yZEy7YPoifwemLhTdsBz3hlZr28oCGJ3kbnpW0xGvQb3VHSTVVbeei0CfXoW6iz1</X509Certificate>
        //      </X509Data>
        //   </KeyInfo>
        //</Signature>
        [Fact]
        public void WhenIdpHasSigningCredentialAndDefaultSignatureMethods_ExpectMetadataSignatureValueAndKeyInfo()
        {
            const string expectedNamespace = "http://www.w3.org/2000/09/xmldsig#";

            entity.SigningCredentials = new Tokens.X509SigningCredentials(new X509Certificate2("idsrv3test.pfx", "idsrv3test"));
            var xml = SerializeMetadata(entity);
            
            xml.Should().HaveElementWithNamespace("Signature", expectedNamespace)
                .Which.Should().HaveElementWithNamespace("SignatureValue", expectedNamespace)
                .And.HaveElementWithNamespace("KeyInfo", expectedNamespace);

            xml["Signature"]["SignatureValue"].InnerText.Should().NotBeNullOrEmpty();

            var keyInfo = xml["Signature"]["KeyInfo"];
            keyInfo.Should().HaveElementWithNamespace("X509Data", expectedNamespace)
                .Which.Should().HaveElementWithNamespace("X509Certificate", expectedNamespace);
            var x509Certificate = keyInfo["X509Data"]["X509Certificate"];
            x509Certificate.InnerText.Should().NotBeNullOrEmpty();

            var loadedCert = new X509Certificate2(Convert.FromBase64String(x509Certificate.InnerText));
            loadedCert.PublicKey.Should().NotBeNull();
            loadedCert.HasPrivateKey.Should().BeFalse();
        }

        [Fact]
        public void WhenIdpHasSigningKey_ExpectPublicKeyInMetadata()
        {
            var key = new KeyDescriptor(new KeyInfo(new X509SecurityKey(new X509Certificate2("idsrv3test.pfx", "idsrv3test")))) { Use = KeyType.Signing };
            idp.Keys.Add(key);

            var xml = SerializeMetadata(entity);

            xml.Should().HaveElementWithNamespace("IDPSSODescriptor", Xmlns);
            var idpElement = xml["IDPSSODescriptor"];

            idpElement.Should().HaveElementWithNamespace("KeyDescriptor", Xmlns)
                .Which.Should().HaveElementWithNamespace("KeyInfo", "http://www.w3.org/2000/09/xmldsig#");
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