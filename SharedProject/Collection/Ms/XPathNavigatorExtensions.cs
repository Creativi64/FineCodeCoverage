using System.Xml.XPath;

namespace FineCodeCoverage.Collection.Ms
{
    internal static class XPathNavigatorExtensions
    {
        public static bool HasChild(this XPathNavigator navigator, string elementName, string nsUri = "")
            => navigator.Clone().MoveToChild(elementName, nsUri);
    }
}
