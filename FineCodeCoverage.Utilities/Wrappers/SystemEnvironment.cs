using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Utilities.Wrappers
{
    [Export(typeof(IEnvironment))]
    internal sealed class SystemEnvironment : IEnvironment
    {
        public string GetEnvironmentVariable(string variable) => Environment.GetEnvironmentVariable(variable);
    }
}
