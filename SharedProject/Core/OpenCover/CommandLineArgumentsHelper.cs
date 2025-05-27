namespace FineCodeCoverage.Engine.OpenCover
{
    internal static class CommandLineArgumentsHelper
    {
        public static string AddQuotes(string value)
        {
            return $@"""{value}""";
        }

        public static string AddEscapeQuotes(string arg)
        {
            return $@"\""{arg}\""";
        }
    }
}
