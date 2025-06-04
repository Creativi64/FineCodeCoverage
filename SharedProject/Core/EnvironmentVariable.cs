using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Engine
{
    [Export(typeof(IEnvironmentVariable))]
    internal class EnvironmentVariable : IEnvironmentVariable
    {
        public string Get(string variable) => Environment.GetEnvironmentVariable(variable);
    }
}
