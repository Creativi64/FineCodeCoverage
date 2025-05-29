namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal static class MsCodeCoverageRegex
    {
        public static string RegexEscapePath(string path)
        {
            return path.Replace(@"\", @"\\");
        }

        public static string RegexModuleName(string moduleName, bool isDll)
        {
            string extensionMatch = isDll ? "dll" : "(dll|exe)";
            return $".*\\\\{EscapeDots(moduleName)}\\.{extensionMatch}$";
        }

        private static string EscapeDots(string moduleName)
        {
            return moduleName.Replace(".", @"\.");
        }
    }

}
