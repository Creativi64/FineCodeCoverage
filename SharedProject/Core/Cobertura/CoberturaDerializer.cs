using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Engine.Cobertura
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(ICoberturaDeserializer))]
    internal class CoberturaDerializer : ICoberturaDeserializer
    {
        private readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(CoberturaReport));
        private readonly XmlReaderSettings xmlReaderSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };
        public CoberturaReport Deserialize(string xmlFile)
        {
            using (var reader = XmlReader.Create(xmlFile, xmlReaderSettings))
            {
                var report = (CoberturaReport)xmlSerializer.Deserialize(reader);
                return report;
            }
        }
    }
}
