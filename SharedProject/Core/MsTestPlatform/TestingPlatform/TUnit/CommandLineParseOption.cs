namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    /*
        adjusted from
        https://github.com/microsoft/testfx/blob/main/src/Platform/Microsoft.Testing.Platform/CommandLine/OptionRecord.cs
    */
    internal sealed class CommandLineParseOption
    {
        public CommandLineParseOption(string name, string[] arguments)
        {
            this.Name = name;
            this.Arguments = arguments;
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