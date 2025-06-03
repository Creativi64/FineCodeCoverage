namespace FineCodeCoverage.Engine.OpenCover
{
    internal static class CommandLineArgumentsHelper
    {
        public static string AddQuotes(string value) => $@"""{value}""";

        public static string AddEscapeQuotes(string arg) => $@"\""{arg}\""";
    }
}