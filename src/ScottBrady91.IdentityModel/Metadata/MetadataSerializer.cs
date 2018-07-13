using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.IdentityModel.Xml;
using ScottBrady91.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using ScottBrady91.IdentityModel.Selectors;
using SigningCredentials = Microsoft.IdentityModel.Tokens.SigningCredentials;

namespace ScottBrady91.IdentityModel.Metadata
{
	public class MetadataSerializer
	{
		const string XmlNs = "http://www.w3.org/XML/1998/namespace";
		const string FedNs = "http://docs.oasis-open.org/wsfed/federation/200706";
		const string WsaNs = "http://www.w3.org/2005/08/addressing";
		const string WspNs = "http://schemas.xmlsoap.org/ws/2002/12/policy";
		const string Saml2MetadataNs = "urn:oasis:names:tc:SAML:2.0:metadata";
		const string Saml2AssertionNs = "urn:oasis:names:tc:SAML:2.0:assertion";
		const string XsiNs = "http://www.w3.org/2001/XMLSchema-instance";
		const string AuthNs = "http://docs.oasis-open.org/wsfed/authorization/200706";
		const string XEncNs = "http://www.w3.org/2001/04/xmlenc#";
		const string DSigNs = "http://www.w3.org/2000/09/xmldsig#";
		const string EcDsaNs = "http://www.w3.org/2001/04/xmldsig-more#";
		const string DSig11Ns = "http://www.w3.org/2009/xmldsig11#";
		const string IdpDiscNs = "urn:oasis:names:tc:SAML:profiles:SSO:idp-discovery-protocol";

		public SecurityTokenSerializer SecurityTokenSerializer { get; }

	    public MetadataSerializer() { } // TODO: Default Key Serializer
        public MetadataSerializer(SecurityTokenSerializer serializer)
	    {
	        SecurityTokenSerializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
	        /*TrustedStoreLocation = IdentityConfiguration.DefaultTrustedStoreLocation;
	        CertificateValidationMode = IdentityConfiguration.DefaultCertificateValidationMode;
	        RevocationMode = IdentityConfiguration.DefaultRevocationMode;*/
        }

		static void WriteWrappedElements(XmlWriter writer, string wrapPrefix,
			string wrapName, string wrapNs, IEnumerable<XmlElement> elts)
		{
			if (elts.Any())
			{
				writer.WriteStartElement(wrapPrefix, wrapName, wrapNs);
				foreach (var elt in elts)
				{
					elt.WriteTo(writer);
				}
				writer.WriteEndElement();
			}
		}

		protected virtual void WriteServiceName(XmlWriter writer, ServiceName serviceName)
		{
			writer.WriteStartElement("wsa", "ServiceName", WsaNs);
			WriteCustomAttributes(writer, serviceName);
			if (!String.IsNullOrEmpty(serviceName.PortName))
			{
				writer.WriteAttributeString("PortName", serviceName.PortName);
			}
			if (!String.IsNullOrEmpty(serviceName.Name))
			{
				writer.WriteString(serviceName.Name);
			}
			writer.WriteEndElement();
		}

		protected virtual void WriteEndpointReference(XmlWriter writer, EndpointReference endpointReference)
		{
			writer.WriteStartElement("wsa", "EndpointReference", WsaNs);
			WriteCustomAttributes(writer, endpointReference);
			writer.WriteStartElement("wsa", "Address", WsaNs);
			writer.WriteString(endpointReference.Uri.ToString());
			writer.WriteEndElement();

			WriteWrappedElements(writer, "wsa", "ReferenceProperties", WsaNs,
				endpointReference.ReferenceProperties);
			WriteWrappedElements(writer, "wsa", "ReferenceParameters", WsaNs,
				endpointReference.ReferenceParameters);
			if (!String.IsNullOrEmpty(endpointReference.PortType))
			{
				writer.WriteStartElement("wsa", "PortType", WsaNs);
				writer.WriteString(endpointReference.PortType);
				writer.WriteEndElement();
			}
			if (endpointReference.ServiceName != null)
			{
				WriteServiceName(writer, endpointReference.ServiceName);
			}
			WriteWrappedElements(writer, "wsa", "Metadata", WsaNs,
				endpointReference.Metadata);
			if (endpointReference.Policies.Count > 0)
			{
				foreach (var polElt in endpointReference.Policies)
				{
					writer.WriteStartElement("wsp", "Policy", WspNs);
					polElt.WriteTo(writer);
					writer.WriteEndElement();
				}
			}
			WriteWrappedElements(writer, "wsp", "Policy", WspNs,
				endpointReference.Policies);
			WriteCustomElements(writer, endpointReference);

			writer.WriteEndElement();
		}

		void WriteEndpointReferences(XmlWriter writer, string elName, string elNs,
			ICollection<EndpointReference> endpointReferences)
		{
			foreach (var endpointReference in endpointReferences)
			{
				writer.WriteStartElement(elName, elNs);
				WriteEndpointReference(writer, endpointReference);
				writer.WriteEndElement();
			}
		}

		protected virtual void WriteApplicationServiceDescriptor(XmlWriter writer, ApplicationServiceDescriptor appService)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (appService == null)
			{
				throw new ArgumentNullException(nameof(appService));
			}
			writer.WriteStartElement("RoleDescriptor", Saml2MetadataNs);
			writer.WriteAttributeString("xsi", "type", XsiNs, "fed:ApplicationServiceType");
			writer.WriteAttributeString("xmlns", "fed", null, FedNs);

			WriteWebServiceDescriptorAttributes(writer, appService);
			WriteCustomAttributes(writer, appService);

			WriteWebServiceDescriptorElements(writer, appService);

			WriteEndpointReferences(writer, "ApplicationServiceEndpoint",
				FedNs, appService.Endpoints);
			WriteEndpointReferences(writer, "SingleSignOutNotificationEndpoint",
				FedNs, appService.SingleSignOutEndpoints);
			WriteEndpointReferences(writer, "PassiveRequestorEndpoint",
				FedNs, appService.PassiveRequestorEndpoints);
			writer.WriteEndElement();
		}

