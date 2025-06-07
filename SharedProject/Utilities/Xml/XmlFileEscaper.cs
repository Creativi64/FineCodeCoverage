namespace FineCodeCoverage.Core.Utilities
{
    internal static class XmlFileEscaper
    {
        public static string Escape(string filePath) => filePath.Replace("&", "&#38;").Replace("'", "&#39;");
    }
}
