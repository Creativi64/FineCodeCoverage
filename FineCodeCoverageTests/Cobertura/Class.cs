using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Cobertura
{
    [XmlRoot(ElementName = "class")]
    [ExcludeFromCodeCoverage]
#pragma warning disable CA1716
    public class Class
#pragma warning restore CA1716
    {
        [XmlArray(ElementName = "methods")]
        [XmlArrayItem(ElementName = "method")]
        public List<Method> Methods { get; set; }

        [XmlArray(ElementName = "lines")]
        [XmlArrayItem(ElementName = "line")]
        public List<Line> Lines { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "filename")]
        public string Filename { get; set; }

        [XmlAttribute(AttributeName = "line-rate")]
        public float LineRate { get; set; }

        [XmlAttribute(AttributeName = "branch-rate")]
        public float BranchRate { get; set; }

        [XmlAttribute(AttributeName = "complexity")]
        public float Complexity { get; set; }
    }
}