		static void WriteStringElementIfPresent(XmlWriter writer, string elName,
			string elNs, string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				writer.WriteElementString(elName, elNs, value);
			}
		}

		static void WriteBase64Element(XmlWriter writer, string elName,
			string elNs, byte[] value)
		{
			if (value != null)
			{
				writer.WriteElementString(elName, elNs, Convert.ToBase64String(value));
			}
		}

		static void WriteStringAttributeIfPresent(XmlWriter writer, string attName,
			string attNs, string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				writer.WriteAttributeString(attName, attNs, value);
			}
		}

		static void WriteUriAttributeIfPresent(XmlWriter writer, string attName,
			string attNs, Uri value)
		{
			if (value != null)
			{
				writer.WriteAttributeString(attName, attNs, value.ToString());
			}
		}

		static void WriteBooleanAttribute(XmlWriter writer, string attName,
			string attNs, bool? value)
		{
			if (value.HasValue)
			{
				writer.WriteAttributeString(attName, attNs, value.Value ? "true" : "false");
			}
		}

		static void WriteStringElements(XmlWriter writer, string elName, string elNs,
			IEnumerable<string> values)
		{
			foreach (string value in values)
			{
				writer.WriteElementString(elName, elNs, value);
			}
		}

		protected virtual void WriteContactPerson(XmlWriter writer, ContactPerson contactPerson)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (contactPerson == null)
			{
				throw new ArgumentNullException(nameof(contactPerson));
			}

			writer.WriteStartElement("ContactPerson", Saml2MetadataNs);
			writer.WriteAttributeString("contactType", ContactTypeHelpers.ToString(contactPerson.Type));
			WriteCustomAttributes(writer, contactPerson);
			WriteWrappedElements(writer, null, "Extensions", Saml2MetadataNs,
				contactPerson.Extensions);
			WriteStringElementIfPresent(writer, "Company", Saml2MetadataNs, contactPerson.Company);
			WriteStringElementIfPresent(writer, "GivenName", Saml2MetadataNs, contactPerson.GivenName);
			WriteStringElementIfPresent(writer, "SurName", Saml2MetadataNs, contactPerson.Surname);
			WriteStringElements(writer, "EmailAddress", Saml2MetadataNs, contactPerson.EmailAddresses);
			WriteStringElements(writer, "TelephoneNumber", Saml2MetadataNs, contactPerson.TelephoneNumbers);
			WriteCustomElements(writer, contactPerson);

			writer.WriteEndElement();
		}

		protected virtual void WriteCustomAttributes<T>(XmlWriter writer, T source)
		{
		}

		protected virtual void WriteCustomElements<T>(XmlWriter writer, T source)
		{
		}

		protected virtual void WriteEndpointAttributes(XmlWriter writer, ProtocolEndpoint endpoint)
		{
			writer.WriteAttributeString("Binding", endpoint.Binding.ToString());
			writer.WriteAttributeString("Location", endpoint.Location.ToString());
			WriteUriAttributeIfPresent(writer, "ResponseLocation", null, endpoint.ResponseLocation);
			WriteCustomAttributes(writer, endpoint);
		}

		protected virtual void WriteEndpoint(XmlWriter writer, ProtocolEndpoint endpoint,
			string name, string ns)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (endpoint == null)
			{
				throw new ArgumentNullException(nameof(endpoint));
			}
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}
			if (ns == null)
			{
				throw new ArgumentNullException(nameof(ns));
			}
			writer.WriteStartElement(name, ns);
			WriteEndpointAttributes(writer, endpoint);
			WriteCustomAttributes(writer, endpoint);
			WriteCustomElements(writer, endpoint);
			writer.WriteEndElement();
		}

		protected virtual void WriteIndexedEndpoint(XmlWriter writer, IndexedEndpoint endpoint,
			string name, string ns)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (endpoint == null)
			{
				throw new ArgumentNullException(nameof(endpoint));
			}
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}
			if (ns == null)
			{
				throw new ArgumentNullException(nameof(ns));
			}
			writer.WriteStartElement(name, ns);
			WriteEndpointAttributes(writer, endpoint);
			WriteBooleanAttribute(writer, "isDefault", null, endpoint.IsDefault);
			writer.WriteAttributeString("index", endpoint.Index.ToString());
			WriteCustomAttributes(writer, endpoint);
			WriteCustomElements(writer, endpoint);
			writer.WriteEndElement();
		}

		protected virtual void WriteEndpoints(XmlWriter writer,
			IEnumerable<ProtocolEndpoint> endpoints, string name, string ns) =>
				WriteCollection(writer, endpoints, (writer_, endpoint) =>
					WriteEndpoint(writer_, endpoint, name, ns));

		protected virtual void WriteIndexedEndpoints(XmlWriter writer,
			IEnumerable<IndexedEndpoint> endpoints, string name, string ns) =>
				WriteCollection(writer, endpoints, (writer_, endpoint) =>
					WriteIndexedEndpoint(writer_, endpoint, name, ns));

		static void WriteStringElement(XmlWriter writer, string elName, string elNs, string value)
		{
			if (!String.IsNullOrEmpty(value))
			{
				writer.WriteStartElement(elName, elNs);
				writer.WriteString(value);
				writer.WriteEndElement();
			}
		}

		void WriteXEncKeySize(XmlWriter writer, int keySize)
		{
			writer.WriteStartElement("KeySize", XEncNs);
			writer.WriteString(keySize.ToString());
			writer.WriteEndElement();
		}

		// md:EncryptionMethod and xenc:EncryptionMethod are virtually identical, but have different namespaces
		// md:EncryptionMethod is not well defined so I've kept it as a separate type for now
		protected virtual void WriteXEncEncryptionMethod(XmlWriter writer, XEncEncryptionMethod method)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (method == null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			writer.WriteStartElement("EncryptionMethod", XEncNs);
			writer.WriteAttributeString("Algorithm", method.Algorithm.ToString());
			WriteCustomAttributes(writer, method);

			if (method.KeySize != 0)
			{
				WriteXEncKeySize(writer, method.KeySize);
			}
			WriteBase64Element(writer, "OAEPparams", XEncNs, method.OAEPparams);

			WriteCustomElements(writer, method);
			writer.WriteEndElement();
		}

		protected virtual void WriteEncryptionMethod(XmlWriter writer, EncryptionMethod method)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (method == null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			writer.WriteStartElement("EncryptionMethod", Saml2MetadataNs);
			writer.WriteAttributeString("Algorithm", method.Algorithm.ToString());
			WriteCustomAttributes(writer, method);

			if (method.KeySize != 0)
			{
				WriteXEncKeySize(writer, method.KeySize);
			}
			WriteBase64Element(writer, "OAEPparams", XEncNs, method.OAEPparams);

			WriteCustomElements(writer, method);
			writer.WriteEndElement();
		}

		void WriteCollection<T>(XmlWriter writer, IEnumerable<T> elts, Action<XmlWriter, T> writeHandler)
		{
			foreach (var elt in elts)
			{
				writeHandler(writer, elt);
			}
		}

		protected virtual void WriteRSAKeyValue(XmlWriter writer, RsaKeyValue rsa)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (rsa == null)
			{
				throw new ArgumentNullException(nameof(rsa));
			}

			writer.WriteStartElement("RSAKeyValue", DSigNs);
			WriteCustomAttributes(writer, rsa);

			WriteBase64Element(writer, "Modulus", DSigNs, rsa.Parameters.Modulus);
			WriteBase64Element(writer, "Exponent", DSigNs, rsa.Parameters.Exponent);

			WriteCustomElements(writer, rsa);
			writer.WriteEndElement();
		}

		static byte[] GetIntAsBigEndian(int value)
		{
			byte[] data = new byte[4];
			data[0] = (byte)(((uint)value >> 24) & 0xff);
			data[1] = (byte)(((uint)value >> 16) & 0xff);
			data[2] = (byte)(((uint)value >> 8) & 0xff);
			data[3] = (byte)((uint)value & 0xff);
			return data;
		}

		protected virtual void WriteDSAKeyValue(XmlWriter writer, DsaKeyValue dsa)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (dsa == null)
			{
				throw new ArgumentNullException(nameof(dsa));
			}

			writer.WriteStartElement("DSAKeyValue", DSigNs);
			WriteCustomAttributes(writer, dsa);

			WriteBase64Element(writer, "P", DSigNs, dsa.Parameters.P);
			WriteBase64Element(writer, "Q", DSigNs, dsa.Parameters.Q);
			WriteBase64Element(writer, "G", DSigNs, dsa.Parameters.G);
			WriteBase64Element(writer, "Y", DSigNs, dsa.Parameters.Y);
			WriteBase64Element(writer, "J", DSigNs, dsa.Parameters.J);
			WriteBase64Element(writer, "Seed", DSigNs, dsa.Parameters.Seed);
			WriteBase64Element(writer, "PgenCounter", DSigNs,
				GetIntAsBigEndian(dsa.Parameters.Counter));

			WriteCustomElements(writer, dsa);
			writer.WriteEndElement();
		}

		protected virtual void WriteECKeyValue(XmlWriter writer, EcKeyValue ec)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (ec == null)
			{
				throw new ArgumentNullException(nameof(ec));
			}

			writer.WriteStartElement("ECKeyValue", DSig11Ns);
			WriteCustomAttributes(writer, ec);

			writer.WriteStartElement("NamedCurve", DSig11Ns);
			writer.WriteAttributeString("URI", "urn:oid:" + ec.Parameters.Curve.Oid.Value);
			writer.WriteEndElement();

			byte[] data = new byte[ec.Parameters.Q.X.Length + ec.Parameters.Q.Y.Length + 1];
			data[0] = 4;
			Array.Copy(ec.Parameters.Q.X, 0, data, 1, ec.Parameters.Q.X.Length);
			Array.Copy(ec.Parameters.Q.Y, 0, data, 1 + ec.Parameters.Q.X.Length,
				ec.Parameters.Q.Y.Length);
			WriteBase64Element(writer, "PublicKey", DSig11Ns, data);

			WriteCustomElements(writer, ec);
			writer.WriteEndElement();
		}

		protected virtual void WriteKeyValue(XmlWriter writer, KeyValue keyValue)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (keyValue == null)
			{
				throw new ArgumentNullException(nameof(keyValue));
			}
			writer.WriteStartElement("KeyValue", DSigNs);
			WriteCustomAttributes(writer, keyValue);
			if (keyValue is RsaKeyValue rsa)
			{
				WriteRSAKeyValue(writer, rsa);
			}
			else if (keyValue is DsaKeyValue dsa)
			{
				WriteDSAKeyValue(writer, dsa);
			}
			else if (keyValue is EcKeyValue ec)
			{
				WriteECKeyValue(writer, ec);
			}
			WriteCustomElements(writer, keyValue);
			writer.WriteEndElement();
		}

		protected virtual void WriteRetrievalMethod(XmlWriter writer, RetrievalMethod method)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (method == null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			writer.WriteStartElement("RetrievalMethod", DSigNs);
			WriteUriAttributeIfPresent(writer, "URI", null, method.Uri);
			WriteUriAttributeIfPresent(writer, "Type", null, method.Type);
			WriteCustomAttributes(writer, method);

			WriteWrappedElements(writer, "ds", "Transforms", DSigNs, method.Transforms); 

			WriteCustomElements(writer, method);
			writer.WriteEndElement();
		}

		protected virtual void WriteX509IssuerSerial(XmlWriter writer, X509IssuerSerial issuerSerial)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (issuerSerial == null)
			{
				throw new ArgumentNullException(nameof(issuerSerial));
			}

			writer.WriteStartElement("X509IssuerSerial", DSigNs);
			WriteCustomAttributes(writer, issuerSerial);
			WriteStringElement(writer, "X509IssuerName", DSigNs, issuerSerial.Name);
			WriteStringElement(writer, "X509SerialNumber", DSigNs, issuerSerial.Serial);
			WriteCustomElements(writer, issuerSerial);
			writer.WriteEndElement();
		}

		protected virtual void WriteX509Digest(XmlWriter writer, X509Digest digest)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (digest == null)
			{
				throw new ArgumentNullException(nameof(digest));
			}

			writer.WriteStartElement("X509Digest", DSigNs);
			writer.WriteAttributeString("Algorithm", digest.Algorithm.ToString());
			WriteCustomAttributes(writer, digest);
			writer.WriteBase64(digest.Value, 0, digest.Value.Length);
			WriteCustomElements(writer, digest);
			writer.WriteEndElement();
		}

		protected virtual void WriteX509Data(XmlWriter writer, X509Data data)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			writer.WriteStartElement("X509Data", DSigNs);
			WriteCustomAttributes(writer, data);
			if (data.IssuerSerial != null)
			{
				WriteX509IssuerSerial(writer, data.IssuerSerial);
			}
			if (data.SKI != null)
			{
				WriteBase64Element(writer, "X509SKI", DSigNs, data.SKI);
			}
			WriteStringElementIfPresent(writer, "X509SubjectName", DSigNs, data.SubjectName);
			foreach (var cert in data.Certificates)
			{
				WriteBase64Element(writer, "X509Certificate", DSigNs, cert.GetRawCertData());
			}
			if (data.CRL != null)
			{
				WriteBase64Element(writer, "X509CRL", DSigNs, data.CRL);
			}
			if (data.Digest != null)
			{
				WriteX509Digest(writer, data.Digest);
			}

			WriteCustomElements(writer, data);
			writer.WriteEndElement();
		}

		protected virtual void WriteKeyData(XmlWriter writer, KeyData keyData)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (keyData == null)
			{
				throw new ArgumentNullException(nameof(keyData));
			}
			if (keyData is X509Data x509Data)
			{
				WriteX509Data(writer, x509Data);
			}
		}

		protected virtual void WriteDSigKeyInfo(XmlWriter writer, DSigKeyInfo keyInfo)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (keyInfo == null)
			{
				throw new ArgumentNullException(nameof(keyInfo));
			}

			writer.WriteStartElement("KeyInfo", DSigNs);
			WriteStringAttributeIfPresent(writer, "Id", null, keyInfo.Id);
			WriteCustomAttributes(writer, keyInfo);

			WriteStringElements(writer, "KeyName", DSigNs, keyInfo.KeyNames);
			WriteCollection(writer, keyInfo.KeyValues, WriteKeyValue);
			WriteCollection(writer, keyInfo.RetrievalMethods, WriteRetrievalMethod);
			WriteCollection(writer, keyInfo.Data, WriteKeyData);
			
			WriteCustomElements(writer, keyInfo);
			writer.WriteEndElement();
		}

		protected virtual void WriteCipherReference(XmlWriter writer, CipherReference reference)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (reference == null)
			{
				throw new ArgumentNullException(nameof(reference));
			}

			writer.WriteStartElement("CipherReference", XEncNs);
			WriteUriAttributeIfPresent(writer, "URI", null, reference.Uri);
			WriteCustomAttributes(writer, reference);

			WriteWrappedElements(writer, "xenc", "Transforms", XEncNs, reference.Transforms);
			WriteCustomElements(writer, reference);
			writer.WriteEndElement();
		}

		protected virtual void WriteCipherData(XmlWriter writer, CipherData data)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			writer.WriteStartElement("CipherData", XEncNs);
			WriteCustomAttributes(writer, data);
			if (data.CipherValue != null)
			{
				WriteStringElement(writer, "CipherValue", XEncNs, data.CipherValue);
			}
			if (data.CipherReference != null)
			{
				WriteCipherReference(writer, data.CipherReference);
			}
			WriteCustomElements(writer, data);
			writer.WriteEndElement();
		}

		// <element name="EncryptionProperty" type="xenc:EncryptionPropertyType"/> 
		// 
		// <complexType name="EncryptionPropertyType" mixed="true">
		//   <choice maxOccurs="unbounded">
		//     <any namespace="##other" processContents="lax"/>
		//   </choice>
		//   <attribute name="Target" type="anyURI" use="optional"/> 
		//   <attribute name="Id" type="ID" use="optional"/> 
		//   <anyAttribute namespace="http://www.w3.org/XML/1998/namespace"/>
		// </complexType>
		protected virtual void WriteEncryptionProperty(XmlWriter writer, EncryptionProperty property)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (property == null)
			{
				throw new ArgumentNullException(nameof(property));
			}

			writer.WriteStartElement("EncryptionProperty", XEncNs);
			WriteUriAttributeIfPresent(writer, "Target", null, property.Target);
			WriteStringAttributeIfPresent(writer, "Id", null, property.Id);
			WriteCustomAttributes(writer, property);
			WriteCustomElements(writer, property);
			writer.WriteEndElement();
		}

		protected virtual void WriteEncryptionProperties(XmlWriter writer, EncryptionProperties properties)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (properties == null)
			{
				throw new ArgumentNullException(nameof(properties));
			}
			writer.WriteStartElement("EncryptionProperties", XEncNs);
			WriteStringAttributeIfPresent(writer, "Id", null, properties.Id);
			WriteCustomAttributes(writer, properties);
			WriteCollection(writer, properties.Properties, WriteEncryptionProperty);
			WriteCustomElements(writer, properties);
			writer.WriteEndElement();
		}

		protected virtual void WriteEncryptedData(XmlWriter writer, EncryptedData data)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			writer.WriteStartElement("EncryptedData", XEncNs);
			WriteStringAttributeIfPresent(writer, "Id", null, data.Id);
			WriteUriAttributeIfPresent(writer, "Type", null, data.Type);
			WriteStringAttributeIfPresent(writer, "MimeType", null, data.MimeType);
			WriteUriAttributeIfPresent(writer, "Encoding", null, data.Encoding);
			WriteCustomAttributes(writer, data);

			if (data.EncryptionMethod != null)
			{
				WriteXEncEncryptionMethod(writer, data.EncryptionMethod);
			}
			if (data.KeyInfo != null)
			{
				WriteDSigKeyInfo(writer, data.KeyInfo);
			}
			if (data.CipherData != null)
			{
				WriteCipherData(writer, data.CipherData);
			}
			if (data.EncryptionProperties != null)
			{
				WriteEncryptionProperties(writer, data.EncryptionProperties);
			}

			WriteCustomElements(writer, data);
			writer.WriteEndElement();
		}

		protected virtual void WriteEncryptedValue(XmlWriter writer, EncryptedValue value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			writer.WriteStartElement("EncryptedValue", AuthNs);
			if (value.DecryptionCondition != null)
			{
				writer.WriteAttributeString("DecryptionCondition", value.DecryptionCondition.ToString());
			}
			WriteCustomAttributes(writer, value);

			WriteEncryptedData(writer, value.EncryptedData);

			WriteCustomElements(writer, value);
			writer.WriteEndElement();
		}

		protected virtual void WriteClaimValue(XmlWriter writer, ClaimValue value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (!String.IsNullOrEmpty(value.Value) && value.StructuredValue != null)
			{
				throw new MetadataSerializationException(
					"Invalid claim value that has both Value and StructuredValue properties set");
			}
			if (value.Value == null && value.StructuredValue == null)
			{
				throw new MetadataSerializationException(
					"Invalid claim value that has neither Value nor StructuredValue properties set");
			}

			if (value.Value != null)
			{
				WriteStringElement(writer, "Value", AuthNs, value.Value);
			}
			else
			{
				writer.WriteStartElement("StructuredValue", AuthNs);
				foreach (var elt in value.StructuredValue)
				{
					elt.WriteTo(writer);
				}
				writer.WriteEndElement();
			}
		}

		protected virtual void WriteCompareConstraint(XmlWriter writer,
			ConstrainedValue.CompareConstraint constraint)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (constraint == null)
			{
				throw new ArgumentNullException(nameof(constraint));
			}

			string elName;
			switch (constraint.CompareOp)
			{
				case ConstrainedValue.CompareConstraint.CompareOperator.Lt:
					elName = "ValueLessThan";
					break;
				case ConstrainedValue.CompareConstraint.CompareOperator.Lte:
					elName = "ValueLessThanOrEqual";
					break;
				case ConstrainedValue.CompareConstraint.CompareOperator.Gt:
					elName = "ValueGreaterThan";
					break;
				case ConstrainedValue.CompareConstraint.CompareOperator.Gte:
					elName = "ValueGreaterThanOrEqual";
					break;
				default:
					throw new MetadataSerializationException(
						$"Unknown constrained value compare operator '{constraint.CompareOp}'");
			}

			writer.WriteStartElement(elName, AuthNs);
			WriteCustomAttributes(writer, constraint);
			WriteClaimValue(writer, constraint.Value);
			WriteCustomElements(writer, constraint);
			writer.WriteEndElement();
		}

		protected virtual void WriteListContraint(XmlWriter writer,
			ConstrainedValue.ListConstraint constraint)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (constraint == null)
			{
				throw new ArgumentNullException(nameof(constraint));
			}

			writer.WriteStartElement("ValueOneOf", AuthNs);
			WriteCustomAttributes(writer, constraint);
			foreach (var value in constraint.Values)
			{
				WriteClaimValue(writer, value);
			}
			WriteCustomElements(writer, constraint);
			writer.WriteEndElement();
		}

		protected virtual void WriteRangeConstraint(XmlWriter writer,
			ConstrainedValue.RangeConstraint constraint)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (constraint == null)
			{
				throw new ArgumentNullException(nameof(constraint));
			}

			writer.WriteStartElement("ValueInRange", AuthNs);
			WriteCustomAttributes(writer, constraint);
			if (constraint.UpperBound != null)
			{
				writer.WriteStartElement("ValueUpperBound", AuthNs);
				WriteClaimValue(writer, constraint.UpperBound);
				writer.WriteEndElement();
			}
			if (constraint.LowerBound != null)
			{
				writer.WriteStartElement("ValueLowerBound", AuthNs);
				WriteClaimValue(writer, constraint.LowerBound);
				writer.WriteEndElement();
			}
			WriteCustomElements(writer, constraint);
			writer.WriteEndElement();
		}

		protected virtual void WriteListConstraint(XmlWriter writer,
			ConstrainedValue.ListConstraint constraint)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (constraint == null)
			{
				throw new ArgumentNullException(nameof(constraint));
			}

			writer.WriteStartElement("ValueOneOf", AuthNs);
			WriteCustomAttributes(writer, constraint);
			foreach (var value in constraint.Values)
			{
				WriteClaimValue(writer, value);
			}
			WriteCustomElements(writer, constraint);
			writer.WriteEndElement();
		}

		protected virtual void WriteConstrainedValue(XmlWriter writer, ConstrainedValue value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			writer.WriteStartElement("ConstrainedValue", AuthNs);
			WriteBooleanAttribute(writer, "AssertConstraint", null, value.AssertConstraint);
			WriteCustomAttributes(writer, value);
			foreach (var constraint in value.Constraints)
			{
				if (constraint is ConstrainedValue.CompareConstraint cc)
				{
					WriteCompareConstraint(writer, cc);
				}
				else if (constraint is ConstrainedValue.ListConstraint lc)
				{
					WriteListConstraint(writer, lc);
				}
				else if (constraint is ConstrainedValue.RangeConstraint rc)
				{
					WriteRangeConstraint(writer, rc);
				}
				else
				{
					throw new MetadataSerializationException(
						$"Unknown constraint type '{constraint.GetType()}'");
				}
			}
			WriteCustomElements(writer, value);
			writer.WriteEndElement();
		}

		protected virtual void WriteDisplayClaim(XmlWriter writer, DisplayClaim claim)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (claim == null)
			{
				throw new ArgumentNullException(nameof(claim));
			}

			writer.WriteStartElement("ClaimType", AuthNs);
			writer.WriteAttributeString("Uri", claim.ClaimType);
			WriteBooleanAttribute(writer, "Optional", null, claim.Optional);
			WriteCustomAttributes(writer, claim);

			WriteStringElement(writer, "DisplayName", AuthNs, claim.DisplayName);
			WriteStringElement(writer, "Description", AuthNs, claim.Description);
			WriteStringElement(writer, "DisplayValue", AuthNs, claim.DisplayValue);
			WriteStringElement(writer, "Value", AuthNs, claim.Value);
			if (claim.StructuredValue != null)
			{
				writer.WriteStartElement("StructuredValue", AuthNs);
				foreach (var elt in claim.StructuredValue)
				{
					elt.WriteTo(writer);
				}
				writer.WriteEndElement();
			}
			if (claim.EncryptedValue != null)
			{
				WriteEncryptedValue(writer, claim.EncryptedValue);
			}
			if (claim.ConstrainedValue != null)
			{
				WriteConstrainedValue(writer, claim.ConstrainedValue);
			}
			WriteCustomElements(writer, claim);
			writer.WriteEndElement();
		}
        
		protected virtual void WriteEntitiesDescriptor(XmlWriter writer, EntitiesDescriptor entitiesDescriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (entitiesDescriptor == null)
			{
				throw new ArgumentNullException(nameof(entitiesDescriptor));
			}

			EnvelopedSignatureWriter signatureWriter = null;
			if (entitiesDescriptor.SigningCredentials != null)
			{
				string referenceId = Guid.NewGuid().ToString("N");
				signatureWriter = new EnvelopedSignatureWriter(writer,
					entitiesDescriptor.SigningCredentials, referenceId);
				writer = signatureWriter;
			}

			writer.WriteStartElement("EntitiesDescriptor", Saml2MetadataNs);
			WriteStringAttributeIfPresent(writer, "ID", null, entitiesDescriptor.Id);
			WriteStringAttributeIfPresent(writer, "Name", null, entitiesDescriptor.Name);
			WriteCustomAttributes(writer, entitiesDescriptor);

			if (signatureWriter != null)
			{
				signatureWriter.WriteSignature();
			}

			WriteWrappedElements(writer, null, "Extensions", Saml2MetadataNs,
				entitiesDescriptor.Extensions);

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

		protected virtual void WriteNameIDFormat(XmlWriter writer, NameIDFormat nameIDFormat)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (nameIDFormat == null)
			{
				throw new ArgumentNullException(nameof(nameIDFormat));
			}
			writer.WriteStartElement("NameIDFormat", Saml2MetadataNs);
			WriteCustomAttributes(writer, nameIDFormat);
			writer.WriteString(nameIDFormat.Uri.ToString());
			WriteCustomElements(writer, nameIDFormat);
			writer.WriteEndElement();
		}

		protected virtual void WriteAuthnAuthorityDescriptor(XmlWriter writer, AuthnAuthorityDescriptor descriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}
			writer.WriteStartElement("AuthnAuthorityDescriptor", Saml2MetadataNs);
			WriteRoleDescriptorAttributes(writer, descriptor);
			WriteCustomAttributes(writer, descriptor);
			WriteRoleDescriptorElements(writer, descriptor);

			WriteEndpoints(writer, descriptor.AuthnQueryServices,
				"AuthnQueryService", Saml2MetadataNs);
			WriteEndpoints(writer, descriptor.AssertionIdRequestServices, 
				"AssertionIDRequestService", Saml2MetadataNs);
			WriteCollection(writer, descriptor.NameIDFormats, WriteNameIDFormat);

			WriteCustomElements(writer, descriptor);
			writer.WriteEndElement();
		}

		protected virtual void WriteAttributeProfile(XmlWriter writer, AttributeProfile profile)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (profile == null)
			{
				throw new ArgumentNullException(nameof(profile));
			}
			writer.WriteStartElement("AttributeProfile", Saml2MetadataNs);
			WriteCustomAttributes(writer, profile);
			writer.WriteString(profile.Uri.ToString());
			WriteCustomElements(writer, profile);
			writer.WriteEndElement();
		}

		protected virtual void WriteAttributeAuthorityDescriptor(XmlWriter writer, AttributeAuthorityDescriptor descriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}
			writer.WriteStartElement("AttributeAuthorityDescriptor", Saml2MetadataNs);
			WriteRoleDescriptorAttributes(writer, descriptor);
			WriteCustomAttributes(writer, descriptor);
			WriteRoleDescriptorElements(writer, descriptor);

			foreach (var service in descriptor.AttributeServices)
			{
				WriteEndpoint(writer, service, "AttributeService", Saml2MetadataNs);
			}

			foreach (var ars in descriptor.AssertionIdRequestServices)
			{
				WriteEndpoint(writer, ars, "AssertionIDRequestService", Saml2MetadataNs);
			}

			foreach (var nameIDFormat in descriptor.NameIdFormats)
			{
				WriteNameIDFormat(writer, nameIDFormat);
			}

			foreach (var attributeProfile in descriptor.AttributeProfiles)
			{
				WriteAttributeProfile(writer, attributeProfile);
			}

			foreach (var attribute in descriptor.Attributes)
			{
				WriteAttribute(writer, attribute);
			}

			WriteCustomElements(writer, descriptor);
			writer.WriteEndElement();
		}

		protected virtual void WritePDPDescriptor(XmlWriter writer, PDPDescriptor descriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}
			writer.WriteStartElement("PDPDescriptor", Saml2MetadataNs);
			WriteRoleDescriptorAttributes(writer, descriptor);
			WriteCustomAttributes(writer, descriptor);

			WriteRoleDescriptorElements(writer, descriptor);
			WriteEndpoints(writer, descriptor.AuthzServices,
				"AuthzService", Saml2MetadataNs);
			WriteEndpoints(writer, descriptor.AssertionIdRequestServices,
				"AssertionIDRequestService", Saml2MetadataNs);
			foreach (var nameIdFormat in descriptor.NameIDFormats)
			{
				WriteNameIDFormat(writer, nameIdFormat);
			}

			WriteCustomElements(writer, descriptor);
			writer.WriteEndElement();
		}

		protected virtual void WriteAdditionalMetadataLocation(XmlWriter writer, AdditionalMetadataLocation location)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (location == null)
			{
				throw new ArgumentNullException(nameof(location));
			}

			writer.WriteStartElement("AdditionalMetadataLocation", Saml2MetadataNs);
			WriteCustomAttributes(writer, location);
			writer.WriteAttributeString("namespace", location.Namespace);
			writer.WriteString(location.Uri.ToString());
			WriteCustomElements(writer, location);
			writer.WriteEndElement();
		}

		protected virtual void WriteEntityDescriptor(XmlWriter writer, EntityDescriptor entityDescriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (entityDescriptor == null)
			{
				throw new ArgumentNullException(nameof(entityDescriptor));
			}

			EnvelopedSignatureWriter signatureWriter = null;
			if (entityDescriptor.SigningCredentials != null)
			{
				string referenceId = Guid.NewGuid().ToString("N");
				signatureWriter = new EnvelopedSignatureWriter(writer,
					entityDescriptor.SigningCredentials, referenceId);
			}

			writer.WriteStartElement("EntityDescriptor", Saml2MetadataNs);
			WriteStringAttributeIfPresent(writer, "ID", null, entityDescriptor.Id);
			writer.WriteAttributeString("entityID", entityDescriptor.EntityId.Id);
			WriteCustomAttributes(writer, entityDescriptor);

			if (signatureWriter != null)
			{
				signatureWriter.WriteSignature();
			}

			WriteWrappedElements(writer, null, "Extensions", Saml2MetadataNs,
				entityDescriptor.Extensions);

			foreach (var roleDescriptor in entityDescriptor.RoleDescriptors)
			{
				if (roleDescriptor is ApplicationServiceDescriptor appDescriptor)
				{
					WriteApplicationServiceDescriptor(writer, appDescriptor);
				}
				else if (roleDescriptor is SecurityTokenServiceDescriptor secDescriptor)
				{
					WriteSecurityTokenServiceDescriptor(writer, secDescriptor);
				}
				else if (roleDescriptor is IdentityProviderSingleSignOnDescriptor idpSsoDescriptor)
				{
					WriteIdpSsoDescriptor(writer, idpSsoDescriptor);
				}
				else if (roleDescriptor is SingleSignOnDescriptor2 spSsoDescriptor)
				{
					WriteSpSsoDescriptor(writer, spSsoDescriptor);
				}
				else if (roleDescriptor is AuthnAuthorityDescriptor authDescriptor)
				{
					WriteAuthnAuthorityDescriptor(writer, authDescriptor);
				}
				else if (roleDescriptor is AttributeAuthorityDescriptor attDescriptor)
				{
					WriteAttributeAuthorityDescriptor(writer, attDescriptor);
				}
				else if (roleDescriptor is PDPDescriptor pdpDescriptor)
				{
					WritePDPDescriptor(writer, pdpDescriptor);
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
			foreach (var meta in entityDescriptor.AdditionalMetadataLocations)
			{
				WriteAdditionalMetadataLocation(writer, meta);
			}
			WriteCustomElements(writer, entityDescriptor);
			writer.WriteEndElement();
		}

		protected virtual void WriteIdpSsoDescriptor(XmlWriter writer, IdentityProviderSingleSignOnDescriptor descriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			writer.WriteStartElement("IDPSSODescriptor", Saml2MetadataNs);
			WriteBooleanAttribute(writer, "WantAuthnRequestsSigned", null, descriptor.WantAuthnRequestsSigned);
			WriteSsoDescriptorAttributes(writer, descriptor);
			WriteCustomAttributes(writer, descriptor);

			WriteSsoDescriptorElements(writer, descriptor);
			WriteEndpoints(writer, descriptor.SingleSignOnServices,
				"SingleSignOnService", Saml2MetadataNs);
			WriteEndpoints(writer, descriptor.NameIDMappingServices,
				"NameIDMappingService", Saml2MetadataNs);
			WriteEndpoints(writer, descriptor.AssertionIDRequestServices,
				"AssertionIDRequestService", Saml2MetadataNs);
			foreach (var attProfile in descriptor.AttributeProfiles)
			{
				WriteAttributeProfile(writer, attProfile);
			}
			foreach (var attribute in descriptor.SupportedAttributes)
			{
				WriteAttribute(writer, attribute);
			}

			WriteCustomElements(writer, descriptor);
			writer.WriteEndElement();
		}

		protected virtual void WriteKeyDescriptor(XmlWriter writer, KeyDescriptor keyDescriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (keyDescriptor == null)
			{
				throw new ArgumentNullException(nameof(keyDescriptor));
			}

			writer.WriteStartElement("KeyDescriptor", Saml2MetadataNs);
			if (keyDescriptor.Use != KeyType.Unspecified)
			{
				string useValue;
				switch (keyDescriptor.Use)
				{
					case KeyType.Signing:
						useValue = "signing";
						break;
					case KeyType.Encryption:
						useValue = "encryption";
						break;
					default:
						throw new MetadataSerializationException(
							$"Unknown KeyType enumeration entry '{keyDescriptor.Use}'");
				}
				writer.WriteAttributeString("use", useValue);
			}
			WriteCustomAttributes(writer, keyDescriptor);

			if (keyDescriptor.KeyInfo == null) throw new MetadataSerializationException("Null key info");

		    SecurityTokenSerializer.WriteKeyIdentifier(writer, keyDescriptor.KeyInfo);

            WriteCollection(writer, keyDescriptor.EncryptionMethods, WriteEncryptionMethod);

			WriteCustomElements(writer, keyDescriptor);
			writer.WriteEndElement();
		}

		protected virtual void WriteLocalizedName(XmlWriter writer, LocalizedName name,
			string elName, string ns)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}
			if (elName == null)
			{
				throw new ArgumentNullException(nameof(elName));
			}
			if (ns == null)
			{
				throw new ArgumentNullException(nameof(ns));
			}

			writer.WriteStartElement(elName, ns);
			writer.WriteAttributeString("xml", "lang", XmlNs, name.Language.Name);
			WriteCustomAttributes(writer, name);
			writer.WriteString(name.Name);
			WriteCustomElements(writer, name);
			writer.WriteEndElement();
		}

		void WriteLocalizedNames(XmlWriter writer, IEnumerable<LocalizedName> names,
			string elName, string ns) =>
				WriteCollection(writer, names, (writer_, name) =>
					WriteLocalizedName(writer_, name, elName, ns));

		protected virtual void WriteLocalizedUri(XmlWriter writer, LocalizedUri uri,
			string name, string ns)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (uri == null)
			{
				throw new ArgumentNullException(nameof(uri));
			}
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}
			if (ns == null)
			{
				throw new ArgumentNullException(nameof(ns));
			}

			writer.WriteStartElement(name, ns);
			writer.WriteAttributeString("xml", "lang", XmlNs, uri.Language.Name);
			WriteCustomAttributes(writer, name);
			writer.WriteString(uri.Uri.ToString());
			WriteCustomElements(writer, name);
			writer.WriteEndElement();
		}

		public void WriteMetadata(Stream stream, MetadataBase metadata)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			if (metadata == null)
			{
				throw new ArgumentNullException(nameof(metadata));
			}
			using (var writer = XmlWriter.Create(stream))
			{
				WriteMetadata(writer, metadata);
			}
		}

		public void WriteMetadata(XmlWriter writer, MetadataBase metadata)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (metadata == null)
			{
				throw new ArgumentNullException(nameof(metadata));
			}
			WriteMetadataCore(writer, metadata);
		}

		protected virtual void WriteMetadataCore(XmlWriter writer, MetadataBase metadata)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (metadata == null)
			{
				throw new ArgumentNullException(nameof(metadata));
			}

			if (metadata is EntitiesDescriptor entities)
			{
				WriteEntitiesDescriptor(writer, entities);
			}
			else if (metadata is EntityDescriptor entity)
			{
				WriteEntityDescriptor(writer, entity);
			}
		}

		protected virtual void WriteOrganization(XmlWriter writer, Organization organization)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (organization == null)
			{
				throw new ArgumentNullException(nameof(organization));
			}
			if (organization.Names.Count == 0)
			{
				throw new MetadataSerializationException(
					"An organisation must have at least one Name property");
			}
			if (organization.DisplayNames.Count == 0)
			{
				throw new MetadataSerializationException(
					"An organisation must have at least one DisplayName property");
			}
			if (organization.Urls.Count == 0)
			{
				throw new MetadataSerializationException(
					"An organisation must have at least one Url property");
			}

			writer.WriteStartElement("Organization", Saml2MetadataNs);
			WriteCustomAttributes(writer, organization);

			if (organization.Extensions.Count > 0)
			{
				writer.WriteStartElement("Extensions", Saml2MetadataNs);
				foreach (var extension in organization.Extensions)
				{
					extension.WriteTo(writer);
				}
				writer.WriteEndElement();
			}
			WriteLocalizedNames(writer, organization.Names,
				"OrganizationName", Saml2MetadataNs);
			WriteLocalizedNames(writer, organization.DisplayNames,
				"OrganizationDisplayName", Saml2MetadataNs);
			WriteCollection(writer, organization.Urls, (writer_, uri) =>
				WriteLocalizedUri(writer_, uri, "OrganizationURL", Saml2MetadataNs));

			WriteCustomElements(writer, organization);
			writer.WriteEndElement();
		}

		protected virtual void WriteRoleDescriptorAttributes(XmlWriter writer, RoleDescriptor descriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			WriteStringAttributeIfPresent(writer, "ID", null, descriptor.Id);
			WriteUriAttributeIfPresent(writer, "errorURL", null, descriptor.ErrorUrl);
			string protocolsSupported = descriptor.ProtocolsSupported.Aggregate("",
				(list, uri) => $"{list}{(list == "" ? "" : " ")}{uri}");
			writer.WriteAttributeString("protocolSupportEnumeration", protocolsSupported);
			WriteCustomAttributes(writer, descriptor);
		}

		void WriteRoleDescriptorElements(XmlWriter writer, RoleDescriptor descriptor, bool writeExtensions)
		{
			if (writeExtensions)
			{
				WriteWrappedElements(writer, null, "Extensions", Saml2MetadataNs,
					descriptor.Extensions);
			}
			foreach (var kd in descriptor.Keys)
			{
				WriteKeyDescriptor(writer, kd);
			}
			if (descriptor.Organization != null)
			{
				WriteOrganization(writer, descriptor.Organization);
			}
			foreach (var contact in descriptor.Contacts)
			{
				WriteContactPerson(writer, contact);
			}
			WriteCustomElements(writer, descriptor);
		}

		protected virtual void WriteRoleDescriptorElements(XmlWriter writer, RoleDescriptor descriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}
			WriteRoleDescriptorElements(writer, descriptor, true);
		}

		protected virtual void WriteSecurityTokenServiceDescriptor(XmlWriter writer,
			SecurityTokenServiceDescriptor descriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			writer.WriteStartElement("RoleDescriptor", Saml2MetadataNs);
			writer.WriteAttributeString("xsi", "type", XsiNs, "fed:SecurityTokenServiceType");
			WriteWebServiceDescriptorAttributes(writer, descriptor);
			WriteCustomAttributes(writer, descriptor);

			WriteWebServiceDescriptorElements(writer, descriptor);
			WriteEndpointReferences(writer, "SecurityTokenServiceEndpoint",
				FedNs, descriptor.SecurityTokenServiceEndpoints);
			WriteEndpointReferences(writer, "SingleSignOutSubscriptionEndpoint",
				FedNs, descriptor.SingleSignOutSubscriptionEndpoints);
			WriteEndpointReferences(writer, "SingleSignOutNotificationEndpoint",
				FedNs, descriptor.SingleSignOutNotificationEndpoints);
			WriteEndpointReferences(writer, "PassiveRequestorEndpoint",
				FedNs, descriptor.PassiveRequestorEndpoints);

			WriteCustomElements(writer, descriptor);

			writer.WriteEndElement();
		}

		protected virtual void WriteRequestedAttribute(XmlWriter writer, RequestedAttribute attribute)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (attribute == null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (attribute == null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

			writer.WriteStartElement("RequestedAttribute", Saml2MetadataNs);
			WriteBooleanAttribute(writer, "isRequired", null, attribute.IsRequired);
			WriteAttributeAttributes(writer, attribute);
			WriteAttributeElements(writer, attribute);
			writer.WriteEndElement();
		}

		protected virtual void WriteAttributeConsumingService(XmlWriter writer, AttributeConsumingService service)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (service == null)
			{
				throw new ArgumentNullException(nameof(service));
			}

			writer.WriteStartElement("AttributeConsumingService", Saml2MetadataNs);
			writer.WriteAttributeString("index", service.Index.ToString());
			WriteBooleanAttribute(writer, "isDefault", null, service.IsDefault);
			WriteCustomAttributes(writer, service);

			WriteLocalizedNames(writer, service.ServiceNames,
				"ServiceName", Saml2MetadataNs);
			WriteLocalizedNames(writer, service.ServiceDescriptions,
				"ServiceDescription", Saml2MetadataNs);
			WriteCollection(writer, service.RequestedAttributes,
				WriteRequestedAttribute);
			WriteCustomElements(writer, service);
			writer.WriteEndElement();
		}

		protected virtual void WriteSpSsoDescriptor(XmlWriter writer, SingleSignOnDescriptor2 descriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			writer.WriteStartElement("SPSSODescriptor", Saml2MetadataNs);
			WriteBooleanAttribute(writer, "AuthnRequestsSigned", null, descriptor.AuthnRequestsSigned);
			WriteBooleanAttribute(writer, "WantAssertionsSigned", null, descriptor.WantAssertionsSigned);
			WriteSsoDescriptorAttributes(writer, descriptor);
			WriteCustomAttributes(writer, descriptor);

			if (descriptor.DiscoveryResponses.Count > 0 || descriptor.Extensions.Any())
			{
				writer.WriteStartElement("Extensions", Saml2MetadataNs);
				WriteIndexedEndpoints(writer, descriptor.DiscoveryResponses.Values,
					"DiscoveryResponse", IdpDiscNs);
				foreach (var extension in descriptor.Extensions)
				{
					extension.WriteTo(writer);
				}
				writer.WriteEndElement();
			}
			WriteSsoDescriptorElements(writer, descriptor, false);
			WriteIndexedEndpoints(writer, descriptor.AssertionConsumerServices.Values,
				"AssertionConsumerService", Saml2MetadataNs);
			WriteCollection(writer, descriptor.AttributeConsumingServices.Values,
				WriteAttributeConsumingService);

			WriteCustomElements(writer, descriptor);
			writer.WriteEndElement();
		}

		protected virtual void WriteSsoDescriptorAttributes(XmlWriter writer, SingleSignOnDescriptor descriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}
			WriteRoleDescriptorAttributes(writer, descriptor);
			WriteCustomAttributes(writer, descriptor);
		}

		void WriteSsoDescriptorElements(XmlWriter writer, SingleSignOnDescriptor descriptor, bool writeExtensions)
		{
			WriteRoleDescriptorElements(writer, descriptor, writeExtensions);
			WriteIndexedEndpoints(writer, descriptor.ArtifactResolutionServices.Values,
				"ArtifactResolutionService", Saml2MetadataNs);
			WriteEndpoints(writer, descriptor.SingleLogoutServices,
				"SingleLogoutService", Saml2MetadataNs);
			WriteEndpoints(writer, descriptor.ManageNameIDServices,
				"ManageNameIDService", Saml2MetadataNs);
			WriteCollection(writer, descriptor.NameIdentifierFormats, WriteNameIDFormat);
			WriteCustomElements(writer, descriptor);
		}

		protected virtual void WriteSsoDescriptorElements(XmlWriter writer, SingleSignOnDescriptor descriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			WriteSsoDescriptorElements(writer, descriptor, true);
		}

		void WriteUris(XmlWriter writer, string parentElementName,
			string childElementName, string ns, IEnumerable<Uri> uris)
		{
			if (!uris.Any())
			{
				return;
			}
			writer.WriteStartElement(parentElementName, ns);
			foreach (var uri in uris)
			{
				writer.WriteStartElement(childElementName, ns);
				writer.WriteAttributeString("Uri", uri.ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		protected virtual void WriteWebServiceDescriptorAttributes(XmlWriter writer, WebServiceDescriptor descriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			WriteRoleDescriptorAttributes(writer, descriptor);
			WriteStringAttributeIfPresent(writer, "ServiceDisplayName",
				null, descriptor.ServiceDisplayName);
			WriteStringAttributeIfPresent(writer, "ServiceDescription",
				null, descriptor.ServiceDescription);
		}

		void WriteDisplayClaims(XmlWriter writer, string parentName, string parentNs,
			IEnumerable<DisplayClaim> claims)
		{
			if (!claims.Any())
			{
				return;
			}

			writer.WriteStartElement(parentName, parentNs);
			WriteCollection(writer, claims, WriteDisplayClaim);
			writer.WriteEndElement();
		}

		protected virtual void WriteWebServiceDescriptorElements(XmlWriter writer, WebServiceDescriptor descriptor)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			WriteRoleDescriptorElements(writer, descriptor);

			WriteUris(writer, "LogicalServiceNamesOffered",
				"IssuerName", FedNs, descriptor.LogicalServiceNamesOffered);
			WriteUris(writer, "TokenTypesOffered",
				"TokenType", FedNs, descriptor.TokenTypesOffered);
			WriteUris(writer, "ClaimDialectsOffered",
				"ClaimDialect", FedNs, descriptor.ClaimDialectsOffered);

			WriteDisplayClaims(writer, "ClaimTypesOffered", FedNs,
				descriptor.ClaimTypesOffered);
			WriteDisplayClaims(writer, "ClaimTypesRequested", FedNs,
				descriptor.ClaimTypesRequested);

			if (descriptor.AutomaticPseudonyms.HasValue)
			{
				writer.WriteStartElement("AutomaticPseudonyms", FedNs);
				writer.WriteString(descriptor.AutomaticPseudonyms.Value ? "true" : "false");
				writer.WriteEndElement();
			}
			if (descriptor.TargetScopes.Count > 0)
			{
				writer.WriteStartElement("TargetScopes", FedNs);
				WriteCollection(writer, descriptor.TargetScopes, WriteEndpointReference);
				writer.WriteEndElement();
			}

			WriteCustomElements(writer, descriptor);
		}

		protected virtual void WriteAttributeAttributes(XmlWriter writer, Saml2Attribute attribute)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (attribute == null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

			writer.WriteAttributeString("Name", attribute.Name);
			WriteUriAttributeIfPresent(writer, "NameFormat", null, attribute.NameFormat);
			WriteStringAttributeIfPresent(writer, "FriendlyName", null, attribute.FriendlyName);
			WriteCustomAttributes(writer, attribute);
		}

		protected virtual void WriteAttributeElements(XmlWriter writer,
			Saml2Attribute attribute)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (attribute == null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

			WriteCollection(writer, attribute.Values, (writer_, value) =>
				WriteStringElement(writer, "AttributeValue", Saml2AssertionNs, value));
			WriteCustomElements(writer, attribute);
		}

		protected virtual void WriteAttribute(XmlWriter writer, Saml2Attribute attribute)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (attribute == null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

			writer.WriteStartElement("Attribute", Saml2AssertionNs);
			WriteAttributeAttributes(writer, attribute);
			WriteAttributeElements(writer, attribute);
			writer.WriteEndElement();
		}
    }
}
