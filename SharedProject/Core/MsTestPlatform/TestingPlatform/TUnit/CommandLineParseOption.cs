namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal sealed class CommandLineParseOption
    {
        public CommandLineParseOption(string name, string[] arguments)
        {
            Name = name;
            Arguments = arguments;
        }
        /// <summary>
        /// Gets the name of the option.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the arguments of the option.
        /// </summary>
        public string[] Arguments { get; }
    }
}
