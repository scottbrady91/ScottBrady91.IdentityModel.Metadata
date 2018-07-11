using System.IO;
using System.Text;
using System.Xml;
using FluentAssertions;
using ScottBrady91.IdentityModel.Metadata;
using Xunit;

namespace ScottBrady91.IdentityModel.Tests
{
    public class MetadataSerializerTests
    {
        [Fact]
        public void WhenEntityIdSupplied_ExpectCorrectNamespaceAndEntityId()
        {
            var entity = new EntityDescriptor
            {
                EntityId = new EntityId("internal")
            };

            var ser = new MetadataSerializer();
            var sb = new StringBuilder();

            string meta;
            using (var stringWriter = new StringWriter(sb))
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings {OmitXmlDeclaration = true}))
                {
                    ser.WriteMetadata(xmlWriter, entity);
                }
            }

            var metadata = sb.ToString();
            metadata.Should().NotBeNullOrEmpty();

            // TODO: Check xml
        }
    }
}