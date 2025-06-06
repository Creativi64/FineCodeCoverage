using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FineCodeCoverage.Core.Utilities
{
    public static class LinqToXmlUtil
    {
        private class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb)
                : base(sb)
            {
            }

            public override Encoding Encoding => Encoding.UTF8;
        }

        private static readonly char[] s_pathSplitter = new[] { '/' };

        public static string ToXmlString(this XDocument xdoc)
            => xdoc.Declaration == null ? xdoc.ToString() : xdoc.Declaration + Environment.NewLine + xdoc;

        public static string FormatXml(this XDocument xDocument, bool utf8 = true)
        {
            var builder = new StringBuilder();
            StringWriter writer = utf8 ? new Utf8StringWriter(builder) : new StringWriter(builder);

            xDocument.Save(writer, SaveOptions.None);
            return builder.ToString();
        }

        public static XElement RemoveAllNamespaces(this XElement @this)
            => new XElement(
                @this.Name.LocalName,
                @this.Nodes().Select(n => n is XElement xEl ? RemoveAllNamespaces(xEl) : n),
                @this.Attributes());

        public static XElement Load(string path, bool removeNamespaces)
        {
            var xelement = XElement.Parse(File.ReadAllText(path));

            if (removeNamespaces)
            {
                xelement = xelement.RemoveAllNamespaces();
            }

            return xelement;
        }

        public static XElement GetStrictDescendant(this XContainer element, string path)
        {
            string[] names = path.Split(s_pathSplitter, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 0)
            {
                throw new ArgumentException("Empty path", nameof(path));
            }

            XContainer result = element;
            foreach (string name in names)
            {
                result = result.Element(name);
                if (result == null)
                {
                    return null;
                }
            }

            return (XElement)result;
        }
    }
}
